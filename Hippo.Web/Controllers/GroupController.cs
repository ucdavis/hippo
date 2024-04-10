using Hippo.Core.Data;
using Hippo.Core.Domain;
using Hippo.Core.Services;
using Hippo.Web.Extensions;
using Hippo.Web.Models;
using Hippo.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Hippo.Core.Models;
using EFCore.BulkExtensions;
using AccountRequest = Hippo.Core.Domain.Request;
using Hippo.Core.Extensions;

namespace Hippo.Web.Controllers;

[Authorize]
public class GroupController : SuperController
{
    private readonly AppDbContext _dbContext;
    private readonly IUserService _userService;
    private readonly INotificationService _notificationService;
    private readonly IHistoryService _historyService;

    public GroupController(AppDbContext dbContext, IUserService userService, IHistoryService historyService, INotificationService notificationService)
    {
        _dbContext = dbContext;
        _userService = userService;
        _historyService = historyService;
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<ActionResult> Groups()
    {
        if (string.IsNullOrWhiteSpace(Cluster))
        {
            return BadRequest("You must supply a cluster name.");
        }

        var currentUser = await _userService.GetCurrentUser();
        var permissions = await _userService.GetCurrentPermissionsAsync();
        var isClusterOrSystemAdmin = permissions.IsClusterOrSystemAdmin(Cluster);

        var groups = await _dbContext.Groups
            .AsNoTracking()
            .Where(g => g.Cluster.Name == Cluster)
            .OrderBy(g => g.DisplayName)
            .Select(GroupModel.GetProjection(isClusterOrSystemAdmin, currentUser.Id))
            .ToArrayAsync();

        return Ok(groups);
    }

    [Authorize(Policy = AccessCodes.ClusterAdminAccess)]
    [HttpPost]
    public async Task<IActionResult> Update([FromBody] Group group)
    {
        if (string.IsNullOrWhiteSpace(Cluster))
        {
            return BadRequest("You must supply a cluster name.");
        }

        if (group.Id == 0)
        {
            return BadRequest("You must specify an Id when updating a group.");
        }

        var existingGroup = await _dbContext.Groups
            .Where(g => g.Cluster.Name == Cluster)
            .SingleOrDefaultAsync(g => g.Id == group.Id);
        if (existingGroup == null)
        {
            return BadRequest($"Group does not exist.");
        }

        existingGroup.DisplayName = group.DisplayName;
        await _dbContext.SaveChangesAsync();
        return Ok(existingGroup);
    }

    [HttpPost]
    public async Task<IActionResult> RequestAccess([FromBody] AddToGroupModel addToGroupModel)
    {
        if (string.IsNullOrWhiteSpace(Cluster))
        {
            return BadRequest("Cluster is required");
        }

        if (addToGroupModel.GroupId == 0)
        {
            return BadRequest("Please select a group from the list.");
        }

        var currentUser = await _userService.GetCurrentUser();
        var group = await _dbContext.Groups
            .Include(g => g.Cluster)
            .SingleOrDefaultAsync(g => g.Cluster.Name == Cluster && g.Id == addToGroupModel.GroupId);
        if (group == null)
        {
            return BadRequest($"Group does not exist.");
        }

        var currentAccount = await _dbContext.Accounts
            .AsNoTracking()
            .SingleOrDefaultAsync(a =>
                a.Cluster.Name == Cluster
                && a.Owner.Iam == currentUser.Iam);

        if (currentAccount == null)
        {
            return BadRequest($"You must have an account before requesting access to additional groups.");
        }

        var hasMembership = await _dbContext.GroupMemberAccount.AnyAsync(gm =>
            gm.Group.Cluster.Name == Cluster
            && gm.GroupId == addToGroupModel.GroupId
            && gm.AccountId == currentAccount.Id);

        if (hasMembership)
        {
            return BadRequest($"You are already a member of this group.");
        }

        var alreadyRequested = await _dbContext.Requests.AnyAsync(r =>
            r.Cluster.Name == Cluster
            && r.Group == group.Name
            && r.Requester.Iam == currentUser.Iam
            && r.Status != AccountRequest.Statuses.Rejected
            && r.Status != AccountRequest.Statuses.Completed);
        if (alreadyRequested)
        {
            return BadRequest($"You have already requested access to this group.");
        }

        var request = new AccountRequest
        {
            Requester = currentUser,
            RequesterId = currentUser.Id,
            Group = group.Name,
            Action = AccountRequest.Actions.AddAccountToGroup,
            Status = AccountRequest.Statuses.PendingApproval,
            Cluster = group.Cluster,
            ClusterId = group.ClusterId,
        }
        .WithAccountRequestData(new AccountRequestDataModel
        {
            SupervisingPI = addToGroupModel.SupervisingPI,
        });

        await _dbContext.Requests.AddAsync(request);
        await _historyService.RequestCreated(request);

        await _dbContext.SaveChangesAsync();

        var success = await _notificationService.AccountRequest(request);
        if (!success)
        {
            Log.Error("Error creating Account Request email");
        }

        var requestModel = await _dbContext.Requests
            .Where(r => r.Id == request.Id)
            .SelectRequestModel(_dbContext)
            .SingleAsync();

        return Ok(requestModel);        
    }
}
