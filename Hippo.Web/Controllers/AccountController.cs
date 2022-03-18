using Hippo.Core.Data;
using Hippo.Core.Domain;
using Hippo.Core.Services;
using Hippo.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Text;

namespace Hippo.Web.Controllers;

[Authorize]
public class AccountController : SuperController
{
    private AppDbContext _dbContext;
    private IUserService _userService;
    private ISshService _sshService;
    private INotificationService _notificationService;
    private IHistoryService _historyService;

    public AccountController(AppDbContext dbContext, IUserService userService, ISshService sshService, INotificationService notificationService, IHistoryService historyService)
    {
        _dbContext = dbContext;
        _userService = userService;
        _sshService = sshService;
        _notificationService = notificationService;
        _historyService = historyService;
    }

    // Return account info for the currently logged in user
    [HttpGet]
    public async Task<ActionResult> Get()
    {
        var currentUser = await _userService.GetCurrentUser();

        return Ok(await _dbContext.Accounts.AsNoTracking().SingleOrDefaultAsync(a => a.Owner.Iam == currentUser.Iam));
    }

    [HttpGet]
    public async Task<ActionResult> Sponsors()
    {
        return Ok(await _dbContext.Accounts.Where(a => a.CanSponsor).AsNoTracking().OrderBy(a => a.Name).ToListAsync());
    }

    // Return all accounts that are waiting for the current user to approve
    [HttpGet]
    public async Task<ActionResult> Pending()
    {
        var currentUser = await _userService.GetCurrentUser();
        //Make this one order by date? Or stay consistent and just by name?
        return Ok(await _dbContext.Accounts.Where(a => a.Sponsor.OwnerId == currentUser.Id && a.Status == Account.Statuses.PendingApproval).AsNoTracking().OrderBy(a => a.Name).ToListAsync());
    }

    [HttpGet]
    public async Task<ActionResult> Sponsored()
    {
        var currentUser = await _userService.GetCurrentUser();

        return Ok(await _dbContext.Accounts.Where(a => a.Sponsor.OwnerId == currentUser.Id && a.Status != Account.Statuses.PendingApproval).AsNoTracking().OrderBy(a => a.Name).ToListAsync());
    }

    // Approve a given pending account if you are the sponsor
    [HttpPost]
    public async Task<ActionResult> Approve(int id)
    {
        var currentUser = await _userService.GetCurrentUser();

        var account = await _dbContext.Accounts.Include(a => a.Owner).AsSingleQuery()
            .SingleOrDefaultAsync(a => a.Id == id && a.Sponsor.OwnerId == currentUser.Id && a.Status == Account.Statuses.PendingApproval);

        if (account == null)
        {
            return NotFound();
        }


        var tempFileName = $"/var/lib/remote-api/.{account.Owner.Kerberos}.txt"; //Leading .
        var fileName = $"/var/lib/remote-api/{account.Owner.Kerberos}.txt";

        _sshService.PlaceFile(account.SshKey, tempFileName);
        _sshService.RenameFile(tempFileName, fileName);

        account.Status = Account.Statuses.Active;

        var success = await _notificationService.AccountDecision(account, true);
        if (!success)
        {
            Log.Error("Error creating Account Decision email");
        }

        await _historyService.Approved(account);


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

        await _historyService.Rejected(account, model.Reason);


        await _dbContext.SaveChangesAsync();

        return Ok();
    }


    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateModel model)
    {
        var currentUser = await _userService.GetCurrentUser();

        if(model.SponsorId == 0)
        {
            return BadRequest("Please select a sponsor from the list.");
        }

        // make sure current user doesn't already have another account
        if (await _dbContext.Accounts.AnyAsync(a => a.Owner.Iam == currentUser.Iam))
        {
            return BadRequest("You already have an account");
        }

        if (!(await _dbContext.Accounts.AnyAsync(a => a.Id == model.SponsorId && a.CanSponsor)))
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

        var sponsorAccount = await _dbContext.Accounts.Include(a => a.Owner).SingleAsync(a => a.OwnerId == model.SponsorId);


        var sb = new StringBuilder();
        sb.AppendLine("sponsor:");
        sb.AppendLine($"    accountname: {sponsorAccount.Name}");
        sb.AppendLine($"    name: {sponsorAccount.Owner.Name}");
        sb.AppendLine($"    email: {sponsorAccount.Owner.Email}");
        sb.AppendLine($"    kerb: {sponsorAccount.Owner.Kerberos}");
        sb.AppendLine($"    iam: {sponsorAccount.Owner.Iam}");
        sb.AppendLine($"    mothra: {sponsorAccount.Owner.MothraId}"); 
        sb.AppendLine();
        sb.AppendLine("Account:");
        sb.AppendLine($"    name: {currentUser.Name}");
        sb.AppendLine($"    email: {currentUser.Email}");
        sb.AppendLine($"    kerb: {currentUser.Kerberos}");
        sb.AppendLine($"    iam: {currentUser.Iam}");
        sb.AppendLine($"    mothra: {currentUser.MothraId}"); 
        sb.AppendLine($"    key: {model.SshKey}");
        


        var account = new Account()
        {
            CanSponsor = false, // TOOD: determine how new sponsors are created
            Owner = currentUser,
            SponsorId = model.SponsorId,
            SshKey = sb.ToString(),
            IsActive = true,
            Name = $"{currentUser.Name} ({currentUser.Email})",
            Status = Account.Statuses.PendingApproval,
        };

        account = await _historyService.Requested(account);

        await _dbContext.Accounts.AddAsync(account);
        await _dbContext.SaveChangesAsync();

        var success = await _notificationService.AccountRequested(account);
        if (!success)
        {
            Log.Error("Error creating Account Request email");
        }

        return Ok(account);
    }

    public class CreateModel
    {
        public int SponsorId { get; set; }
        public string SshKey { get; set; } = String.Empty;
    }


}
