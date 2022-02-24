using Hippo.Core.Data;
using Hippo.Core.Domain;
using Hippo.Core.Models;
using Hippo.Core.Services;
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

    public AdminController(AppDbContext dbContext, IUserService userService, IIdentityService identityService, IHistoryService historyService)
    {
        _dbContext = dbContext;
        _userService = userService;
        _identityService = identityService;
        _historyService = historyService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        return Ok(await _dbContext.Users.Where(a => a.IsAdmin).AsNoTracking().ToListAsync());
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

        var user = await _dbContext.Users.SingleOrDefaultAsync(a => a.Iam == userLookup.Iam);
        if (user != null)
        {
            if (user.IsAdmin)
            {
                return BadRequest("User is already an admin.");
            }
            user.IsAdmin = true;
        }
        else
        {
            user = userLookup;
            user.IsAdmin = true;
            await _dbContext.Users.AddAsync(user);
        }
        await _dbContext.SaveChangesAsync();
        return Ok(user);

    }

    [HttpPost]
    public async Task<IActionResult> Remove(int id)
    {
        var user = await _dbContext.Users.SingleOrDefaultAsync(a => a.Id == id);
        if (user == null)
        {
            return NotFound();
        }

        if(user.Id == (await _userService.GetCurrentUser()).Id)
        {
            return BadRequest("Can't remove yourself");
        }

        user.IsAdmin = false;
        await _dbContext.SaveChangesAsync();
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> Sponsors()
    {
        return Ok(await _dbContext.Accounts.Include(a => a.Owner).Where(a => a.CanSponsor).AsNoTracking().ToListAsync());
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

        var account = await _dbContext.Accounts.SingleOrDefaultAsync(a => a.OwnerId == user.Id);
        if(account != null)
        {
            if(account.Status != Statuses.Active)
            {
                return BadRequest($"Existing Account for user is not in the Active status: {account.Status}");
            }
            account.CanSponsor = true;
            if (!string.IsNullOrWhiteSpace(model.Name))
            {
                account.Name = model.Name;
                await _historyService.AddHistory(account, "NameUpdated");
            }
            await _historyService.AddHistory(account, "MadeSponsor");
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
            await _historyService.AddHistory(account, "CreatedSponsor");
        }
        await _dbContext.SaveChangesAsync();
        return Ok(user);

    }

}
