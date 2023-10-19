using Hippo.Core.Data;
using Hippo.Core.Domain;
using Hippo.Core.Services;
using Hippo.Web.Extensions;
using Hippo.Web.Models;
using Hippo.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using Serilog;
using Hippo.Core.Models;
using AccountRequest = Hippo.Core.Domain.Request;

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

        var requests = await _dbContext.Requests
            .AsNoTracking()
            .Where(r => r.Status == AccountRequest.Statuses.PendingApproval)
            .CanAccess(_dbContext, Cluster, currentUser.Iam)
            // the projection is sorting groups by name, so we'll sort by the first group name
            .OrderBy(a => a.Group.Name)
                .ThenBy(a => a.Account.Name)
            .Select(RequestModel.Projection)
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

        var request = await _dbContext.Requests
            .IgnoreQueryFilters()
            .AsSplitQuery()
            .Where(r => r.Id == id && r.Status == AccountRequest.Statuses.PendingApproval)
            .CanAccess(_dbContext, Cluster, currentUser.Iam)
            .Include(r => r.Account.Owner)
            .Include(r => r.Account.Cluster)
            .Include(r => r.Group)
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
        var permissions = await _userService.GetCurrentPermissionsAsync();
        var isClusterOrSystemAdmin = permissions.IsClusterOrSystemAdmin(Cluster);
        var isGroupAdmin = permissions.IsGroupAdmin(Cluster, request.GroupId);

        if (!isClusterOrSystemAdmin && !isGroupAdmin)
        {
            return Forbid();
        }

        if (request.Account == null)
        {
            return BadRequest("No account associated with this request");
        }

        if (request.Group == null)
        {
            return BadRequest("No group associated with this request");
        }        

        if (!await _accountUpdateService.UpdateAccount(request.Account, request.Group))
        {
            // It could be that ssh is down
            return BadRequest("Error updating account. Please try again later.");
        }

        request.Account.Status = Account.Statuses.Active;
        // TODO: Should request status be set to Processing, and have the AccountSyncService set it to active when confirmed?
        request.Status = AccountRequest.Statuses.Completed;


        var success = await _notificationService.AccountDecision(request, true);
        if (!success)
        {
            Log.Error("Error creating Account Decision email");
        }

        // safe to assume admin override if no GroupAdmin permission is found
        await _historyService.AccountApproved(request.Account, !isGroupAdmin);

        await _dbContext.SaveChangesAsync();

        return Ok();
    }

    private async Task<ActionResult> ApproveAddAccountToGroup(AccountRequest request)
    {
        var permissions = await _userService.GetCurrentPermissionsAsync();
        var isClusterOrSystemAdmin = permissions.IsClusterOrSystemAdmin(Cluster);
        var isGroupAdmin = permissions.IsGroupAdmin(Cluster, request.GroupId);

        if (!isClusterOrSystemAdmin && !isGroupAdmin)
        {
            return Forbid();
        }

        if (request.Account == null)
        {
            return BadRequest("No account associated with this request");
        }

        if (request.Group == null)
        {
            return BadRequest("No group associated with this request");
        }

        if (!await _accountUpdateService.UpdateAccount(request.Account, request.Group))
        {
            // It could be that ssh is down
            return BadRequest("Error updating account. Please try again later.");
        }

        request.Account.Status = Account.Statuses.Active;
        // TODO: Should request status be set to Processing, and have the AccountSyncService set it to active when confirmed?
        request.Status = AccountRequest.Statuses.Completed;

        var success = await _notificationService.AccountDecision(request, true);
        if (!success)
        {
            Log.Error("Error creating Account Decision email");
        }

        // safe to assume admin override if no GroupAdmin permission is found
        await _historyService.AccountApproved(request.Account, !isGroupAdmin);

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

        var request = await _dbContext.Requests
            .IgnoreQueryFilters()
            .Where(r => r.Id == id && r.Status == AccountRequest.Statuses.PendingApproval)
            .CanAccess(_dbContext, Cluster, currentUser.Iam)
            .Include(r => r.Account.Owner)
            .Include(r => r.Account.Cluster)
            .Include(r => r.Group)
            .SingleOrDefaultAsync();

        if (request == null)
        {
            return NotFound();
        }

        var permissions = await _userService.GetCurrentPermissionsAsync();
        var isClusterOrSystemAdmin = permissions.IsClusterOrSystemAdmin(Cluster);
        var isGroupAdmin = permissions.IsGroupAdmin(Cluster, request.GroupId);

        if (!isClusterOrSystemAdmin && !isGroupAdmin)
        {
            return Forbid();
        }

        var account = request.Account;

        if (account == null)
        {
            return BadRequest("No account associated with this request");
        }

        account.Status = Account.Statuses.Rejected;
        account.IsActive = false;
        request.Status = AccountRequest.Statuses.Rejected;

        var success = await _notificationService.AccountDecision(request, false, reason: model.Reason);
        if (!success)
        {
            Log.Error("Error creating Account Decision email");
        }

        // safe to assume admin override if no GroupAdmin permission is found
        await _historyService.AccountRejected(account, !isGroupAdmin, model.Reason);


        await _dbContext.SaveChangesAsync();

        return Ok();
    }
}
