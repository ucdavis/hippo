using Hippo.Core.Data;
using Hippo.Core.Domain;
using Hippo.Core.Services;
using Hippo.Web.Extensions;
using Hippo.Web.Models;
using Hippo.Web.Services;
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
    private readonly ISshService _sshService;
    private readonly INotificationService _notificationService;
    private readonly IHistoryService _historyService;
    private readonly IYamlService _yamlService;

    public RequestController(AppDbContext dbContext, IUserService userService, ISshService sshService, INotificationService notificationService, IHistoryService historyService, IYamlService yamlService)
    {
        _dbContext = dbContext;
        _userService = userService;
        _sshService = sshService;
        _notificationService = notificationService;
        _historyService = historyService;
        _yamlService = yamlService;
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
            .Where(r => r.Status == AccountRequest.StatusValues.PendingApproval)
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
            .Where(r => r.Id == id && r.Status == AccountRequest.StatusValues.PendingApproval)
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

        var connectionInfo = await _dbContext.Clusters.GetSshConnectionInfo(Cluster);

        var tempFileName = $"/var/lib/remote-api/.{account.Owner.Kerberos}.yaml"; //Leading .
        var fileName = $"/var/lib/remote-api/{account.Owner.Kerberos}.yaml";

        await _sshService.PlaceFile(account.AccountYaml, tempFileName, connectionInfo);
        await _sshService.RenameFile(tempFileName, fileName, connectionInfo);

        account.Status = Account.Statuses.Active;

        var success = await _notificationService.AccountDecision(request, true);
        if (!success)
        {
            Log.Error("Error creating Account Decision email");
        }

        // safe to assume admin override if no GroupAdmin permission is found
        await _historyService.AccountApproved(account, !isGroupAdmin);

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
            .Where(r => r.Id == id && r.Status == AccountRequest.StatusValues.PendingApproval)
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
