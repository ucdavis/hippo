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

namespace Hippo.Web.Controllers;

[Authorize]
public class AccountController : SuperController
{
    private readonly AppDbContext _dbContext;
    private readonly IUserService _userService;
    private readonly ISshService _sshService;
    private readonly INotificationService _notificationService;
    private readonly IHistoryService _historyService;
    private readonly IYamlService _yamlService;

    public AccountController(AppDbContext dbContext, IUserService userService, ISshService sshService, INotificationService notificationService, IHistoryService historyService, IYamlService yamlService)
    {
        _dbContext = dbContext;
        _userService = userService;
        _sshService = sshService;
        _notificationService = notificationService;
        _historyService = historyService;
        _yamlService = yamlService;
    }

    // Return account info for the currently logged in user
    [HttpGet]
    public async Task<ActionResult> Get()
    {
        if (string.IsNullOrWhiteSpace(Cluster))
        {
            return BadRequest("Cluster is required");
        }
        var currentUser = await _userService.GetCurrentUser();

        return Ok(await _dbContext.Accounts
            .InCluster(Cluster)
            .Where(a => a.Owner.Iam == currentUser.Iam)
            .Select(AccountModel.Projection)
            .ToArrayAsync());
    }

    // Return all accounts that are waiting for the current user to approve
    [HttpGet]
    [Authorize(Policy = AccessCodes.GroupAdminAccess)]
    public async Task<ActionResult> Pending()
    {
        var currentUser = await _userService.GetCurrentUser();
        if (string.IsNullOrWhiteSpace(Cluster))
        {
            return BadRequest("Cluster is required");
        }

        return Ok(await _dbContext.Accounts
            .AsNoTracking()
            .PendingApproval()
            .CanAccess(_dbContext, Cluster, currentUser.Iam)
            // the projection is sorting groups by name, so we'll sort by the first group name
            .OrderBy(a => a.GroupAccounts.OrderBy(ga => ga.Group.Name).First().Group.Name)
                .ThenBy(a => a.Name)
            .Select(AccountModel.Projection)
            .ToArrayAsync());
    }

    [HttpGet]
    [Authorize(Policy = AccessCodes.GroupAdminAccess)]
    public async Task<ActionResult> Active()
    {
        var currentUser = await _userService.GetCurrentUser();
        if (string.IsNullOrWhiteSpace(Cluster))
        {
            return BadRequest("Cluster is required");
        }

        return Ok(await _dbContext.Accounts
            .AsNoTracking()
            .Where(a => a.Status == Account.Statuses.Active)
            .CanAccess(_dbContext, Cluster, currentUser.Iam)
            // the projection is sorting groups by name, so we'll sort by the first group name
            .OrderBy(a => a.GroupAccounts.OrderBy(ga => ga.Group.Name).First().Group.Name)
                .ThenBy(a => a.Name)
            .Select(AccountModel.Projection)
            .ToArrayAsync());
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

        var account = await _dbContext.Accounts
            .PendingApproval()
            .CanAccess(_dbContext, Cluster, currentUser.Iam)
            .Include(a => a.Owner)
            .AsSingleQuery()
            .SingleOrDefaultAsync(a => a.Id == id);

        if (account == null)
        {
            return NotFound();
        }

        var connectionInfo = await _dbContext.Clusters.GetSshConnectionInfo(Cluster);

        var tempFileName = $"/var/lib/remote-api/.{account.Owner.Kerberos}.yaml"; //Leading .
        var fileName = $"/var/lib/remote-api/{account.Owner.Kerberos}.yaml";

        await _sshService.PlaceFile(account.AccountYaml, tempFileName, connectionInfo);
        await _sshService.RenameFile(tempFileName, fileName, connectionInfo);

        account.Status = Account.Statuses.Active;

        var success = await _notificationService.AccountDecision(account, true);
        if (!success)
        {
            Log.Error("Error creating Account Decision email");
        }

        // safe to assume admin override if no GroupAdmin permission is found
        var permissions = await _userService.GetCurrentPermissionsAsync();
        var isAdminOverride = permissions.Any(p => p.Cluster.Name == Cluster
            && p.Role.Name == Role.Codes.GroupAdmin
            && account.GroupAccounts.Any(ga => ga.GroupId == p.GroupId));

        await _historyService.AccountApproved(account, isAdminOverride);

        await _dbContext.SaveChangesAsync();

        return Ok();
    }

    [HttpPost]
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

        var account = await _dbContext.Accounts
            .PendingApproval()
            .CanAccess(_dbContext, Cluster, currentUser.Iam)
            .Include(a => a.Owner)
            .AsSingleQuery()
            .SingleOrDefaultAsync(a => a.Id == id);

        if (account == null)
        {
            return NotFound();
        }

        account.Status = Account.Statuses.Rejected;
        account.IsActive = false;

        var success = await _notificationService.AccountDecision(account, false, reason: model.Reason);
        if (!success)
        {
            Log.Error("Error creating Account Decision email");
        }

        // safe to assume admin override if no GroupAdmin permission is found
        var permissions = await _userService.GetCurrentPermissionsAsync();
        var isAdminOverride = permissions.Any(p => p.Cluster.Name == Cluster
            && p.Role.Name == Role.Codes.GroupAdmin
            && account.GroupAccounts.Any(ga => ga.GroupId == p.GroupId));
            
        await _historyService.AccountRejected(account, isAdminOverride, model.Reason);


        await _dbContext.SaveChangesAsync();

        return Ok();
    }


    [HttpPost]
    public async Task<ActionResult> Create([FromBody] AccountCreateModel model)
    {
        var currentUser = await _userService.GetCurrentUser();
        model.SshKey = Regex.Replace(model.SshKey, @"(?<!ssh-rsa)\s+(([\w\.\-]+)@([\w\-]+\.?)+)?", "");

        if (model.GroupId == 0)
        {
            return BadRequest("Please select a group from the list.");
        }
        if (string.IsNullOrWhiteSpace(model.SshKey))
        {
            return BadRequest("Missing SSH Key");
        }
        if (!model.SshKey.StartsWith("ssh-") || model.SshKey.Trim().Length < 25)
        {
            return BadRequest("Invalid SSH key");
        }

        var cluster = await _dbContext.Clusters.SingleOrDefaultAsync(c => c.Name == Cluster);
        if (cluster == null)
        {
            return BadRequest("Cluster not found");
        }

        var existingAccount = await _dbContext.Accounts
            .Include(a => a.Owner)
            .Include(a => a.Cluster)
            .Include(a => a.GroupAccounts)
                .ThenInclude(ga => ga.Group)
            .SingleOrDefaultAsync(a =>
                a.OwnerId == currentUser.Id
                && a.ClusterId == cluster.Id
                && a.Status != Account.Statuses.Rejected);

        if (existingAccount != null && existingAccount.Status == Account.Statuses.Active)
        {
            if (existingAccount.Status != Account.Statuses.Active)
            {
                return BadRequest("Only Active accounts can be updated.");
            }

            existingAccount.AccountYaml = await _yamlService.Get(currentUser, model, cluster);

            var connectionInfo = await _dbContext.Clusters.GetSshConnectionInfo(Cluster);
            var tempFileName = $"/var/lib/remote-api/.{existingAccount.Owner.Kerberos}.yaml"; //Leading .
            var fileName = $"/var/lib/remote-api/{existingAccount.Owner.Kerberos}.yaml";

            await _sshService.PlaceFile(existingAccount.AccountYaml, tempFileName, connectionInfo);
            await _sshService.RenameFile(tempFileName, fileName, connectionInfo);

            // safe to assume admin override if current user is not the owner
            var isAdminOverride = existingAccount.OwnerId != currentUser.Id;

            await _historyService.AccountUpdated(existingAccount, isAdminOverride);

            await _dbContext.SaveChangesAsync();

            return Ok(new AccountModel(existingAccount));
        }
        else
        {

            var account = new Account()
            {
                Owner = currentUser,
                AccountYaml = await _yamlService.Get(currentUser, model, cluster),
                IsActive = true,
                Name = $"{currentUser.Name} ({currentUser.Email})",
                ClusterId = cluster.Id,
                Status = Account.Statuses.PendingApproval,
            };

            account = await _historyService.AccountRequested(account);

            await _dbContext.Accounts.AddAsync(account);
            await _dbContext.GroupsAccounts.AddAsync(new GroupAccount { GroupId = model.GroupId, AccountId = account.Id });
            await _dbContext.SaveChangesAsync();

            var success = await _notificationService.AccountRequested(account);
            if (!success)
            {
                Log.Error("Error creating Account Request email");
            }

            var accountModel = await _dbContext.Accounts
                .Where(a => a.Id == account.Id)
                .Select(AccountModel.Projection)
                .SingleAsync();

            return Ok(accountModel);
        }
    }
}
