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
using HippoRequest = Hippo.Core.Domain.Request;
using Hippo.Core.Extensions;
using Microsoft.Extensions.Options;
using Hippo.Web.Models.Settings;

namespace Hippo.Web.Controllers;

[Authorize]
public class GroupController : SuperController
{
    private readonly AppDbContext _dbContext;
    private readonly IUserService _userService;
    private readonly INotificationService _notificationService;
    private readonly IHistoryService _historyService;
    private readonly IAccountUpdateService _accountUpdateService;
    private readonly FeatureFlagSettings _featureFlagSettings;

    public GroupController(AppDbContext dbContext, IUserService userService, IHistoryService historyService,
        INotificationService notificationService, IAccountUpdateService accountUpdateService, IOptions<FeatureFlagSettings> featureFlagSettings)
    {
        _dbContext = dbContext;
        _userService = userService;
        _historyService = historyService;
        _notificationService = notificationService;
        _accountUpdateService = accountUpdateService;
        _featureFlagSettings = featureFlagSettings.Value;
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
    public async Task<IActionResult> Update([FromBody] UpdateGroupModel updateGroupModel)
    {
        if (string.IsNullOrWhiteSpace(Cluster))
        {
            return BadRequest("You must supply a cluster name.");
        }

        if (updateGroupModel.Id == 0)
        {
            return BadRequest("You must specify an Id when updating a group.");
        }

        var existingGroup = await _dbContext.Groups
            .Where(g => g.Cluster.Name == Cluster)
            .SingleOrDefaultAsync(g => g.Id == updateGroupModel.Id);
        if (existingGroup == null)
        {
            return BadRequest($"Group does not exist.");
        }

        existingGroup.DisplayName = updateGroupModel.DisplayName;
        await _dbContext.SaveChangesAsync();

        var groupModel = await _dbContext.Groups
            .Where(g => g.Id == updateGroupModel.Id)
            .Select(GroupModel.GetProjection(true))
            .SingleAsync();
        return Ok(groupModel);
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
            && r.Status != HippoRequest.Statuses.Rejected
            && r.Status != HippoRequest.Statuses.Completed);
        if (alreadyRequested)
        {
            return BadRequest($"You have already requested access to this group.");
        }

        // Get user Id of the Supervising PI
        var supervisingPIUserId = 0;
        if (!string.IsNullOrWhiteSpace(addToGroupModel.SupervisingPIIamId))
        {
            var user = await _userService.GetUserByIam(addToGroupModel.SupervisingPIIamId);

            if (user == null)
            {
                return BadRequest("Supervising PI not found");
            }
            supervisingPIUserId = user.Id;
        }

        var request = new HippoRequest
        {
            Requester = currentUser,
            RequesterId = currentUser.Id,
            Group = group.Name,
            Action = HippoRequest.Actions.AddAccountToGroup,
            Status = HippoRequest.Statuses.PendingApproval,
            Cluster = group.Cluster,
            ClusterId = group.ClusterId,
        }
        .WithAccountRequestData(new AccountRequestDataModel
        {
            SupervisingPI = addToGroupModel.SupervisingPI,
            SupervisingPIUserId = supervisingPIUserId
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

    [HttpPost]
    public async Task<IActionResult> RequestCreation([FromBody] CreateGroupModel createGroupModel)
    {
        if (!_featureFlagSettings.CreateGroup)
        {
            return BadRequest("This feature is not enabled.");
        }
        
        if (string.IsNullOrWhiteSpace(Cluster))
        {
            return BadRequest("Cluster is required");
        }

        if (string.IsNullOrWhiteSpace(createGroupModel.DisplayName))
        {
            return BadRequest("Group display name is required");
        }

        var currentUser = await _userService.GetCurrentUser();
        var currentAccount = await _dbContext.Accounts
            .Include(a => a.Cluster)
            .SingleOrDefaultAsync(a =>
                a.Cluster.Name == Cluster
                && a.Owner.Iam == currentUser.Iam);
        if (currentAccount == null)
        {
            return BadRequest($"You must have an account before requesting group creation.");
        }

        var existingGroup = await _dbContext.Groups
            .Where(g => g.Cluster.Name == Cluster)
            .SingleOrDefaultAsync(g => g.Name == createGroupModel.Name);
        if (existingGroup != null)
        {
            return BadRequest("A group with this name already exists in the cluster.");
        }

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
        var alreadyRequested = await _dbContext.Requests.AnyAsync(r =>
            r.Cluster.Name == Cluster
            && r.Requester.Iam == currentUser.Iam
            && r.Action == HippoRequest.Actions.CreateGroup
            && HippoRequest.Statuses.Pending.Contains(r.Status)
            // this ugly cast is an unfortunate side effect of automapping nvarchar to a JsonElement property
            && CustomFunctions.JsonValue((string)(object)r.Data, "$.name") == createGroupModel.Name
        );
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

        if (alreadyRequested)
        {
            return BadRequest("You have already requested creation of a group with this display name.");
        }

        var request = new HippoRequest
        {
            Requester = currentUser,
            RequesterId = currentUser.Id,
            Action = HippoRequest.Actions.CreateGroup,
            Status = HippoRequest.Statuses.PendingApproval,
            Cluster = currentAccount.Cluster,
            ClusterId = currentAccount.ClusterId,
        }
        .WithCreateGroupRequestData(new GroupRequestDataModel
        {
            Name = createGroupModel.Name,
            DisplayName = createGroupModel.DisplayName
        });

        await _dbContext.Requests.AddAsync(request);
        await _historyService.RequestCreated(request);

        await _dbContext.SaveChangesAsync();

        var success = await _notificationService.GroupRequest(request);
        if (!success)
        {
            Log.Error("Error creating Create Group Request email");
        }

        var requestModel = await _dbContext.Requests
            .Where(r => r.Id == request.Id)
            .SelectRequestModel(_dbContext)
            .SingleAsync();

        return Ok(requestModel);
    }

    [Authorize(Policy = AccessCodes.GroupAdminAccess)]
    [HttpPost]
    public async Task<IActionResult> RequestRemoveMember([FromBody] GroupMemberModel groupMemberModel)
    {
        if (!_featureFlagSettings.RemoveAccountFromGroup)
        {
            return BadRequest("This feature is not enabled.");
        }

        if (string.IsNullOrWhiteSpace(Cluster))
        {
            return BadRequest("Cluster is required");
        }

        var currentUser = await _userService.GetCurrentUser();
        var group = await _dbContext.Groups
            .AsSplitQuery()
            //limit member accouts to just the one we're interested in...
            .Include(g => g.MemberAccounts.Where(a => a.Id == groupMemberModel.AccountId))
                .ThenInclude(a => a.Owner)
            //it's ugly, but duplicate includes are the only way to do sibling ThenIncludes
            .Include(g => g.MemberAccounts.Where(a => a.Id == groupMemberModel.AccountId))
                .ThenInclude(a => a.Cluster)
            .Include(g => g.AdminAccounts.Where(a => a.OwnerId == currentUser.Id))
            .SingleOrDefaultAsync(g => g.Cluster.Name == Cluster && g.Id == groupMemberModel.GroupId);
        if (group == null)
        {
            return BadRequest("Group does not exist");
        }

        // ensure user is a system, cluster or group admin
        var permissions = await _userService.GetCurrentPermissionsAsync();
        var isSystemOrClusterAdmin = permissions.IsClusterOrSystemAdmin(Cluster);
        var isAdmin = isSystemOrClusterAdmin || group.AdminAccounts.Any(a => a.OwnerId == currentUser.Id);
        if (!isAdmin)
        {
            return Forbid();
        }

        var accountToRemove = group.MemberAccounts.SingleOrDefault(a => a.Id == groupMemberModel.AccountId);
        if (accountToRemove == null)
        {
            return BadRequest("Account is not a member of group");
        }

        var result = await _accountUpdateService.QueueRemoveGroupMember(accountToRemove, group);
        if (result.IsError)
        {
            return BadRequest($"Error queuing {QueuedEvent.Actions.RemoveAccountFromGroup} message: {result.Message}");
        }

        return Ok();
    }
}
