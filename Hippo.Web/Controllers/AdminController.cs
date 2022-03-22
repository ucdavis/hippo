using Hippo.Core.Data;
using Hippo.Core.Domain;
using Hippo.Core.Models;
using Hippo.Core.Services;
using Hippo.Web.Extensions;
using Hippo.Web.Models;
using Hippo.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using static Hippo.Core.Domain.Account;

namespace Hippo.Web.Controllers;

[Authorize(Policy = AccessCodes.AdminAccess)]
public class AdminController : SuperController
{
    private AppDbContext _dbContext;
    private IUserService _userService;
    private IIdentityService _identityService;
    private IHistoryService _historyService;
    private ISshService _sshService;
    private INotificationService _notificationService;

    public AdminController(AppDbContext dbContext, IUserService userService, IIdentityService identityService, ISshService sshService, INotificationService notificationService, IHistoryService historyService)
    {
        _dbContext = dbContext;
        _userService = userService;
        _identityService = identityService;
        _historyService = historyService;
        _sshService = sshService;
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        return Ok(await _dbContext.Accounts.InCluster(Cluster).Where(a => a.IsAdmin).Include(a => a.Owner).AsNoTracking().OrderBy(a => a.Owner.FirstName).ThenBy(a => a.Owner.LastName).ToListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Create(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest("You must supply either an email or kerb id to lookup.");
        }

        var userLookup = id.Contains("@")
                    ? await _identityService.GetByEmail(id)
                    : await _identityService.GetByKerberos(id);
        if (userLookup == null)
        {
            return BadRequest("User Not Found");
        }

        // try to find user account in the cluster
        var clusterAccount = await _dbContext.Accounts.Include(a => a.Owner).InCluster(Cluster).Where(a => a.Owner.Iam == userLookup.Iam).SingleOrDefaultAsync();

        if (clusterAccount != null)
        {
            // user found with existing account.  upgrade to admin
            if (clusterAccount.IsAdmin)
            {
                return BadRequest("User is already an admin.");
            }
            clusterAccount.IsAdmin = true;
        }
        else
        {
            // need to create new account, but user might already exist
            var user = await _dbContext.Users.SingleOrDefaultAsync(a => a.Iam == userLookup.Iam);

            if (user == null)
            {
                // user doesn't exist, assing them to found user
                user = userLookup;
            }

            // now we have the user but need to create the account
            clusterAccount = new Account
            {
                Name = user.Name,
                CanSponsor = false,
                Owner = user,
                IsAdmin = true,
                IsActive = true,
                Status = Account.Statuses.PendingApproval,
                ClusterId = (await _dbContext.Clusters.SingleAsync(c => c.Name == Cluster)).Id
            };

            _dbContext.Accounts.Add(clusterAccount);
        }

        await _historyService.AddHistory("Admin role added", $"Kerb: {clusterAccount.Owner.Kerberos} IAM: {clusterAccount.Owner.Iam} Email: {clusterAccount.Owner.Email} Name: {clusterAccount.Owner.Name}");

        await _dbContext.SaveChangesAsync();
        return Ok(clusterAccount);
    }

    [HttpPost]
    public async Task<IActionResult> Remove(int id)
    {
        // TODO: change from userID to account ID

        // try to find user account in the cluster
        var clusterAccount = await _dbContext.Accounts.Include(a => a.Owner).InCluster(Cluster).Where(a => a.Owner.Id == id).SingleOrDefaultAsync();

        if (clusterAccount == null)
        {
            return NotFound();
        }

        if (clusterAccount.Owner.Id == (await _userService.GetCurrentUser()).Id)
        {
            return BadRequest("Can't remove yourself");
        }

        clusterAccount.IsAdmin = false;

        await _historyService.AddHistory("Admin role removed", $"Kerb: {clusterAccount.Owner.Kerberos} IAM: {clusterAccount.Owner.Iam} Email: {clusterAccount.Owner.Email} Name: {clusterAccount.Owner.Name}");

        await _dbContext.SaveChangesAsync();
        return Ok();
    }

    // TODO: FROM HERE DOWN the methods need to be updated for the new cluster and account isAdmin feature

    [HttpGet]
    public async Task<IActionResult> Sponsors()
    {
        return Ok(await _dbContext.Accounts.Include(a => a.Owner).Where(a => a.CanSponsor).AsNoTracking().OrderBy(a => a.Name).ToListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> CreateSponsor([FromBody] SponsorCreateModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Lookup))
        {
            return BadRequest("You must supply either an email or kerb id to lookup.");
        }

        var userLookup = model.Lookup.Contains("@")
                    ? await _identityService.GetByEmail(model.Lookup)
                    : await _identityService.GetByKerberos(model.Lookup);
        if (userLookup == null)
        {
            return BadRequest("User Not Found");
        }

        var user = await _dbContext.Users.SingleOrDefaultAsync(a => a.Iam == userLookup.Iam);
        if (user == null)
        {
            user = userLookup;
            await _dbContext.Users.AddAsync(user);
        }

        var isNewAccount = false;

        var account = await _dbContext.Accounts.SingleOrDefaultAsync(a => a.OwnerId == user.Id);
        if (account != null)
        {
            if (account.Status != Statuses.Active)
            {
                return BadRequest($"Existing Account for user is not in the Active status: {account.Status}");
            }
            account.CanSponsor = true;
            if (!string.IsNullOrWhiteSpace(model.Name))
            {
                account.Name = model.Name;
                await _historyService.AddAccountHistory(account, "NameUpdated");
            }
            await _historyService.AddAccountHistory(account, "MadeSponsor");
        }
        else
        {
            account = new Account
            {
                Status = Statuses.Active,
                Name = string.IsNullOrWhiteSpace(model.Name) ? user.Name : model.Name,
                Owner = user,
                CanSponsor = true,
            };
            await _historyService.AddAccountHistory(account, "CreatedSponsor");
            await _dbContext.Accounts.AddAsync(account);

            isNewAccount = true;
        }

        await _historyService.AddHistory("Sponsor role added", $"New Account: {isNewAccount}", account);

        await _dbContext.SaveChangesAsync();

        return StatusCode(isNewAccount ? StatusCodes.Status201Created : StatusCodes.Status200OK, account);
    }

    [HttpPost]
    public async Task<IActionResult> RemoveSponsor(int id)
    {
        var account = await _dbContext.Accounts.SingleOrDefaultAsync(a => a.Id == id);
        if (account == null)
        {
            return NotFound();
        }


        account.CanSponsor = false;
        await _historyService.AddAccountHistory(account, "RemovedSponsor");
        await _historyService.AddHistory("Sponsor role removed", null, account);
        await _dbContext.SaveChangesAsync();
        return Ok();
    }

    // Return all accounts that are waiting for any sponsor to approve
    [HttpGet]
    public async Task<ActionResult> Pending()
    {
        return Ok(await _dbContext.Accounts.Where(a => a.Status == Account.Statuses.PendingApproval).Include(a => a.Sponsor).ThenInclude(a => a.Owner).AsNoTracking().OrderBy(a => a.Name).ToListAsync());
    }

    // Approve a given pending account 
    [HttpPost]
    public async Task<ActionResult> Approve(int id)
    {
        var currentUser = await _userService.GetCurrentUser();

        var account = await _dbContext.Accounts.Include(a => a.Owner).AsSingleQuery()
            .SingleOrDefaultAsync(a => a.Id == id && a.Status == Account.Statuses.PendingApproval);

        if (account == null)
        {
            return NotFound();
        }

        var tempFileName = $"/var/lib/remote-api/.{account.Owner.Kerberos}.txt"; //Leading .
        var fileName = $"/var/lib/remote-api/{account.Owner.Kerberos}.txt";

        _sshService.PlaceFile(account.SshKey, tempFileName);
        _sshService.RenameFile(tempFileName, fileName);

        account.Status = Account.Statuses.Active;


        var success = await _notificationService.AccountDecision(account, true, "Admin Override");
        if (!success)
        {
            Log.Error("Error creating Account Decision email");
        }

        success = await _notificationService.AdminOverrideDecision(account, true, currentUser); //Notify sponsor
        if (!success)
        {
            Log.Error("Error creating Admin Override Decision email");
        }

        await _historyService.AccountApproved(account);

        await _historyService.AddHistory("Account override approve", $"Kerb: {account.Owner.Kerberos} IAM: {account.Owner.Iam} Email: {account.Owner.Email} Name: {account.Owner.Name}", account);


        await _dbContext.SaveChangesAsync();

        return Ok();
    }

    [HttpPost]
    public async Task<ActionResult> Reject(int id, [FromBody] RequestRejectionModel model)
    {
        if (String.IsNullOrWhiteSpace(model.Reason))
        {
            return BadRequest("Missing Reject Reason");
        }

        var currentUser = await _userService.GetCurrentUser();

        var account = await _dbContext.Accounts.Include(a => a.Owner).AsSingleQuery()
            .SingleOrDefaultAsync(a => a.Id == id && a.Status == Account.Statuses.PendingApproval);

        if (account == null)
        {
            return NotFound();
        }


        account.Status = Account.Statuses.Rejected;
        account.IsActive = false;

        var success = await _notificationService.AccountDecision(account, false, "Admin Override", reason: model.Reason);
        if (!success)
        {
            Log.Error("Error creating Account Decision email");
        }
        success = await _notificationService.AdminOverrideDecision(account, false, currentUser, reason: model.Reason); //Notify sponsor
        if (!success)
        {
            Log.Error("Error creating Admin Override Decision email");
        }

        await _historyService.AccountRejected(account, model.Reason);

        await _historyService.AddHistory("Account override rejected", $"Kerb: {account.Owner.Kerberos} IAM: {account.Owner.Iam} Email: {account.Owner.Email} Name: {account.Owner.Name} Reason: {model.Reason}", account);


        await _dbContext.SaveChangesAsync();

        return Ok();
    }
}
