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
public class AccountController : SuperController
{
    private readonly AppDbContext _dbContext;
    private readonly IUserService _userService;
    private readonly INotificationService _notificationService;
    private readonly IHistoryService _historyService;
    private readonly IAccountUpdateService _accountUpdateService;

    public AccountController(AppDbContext dbContext, IUserService userService, INotificationService notificationService, 
        IHistoryService historyService, IAccountUpdateService accountUpdateService)
    {
        _dbContext = dbContext;
        _userService = userService;
        _notificationService = notificationService;
        _historyService = historyService;
        _accountUpdateService = accountUpdateService;
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
        if (string.IsNullOrWhiteSpace(Cluster))
        {
            return BadRequest("Cluster is required");
        }
        var currentUser = await _userService.GetCurrentUser();

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



    [HttpPost]
    public async Task<ActionResult> Create([FromBody] AccountCreateModel model)
    {
        if (string.IsNullOrWhiteSpace(Cluster))
        {
            return BadRequest("Cluster is required");
        }
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

        if (existingAccount != null)
        {
            return BadRequest("You already have an account for this cluster");
        }

        var account = new Account()
        {
            Owner = currentUser,
            AccountYaml = model.SshKey,
            IsActive = true,
            Name = $"{currentUser.Name} ({currentUser.Email})",
            Cluster = cluster,
            Status = Account.Statuses.PendingApproval,
        };

        account = await _historyService.AccountRequested(account);

        await _dbContext.Accounts.AddAsync(account);
        await _dbContext.GroupsAccounts.AddAsync(new GroupAccount { GroupId = model.GroupId, AccountId = account.Id });
        var request = new AccountRequest
        {
            Account = account,
            Requester = currentUser,
            Group = await _dbContext.Groups.Where(g => g.Id == model.GroupId).SingleAsync(),
            Action = AccountRequest.Actions.CreateAccount,
            Status = AccountRequest.Statuses.PendingApproval,
            Cluster = cluster,
        };
        await _dbContext.Requests.AddAsync(request);
        await _dbContext.SaveChangesAsync();

        var success = await _notificationService.AccountRequest(request);
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

    [HttpPost]
    public async Task<ActionResult> UpdateSsh(AccountSshKeyModel model)
    {
        if (string.IsNullOrWhiteSpace(Cluster))
        {
            return BadRequest("Cluster is required");
        }

        var currentUser = await _userService.GetCurrentUser();
        var existingAccount = await _dbContext.Accounts
            .Include(a => a.Owner)
            .Include(a => a.Cluster)
            .SingleOrDefaultAsync(a =>
                a.Id == model.AccountId
                && a.OwnerId == currentUser.Id
                && a.Cluster.Name == Cluster);

        if (existingAccount == null)
        {
            return NotFound();
        }

        if (existingAccount.Status != Account.Statuses.Active)
        {
            return BadRequest("Only Active accounts can be updated.");
        }

        existingAccount.AccountYaml = model.SshKey;

        if (!await _accountUpdateService.UpdateAccount(existingAccount))
        {
            // It could be that ssh is down
            return BadRequest("Error updating account. Please try again later.");
        }

        // safe to assume admin override if current user is not the owner
        var isAdminOverride = existingAccount.OwnerId != currentUser.Id;

        await _historyService.AccountUpdated(existingAccount, isAdminOverride);

        await _dbContext.SaveChangesAsync();

        return Ok(new AccountModel(existingAccount));
    }
}
