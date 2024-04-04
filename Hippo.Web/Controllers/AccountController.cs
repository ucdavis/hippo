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
using System.Text.Json;

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
        var permissions = await _userService.GetCurrentPermissionsAsync();
        var isClusterOrSystemAdmin = permissions.IsClusterOrSystemAdmin(Cluster);

        return Ok(await _dbContext.Accounts
            .AsSplitQuery()
            .CanAccess(Cluster, currentUser.Iam, isClusterOrSystemAdmin)
            // the projection is sorting groups by name, so we'll sort by the first group name
            .OrderBy(a => a.MemberOfGroups.OrderBy(g => g.Name).First().Name)
                .ThenBy(a => a.Name)
            .Select(AccountModel.Projection)
            .ToArrayAsync());
    }



    [HttpPost]
    public async Task<ActionResult> Create([FromBody] AccountCreateModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (string.IsNullOrWhiteSpace(Cluster))
        {
            return BadRequest("Cluster is required");
        }
        var currentUser = await _userService.GetCurrentUser();

        if (model.GroupId == 0)
        {
            return BadRequest("Please select a group from the list.");
        }
        var cluster = await _dbContext.Clusters
            .Include(c => c.AccessTypes)
            .SingleOrDefaultAsync(c => c.Name == Cluster);
        if (cluster == null)
        {
            return BadRequest("Cluster not found");
        }

        if (model.AccessTypes.Contains(AccessType.Codes.SshKey))
        {
            if (!cluster.AccessTypes.Any(at => at.Name == AccessType.Codes.SshKey))
            {
                return BadRequest("SSH Key access is not allowed for this cluster");
            }

            if (string.IsNullOrWhiteSpace(model.SshKey))
            {
                return BadRequest("Missing SSH Key");
            }

            if (!model.SshKey.IsValidSshKey())
            {
                return BadRequest("Invalid SSH key");
            }
        }
        else
        {
            if (cluster.AccessTypes.Count == 1 && cluster.AccessTypes[0].Name == AccessType.Codes.SshKey)
            {
                return BadRequest("SSH Key is required for this cluster");
            }

            if (!string.IsNullOrWhiteSpace(model.SshKey))
            {
                // This shouldn't ever happen, but...
                return BadRequest("SSH Key provided without accompanying AccessType 'SshKey'");
            }
        }

        var hasAccount = await _dbContext.Accounts.AnyAsync(a => a.OwnerId == currentUser.Id
                && a.ClusterId == cluster.Id);

        if (hasAccount)
        {
            return BadRequest("You already have an account for this cluster");
        }

        // AccountRequest is an alias for Hippo.Core.Domain.Request to avoid clash with ControllerBase.Request
        var request = new AccountRequest
        {
            Requester = currentUser,
            Group = await _dbContext.Groups.Where(g => g.Id == model.GroupId).Select(g => g.Name).SingleAsync(),
            Action = AccountRequest.Actions.CreateAccount,
            Status = AccountRequest.Statuses.PendingApproval,
            Cluster = cluster
        }
        .WithAccountRequestData(new AccountRequestDataModel
        {
            SupervisingPI = model.SupervisingPI,
            SshKey = model.SshKey,
            AccessTypes = model.AccessTypes
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
    public async Task<ActionResult> UpdateSsh([FromBody] AccountSshKeyModel model)
    {
        if (string.IsNullOrWhiteSpace(Cluster))
        {
            return BadRequest("Cluster is required");
        }
        var cluster = await _dbContext.Clusters
            .Include(c => c.AccessTypes)
            .SingleOrDefaultAsync(c => c.Name == Cluster);
        if (cluster == null)
        {
            return BadRequest("Cluster not found");
        }
        if (!cluster.AccessTypes.Any(at => at.Name == AccessType.Codes.SshKey))
        {
            return BadRequest("User SSH Keys are not enabled for this cluster");
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
            .Include(a => a.AccessTypes)
            .AsSplitQuery()
            .SingleOrDefaultAsync(a =>
                a.Id == model.AccountId
                && a.OwnerId == currentUser.Id
                && a.Cluster.Name == Cluster);

        if (existingAccount == null)
        {
            return NotFound();
        }

        existingAccount.SshKey = model.SshKey;
        if (!existingAccount.AccessTypes.Any(at => at.Name == AccessType.Codes.SshKey))
        {
            existingAccount.AccessTypes.Add(await _dbContext.AccessTypes.SingleAsync(at => at.Name == AccessType.Codes.SshKey));
        }

        var result = await _accountUpdateService.QueueUpdateSshKey(existingAccount, model.SshKey);
        if (result.IsError)
        {
            // shouldn't ever get here, but just in case
            return BadRequest($"Error queuing {QueuedEvent.Actions.UpdateSshKey}. The error has been logged for review. Please try again later.");
        }

        // safe to assume admin override if current user is not the owner
        var isAdminOverride = existingAccount.OwnerId != currentUser.Id;

        await _historyService.AccountUpdated(existingAccount, isAdminOverride);

        await _dbContext.SaveChangesAsync();

        return Ok(new AccountModel(existingAccount));
    }
}
