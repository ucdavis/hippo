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
using AccountRequest = Hippo.Core.Domain.Request;
using Hippo.Core.Extensions;

namespace Hippo.Web.Controllers;

[Authorize]
public class RequestController : SuperController
{
    private readonly AppDbContext _dbContext;
    private readonly IUserService _userService;
    private readonly INotificationService _notificationService;
    private readonly IHistoryService _historyService;
    private readonly IAccountUpdateService _accountUpdateService;

    public RequestController(AppDbContext dbContext, IUserService userService, INotificationService notificationService,
        IHistoryService historyService, IAccountUpdateService accountUpdateService)
    {
        _dbContext = dbContext;
        _userService = userService;
        _notificationService = notificationService;
        _historyService = historyService;
        _accountUpdateService = accountUpdateService;
    }

    // Return all requests that are waiting for the current user to approve
    [HttpGet]
    [Authorize(Policy = AccessCodes.GroupAdminAccess)]
    public async Task<ActionResult> Pending()
    {
        var currentUser = await _userService.GetCurrentUser();
        if (string.IsNullOrWhiteSpace(Cluster))
        {
            return BadRequest("Cluster is required");
        }

        var permissions = await _userService.GetCurrentPermissionsAsync();
        var isClusterOrSystemAdmin = permissions.IsClusterOrSystemAdmin(Cluster);

        var requests = await _dbContext.Requests
            .AsNoTracking()
            .Where(r => r.Status == AccountRequest.Statuses.PendingApproval)
            .CanAccess(_dbContext, Cluster, currentUser.Iam, isClusterOrSystemAdmin)
            // the projection is sorting groups by name, so we'll sort by the first group name
            .OrderBy(r => r.Group)
                .ThenBy(r => r.Requester.FirstName)
                    .ThenBy(r => r.Requester.LastName)
            .SelectRequestModel(_dbContext)
            .ToArrayAsync();

        return Ok(requests);
    }

    // Approve a given pending account if you are the sponsor
    [HttpPost]
    [Authorize(Policy = AccessCodes.GroupAdminAccess)]
    public async Task<ActionResult> Approve(int id)
    {
        var currentUser = await _userService.GetCurrentUser();
        if (string.IsNullOrWhiteSpace(Cluster))
        {
            return BadRequest("Cluster is required");
        }

        var permissions = await _userService.GetCurrentPermissionsAsync();
        var isClusterOrSystemAdmin = permissions.IsClusterOrSystemAdmin(Cluster);

        var request = await _dbContext.Requests
            .IgnoreQueryFilters()
            .AsSplitQuery()
            .Where(r => r.Id == id && r.Status == AccountRequest.Statuses.PendingApproval)
            .CanAccess(_dbContext, Cluster, currentUser.Iam, isClusterOrSystemAdmin)
            .Include(r => r.Requester)
            .Include(r => r.Cluster)
            .SingleOrDefaultAsync();

        if (request == null)
        {
            return NotFound();
        }

        var result = request.Action switch
        {
            AccountRequest.Actions.CreateAccount => await ApproveCreateAccount(request),
            AccountRequest.Actions.AddAccountToGroup => await ApproveAddAccountToGroup(request),
            _ => BadRequest("Invalid action")
        };

        return result;
    }

    private async Task<ActionResult> ApproveCreateAccount(AccountRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Group))
        {
            return BadRequest("No group associated with this request");
        }

        var group = await _dbContext.Groups.SingleAsync(g => g.ClusterId == request.ClusterId && g.Name == request.Group);
        var currentUser = await _userService.GetCurrentUser();
        var permissions = await _userService.GetCurrentPermissionsAsync();
        var isClusterOrSystemAdmin = permissions.IsClusterOrSystemAdmin(Cluster);
        var isGroupAdmin = await _dbContext.GroupAdminAccount.AnyAsync(ga =>
            ga.GroupId == group.Id
            && ga.Group.AdminAccounts.Any(aa => aa.OwnerId == currentUser.Id));

        if (!isClusterOrSystemAdmin && !isGroupAdmin)
        {
            return Forbid();
        }

        var result = await _accountUpdateService.QueueCreateAccount(group, request);
        if (result.IsError)
        {
            return BadRequest($"Error queuing {QueuedEvent.Actions.CreateAccount} request: {result.Message}");
        }

        request.Status = AccountRequest.Statuses.Processing;
        request.UpdatedOn = DateTime.UtcNow;

        var success = await _notificationService.AccountDecision(request, true, decidedBy: currentUser.Name,
            reason: "Your account request has been approved. You will receive another email with more details once your " +
            $"account is created on {Cluster}. You can check the \"My Account\" tab of Hippo to see what " +
            "resources you have access to.");
        if (!success)
        {
            Log.Error("Error creating Account Decision email");
        }

        // safe to assume admin override if no GroupAdmin permission is found
        await _historyService.RequestApproved(request, !isGroupAdmin);

        await _dbContext.SaveChangesAsync();

        return Ok();
    }

    private async Task<ActionResult> ApproveAddAccountToGroup(AccountRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Group))
        {
            return BadRequest("No group associated with this request");
        }

        var group = await _dbContext.Groups.SingleAsync(g => g.ClusterId == request.ClusterId && g.Name == request.Group);
        var currentUser = await _userService.GetCurrentUser();
        var permissions = await _userService.GetCurrentPermissionsAsync();
        var isClusterOrSystemAdmin = permissions.IsClusterOrSystemAdmin(Cluster);
        var isGroupAdmin = await _dbContext.GroupAdminAccount.AnyAsync(ga =>
            ga.GroupId == group.Id
            && ga.Group.AdminAccounts.Any(aa => aa.OwnerId == currentUser.Id));

        if (!isClusterOrSystemAdmin && !isGroupAdmin)
        {
            return Forbid();
        }

        var account = await _dbContext.Accounts
            .Where(a => a.ClusterId == request.ClusterId && a.OwnerId == request.RequesterId)
            .SingleOrDefaultAsync();

        if (account == null)
        {
            return BadRequest("No account found for this request");
        }

        var result = await _accountUpdateService.QueueAddAccountToGroup(account, group, request);
        if (result.IsError)
        {
            return BadRequest($"Error queuing {QueuedEvent.Actions.AddAccountToGroup} request: {result.Message}");
        }

        request.Status = AccountRequest.Statuses.Processing;
        request.UpdatedOn = DateTime.UtcNow;

        var success = await _notificationService.AccountDecision(request, true, currentUser.Name);
        if (!success)
        {
            Log.Error("Error creating Account Decision email");
        }

        // safe to assume admin override if no GroupAdmin permission is found
        await _historyService.RequestApproved(request, !isGroupAdmin);

        await _dbContext.SaveChangesAsync();

        return Ok();
    }

    [HttpPost]
    [Authorize(Policy = AccessCodes.GroupAdminAccess)]
    public async Task<ActionResult> Reject(int id, [FromBody] RequestRejectionModel model)
    {
        if (string.IsNullOrWhiteSpace(Cluster))
        {
            return BadRequest("Cluster is required");
        }
        if (String.IsNullOrWhiteSpace(model.Reason))
        {
            return BadRequest("Missing Reject Reason");
        }

        var currentUser = await _userService.GetCurrentUser();
        var permissions = await _userService.GetCurrentPermissionsAsync();
        var isClusterOrSystemAdmin = permissions.IsClusterOrSystemAdmin(Cluster);

        var request = await _dbContext.Requests
            .IgnoreQueryFilters()
            .Where(r => r.Id == id && r.Status == AccountRequest.Statuses.PendingApproval)
            .CanAccess(_dbContext, Cluster, currentUser.Iam, isClusterOrSystemAdmin)
            .Include(r => r.Requester)
            .Include(r => r.Cluster)
            .SingleOrDefaultAsync();

        if (request == null)
        {
            return NotFound();
        }

        var group = await _dbContext.Groups.SingleAsync(g => g.ClusterId == request.ClusterId && g.Name == request.Group);
        var isGroupAdmin = await _dbContext.GroupAdminAccount.AnyAsync(ga =>
            ga.GroupId == group.Id
            && ga.Group.AdminAccounts.Any(aa => aa.OwnerId == currentUser.Id));

        if (!isClusterOrSystemAdmin && !isGroupAdmin)
        {
            return Forbid();
        }

        request.Status = AccountRequest.Statuses.Rejected;
        request.UpdatedOn = DateTime.UtcNow;

        var success = await _notificationService.AccountDecision(request, false, decidedBy: currentUser.Name, reason: model.Reason);
        if (!success)
        {
            Log.Error("Error creating Account Decision email");
        }

        // safe to assume admin override if no GroupAdmin permission is found
        await _historyService.RequestRejected(request, !isGroupAdmin, model.Reason);

        await _dbContext.SaveChangesAsync();

        return Ok();
    }
}
