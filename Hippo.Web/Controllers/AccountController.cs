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
using Hippo.Core.Extensions;

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
            .AsSplitQuery()
            .InCluster(Cluster)
            .Where(a => a.Owner.Iam == currentUser.Iam)
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
            .AsSplitQuery()
            .CanAccess(_dbContext, Cluster, currentUser.Iam)
            // the projection is sorting groups by name, so we'll sort by the first group name
            .OrderBy(a => a.MemberOfGroups.OrderBy(g => g.Name).First().Name)
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

        if (model.GroupId == 0)
        {
            return BadRequest("Please select a group from the list.");
        }
        if (string.IsNullOrWhiteSpace(model.SshKey))
        {
            return BadRequest("Missing SSH Key");
        }
        if (!model.SshKey.IsValidSshKey())
        {
            return BadRequest("Invalid SSH key");
        }

        var cluster = await _dbContext.Clusters.SingleOrDefaultAsync(c => c.Name == Cluster);
        if (cluster == null)
        {
            return BadRequest("Cluster not found");
        }

        var hasAccount = await _dbContext.Accounts.AnyAsync(a =>a.OwnerId == currentUser.Id
                && a.ClusterId == cluster.Id);

        if (hasAccount)
        {
            return BadRequest("You already have an account for this cluster");
        }

        var request = new AccountRequest
        {
            Requester = currentUser,
            Group = await _dbContext.Groups.Where(g => g.Id == model.GroupId).Select(g => g.Name).SingleAsync(),
            Action = AccountRequest.Actions.CreateAccount,
            Status = AccountRequest.Statuses.PendingApproval,
            Cluster = cluster,
            SupervisingPI = model.SupervisingPI,
        };
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
    public async Task<ActionResult> UpdateSsh([FromBody] AccountSshKeyModel model)
    {
        if (string.IsNullOrWhiteSpace(Cluster))
        {
            return BadRequest("Cluster is required");
        }
        if (string.IsNullOrWhiteSpace(model.SshKey))
        {
            return BadRequest("SSH Key is required");
        }
        if (!model.SshKey.IsValidSshKey())
        {
            return BadRequest("Invalid SSH key");
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

        existingAccount.SshKey = model.SshKey;

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
