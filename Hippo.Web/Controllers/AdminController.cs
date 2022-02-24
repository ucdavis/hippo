using Hippo.Core.Data;
using Hippo.Core.Domain;
using Hippo.Core.Models;
using Hippo.Core.Services;
using Hippo.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Hippo.Web.Controllers;

[Authorize(Policy = AccessCodes.AdminAccess)]
public class AdminController : SuperController
{
    private AppDbContext _dbContext;
    private IUserService _userService;
    private IIdentityService _identityService;

    public AdminController(AppDbContext dbContext, IUserService userService, IIdentityService identityService)
    {
        _dbContext = dbContext;
        _userService = userService;
        _identityService = identityService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        return Ok(await _dbContext.Users.Where(a => a.IsAdmin).AsNoTracking().ToListAsync());
    }

    [HttpGet]
    public async Task<IActionResult> Sponsors()
    {
        return Ok(await _dbContext.Accounts.Include(a => a.Owner).Where(a => a.CanSponsor).AsNoTracking().ToListAsync());
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
}
