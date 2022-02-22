using Hippo.Core.Data;
using Hippo.Core.Domain;
using Hippo.Core.Services;
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
        return Ok(await _dbContext.Accounts.Where(a => a.CanSponsor).AsNoTracking().ToListAsync());
    }

    // Return all accounts that are waiting for the current user to approve
    [HttpGet]
    public async Task<ActionResult> Pending()
    {
        var currentUser = await _userService.GetCurrentUser();

        return Ok(await _dbContext.Accounts.Where(a => a.Sponsor.OwnerId == currentUser.Id && a.Status == Account.Statuses.PendingApproval).AsNoTracking().ToListAsync());
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

        Console.WriteLine($"Approving account {account.Owner.Iam} with ssh key {account.SshKey}");

        var fileName = $"/var/lib/remote-api/{account.Owner.Kerberos}";

        _sshService.PlaceFile(account.SshKey, $"{fileName}.dot");
        _sshService.RenameFile($"{fileName}.dot", $"{fileName}.txt");

        account.Status = Account.Statuses.Active;

        // TODO: send notification
        // TODO: save history of approval
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
    public async Task<ActionResult> Create([FromBody] CreateModel model)
    {
        var currentUser = await _userService.GetCurrentUser();

        // make sure current user doesn't already have another account
        if (await _dbContext.Accounts.AnyAsync(a => a.Owner.Iam == currentUser.Iam))
        {
            return BadRequest("You already have an account");
        }

        if(!(await _dbContext.Accounts.AnyAsync(a => a.Id == model.SponsorId && a.CanSponsor)))
        {
            return BadRequest("Bad Sponsor Id");
        }
        if (string.IsNullOrWhiteSpace(model.SshKey))
        {
            return BadRequest("Missing SSH Key");
        }
        if(!model.SshKey.StartsWith("-----BEGIN RSA PRIVATE KEY-----") || !model.SshKey.EndsWith("-----END RSA PRIVATE KEY-----"))
        {
            return BadRequest("Invalid SSH key");
        }

        var account = new Account()
        {
            CanSponsor = false, // TOOD: determine how new sponsors are created
            Owner = currentUser,
            SponsorId = model.SponsorId,
            SshKey = model.SshKey,
            IsActive = true,
            Name = currentUser.Name,
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
        public string SshKey { get; set; } = string.Empty;
    }
}
