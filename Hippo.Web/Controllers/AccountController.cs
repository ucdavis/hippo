using Hippo.Core.Data;
using Hippo.Core.Domain;
using Hippo.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hippo.Web.Controllers;

[Authorize]
public class AccountController : SuperController
{
    private AppDbContext _dbContext;
    private IUserService _userService;

    public AccountController(AppDbContext dbContext, IUserService userService)
    {
        _dbContext = dbContext;
        _userService = userService;
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

        var account = await _dbContext.Accounts.SingleOrDefaultAsync(a => a.Id == id && a.Sponsor.OwnerId == currentUser.Id && a.Status == Account.Statuses.PendingApproval);

        if (account == null)
        {
            return NotFound();
        }

        account.Status = Account.Statuses.Active;

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

        await _dbContext.Accounts.AddAsync(account);
        await _dbContext.SaveChangesAsync();

        return Ok(account);
    }

    public class CreateModel
    {
        public int SponsorId { get; set; }
        public string SshKey { get; set; } = string.Empty;
    }
}
