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
using Serilog;

namespace Hippo.Web.Controllers;

[Authorize]
public class AccountController : SuperController
{
    private AppDbContext _dbContext;
    private IUserService _userService;
    private ISshService _sshService;
    private INotificationService _notificationService;
    private IHistoryService _historyService;
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
    public async Task<ActionResult> Get(string cluster)
    {
        var currentUser = await _userService.GetCurrentUser();

        return Ok(await _dbContext.Accounts.InCluster(Cluster).Where(a => a.Owner.Iam == currentUser.Iam).AsNoTracking().ToArrayAsync());
    }

    [HttpGet]
    public async Task<ActionResult> Sponsors()
    {
        return Ok(await _dbContext.Accounts.InCluster(Cluster).Where(a => a.CanSponsor).AsNoTracking().OrderBy(a => a.Name).ToArrayAsync());
    }

    // Return all accounts that are waiting for the current user to approve
    [HttpGet]
    public async Task<ActionResult> Pending()
    {
        var currentUser = await _userService.GetCurrentUser();
        //Make this one order by date? Or stay consistent and just by name?
        return Ok(await _dbContext.Accounts.InCluster(Cluster).Where(a => a.Sponsor.OwnerId == currentUser.Id && a.Status == Account.Statuses.PendingApproval).AsNoTracking().OrderBy(a => a.Name).ToArrayAsync());
    }

    [HttpGet]
    public async Task<ActionResult> Sponsored()
    {
        var currentUser = await _userService.GetCurrentUser();

        return Ok(await _dbContext.Accounts.InCluster(Cluster).Where(a => a.Sponsor.OwnerId == currentUser.Id && a.Status != Account.Statuses.PendingApproval).AsNoTracking().OrderBy(a => a.Name).ToArrayAsync());
    }

    // Approve a given pending account if you are the sponsor
    [HttpPost]
    public async Task<ActionResult> Approve(int id)
    {
        var currentUser = await _userService.GetCurrentUser();

        var account = await _dbContext.Accounts.InCluster(Cluster).Include(a => a.Owner).AsSingleQuery()
            .SingleOrDefaultAsync(a => a.Id == id && a.Sponsor.OwnerId == currentUser.Id && a.Status == Account.Statuses.PendingApproval);

        if (account == null)
        {
            return NotFound();
        }

        var connectionInfo = await _dbContext.Clusters.GetSshConnectionInfo(Cluster);

        var tempFileName = $"/var/lib/remote-api/.{account.Owner.Kerberos}.yaml"; //Leading .
        var fileName = $"/var/lib/remote-api/{account.Owner.Kerberos}.yaml";

        await _sshService.PlaceFile(account.SshKey, tempFileName, connectionInfo);
        await _sshService.RenameFile(tempFileName, fileName, connectionInfo);

        account.Status = Account.Statuses.Active;

        var success = await _notificationService.AccountDecision(account, true);
        if (!success)
        {
            Log.Error("Error creating Account Decision email");
        }

        await _historyService.AccountApproved(account);

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

        var account = await _dbContext.Accounts.InCluster(Cluster).Include(a => a.Owner).AsSingleQuery()
            .SingleOrDefaultAsync(a => a.Id == id && a.Sponsor.OwnerId == currentUser.Id && a.Status == Account.Statuses.PendingApproval);

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

        await _historyService.AccountRejected(account, model.Reason);


        await _dbContext.SaveChangesAsync();

        return Ok();
    }


    [HttpPost]
    public async Task<ActionResult> Create([FromBody] AccountCreateModel model)
    {
        var currentUser = await _userService.GetCurrentUser();

        if(model.SponsorId == 0)
        {
            return BadRequest("Please select a sponsor from the list.");
        }
        if (!(await _dbContext.Accounts.InCluster(Cluster).AnyAsync(a => a.Id == model.SponsorId && a.CanSponsor)))
        {
            return BadRequest("Sponsor not found.");
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
            .InCluster(Cluster)
            .AsSingleQuery()
            .FirstOrDefaultAsync(a => a.OwnerId == currentUser.Id && a.Status != Account.Statuses.Rejected);

        if (existingAccount != null) 
        {
            existingAccount.SshKey = await _yamlService.Get(currentUser, model, cluster);

            await _historyService.AccountApproved(existingAccount);
            await _historyService.AddHistory("Existing account override approve", $"Kerb: {existingAccount.Owner.Kerberos} IAM: {existingAccount.Owner.Iam} Email: {existingAccount.Owner.Email} Name: {existingAccount.Owner.Name}", existingAccount);

            await _dbContext.SaveChangesAsync();

            var connectionInfo = await _dbContext.Clusters.GetSshConnectionInfo(Cluster);
            var tempFileName = $"/var/lib/remote-api/.{existingAccount.Owner.Kerberos}.yaml"; //Leading .
            var fileName = $"/var/lib/remote-api/{existingAccount.Owner.Kerberos}.yaml";

            await _sshService.PlaceFile(existingAccount.SshKey, tempFileName, connectionInfo);
            await _sshService.RenameFile(tempFileName, fileName, connectionInfo);

            return Ok(existingAccount);
        }

        var account = new Account()
        {
            CanSponsor = false, 
            Owner = currentUser,
            SponsorId = model.SponsorId,
            SshKey = await _yamlService.Get(currentUser, model, cluster),
            IsActive = true,
            Name = $"{currentUser.Name} ({currentUser.Email})",
            ClusterId = cluster.Id,
            Status = Account.Statuses.PendingApproval,
        };

        account = await _historyService.AccountRequested(account);

        await _dbContext.Accounts.AddAsync(account);
        await _dbContext.SaveChangesAsync();

        var success = await _notificationService.AccountRequested(account);
        if (!success)
        {
            Log.Error("Error creating Account Request email");
        }

        return Ok(account);
    }
}
