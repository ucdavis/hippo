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

    [HttpPost]
    public async Task<IActionResult> Create(string id)
    {
        var user = id.Contains("@")
                    ? await _identityService.GetByEmail(id)
                    : await _identityService.GetByKerberos(id);
        if (user == null)
        {
            return BadRequest("User Not Found");
        }

        var dbUser = await _dbContext.Users.SingleOrDefaultAsync(a => a.Iam == user.Iam);
        if (dbUser != null)
        {
            dbUser.IsAdmin = true;
        }
        else
        {
            user.IsAdmin = true;
            await _dbContext.Users.AddAsync(user);
        }
        await _dbContext.SaveChangesAsync();
        return Ok();

    }

    [HttpPost]
    public async Task<IActionResult> Remove(string iamId)
    {
        if (iamId == null)
        {
            return BadRequest("Missing IAM id");
        }
        var user = await _dbContext.Users.SingleOrDefaultAsync(a => a.Iam == iamId);
        if (user == null)
        {
            return NotFound();
        }
        user.IsAdmin = false;
        await _dbContext.SaveChangesAsync();
        return Ok();
    }
}
