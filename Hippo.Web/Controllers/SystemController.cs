using Hippo.Core.Data;
using Hippo.Core.Extensions;
using Hippo.Core.Models;
using Hippo.Core.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Security.Claims;

namespace Hippo.Web.Controllers
{
    [Authorize]
    public class SystemController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly IIdentityService _identityService;
        private readonly IUserService _userService;

        public SystemController(AppDbContext dbContext, IIdentityService identityService, IUserService userService)
        {
            _dbContext = dbContext;
            _identityService = identityService;
            _userService = userService;
        }

        [Authorize(Policy = AccessCodes.SystemAccess)]
        public async Task<IActionResult> Emulate(string id)
        {
            var currentUser = await _userService.GetCurrentUser();
            Log.Information($"Emulation attempted for {id} by {currentUser.Name}");
            var lookupVal = id.Trim();

            var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.Email == lookupVal || u.Kerberos == lookupVal);
            if (user == null)
            {
                // not found in db, look up user in IAM
                user = lookupVal.Contains("@")
                    ? await _identityService.GetByEmail(lookupVal)
                    : await _identityService.GetByKerberos(lookupVal);

                if (user != null)
                {
                    // user found in IAM but not in our db
                    // let the UserService handle creating it and setting up account ownership when applicable
                    await _userService.GetUser(user.GetClaims());
                }
                else
                {
                    throw new Exception("User is null");
                }
            }

            var identity = new ClaimsIdentity(user.GetClaims(), CookieAuthenticationDefaults.AuthenticationScheme);

            // kill old login
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // create new login
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));


            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> EndEmulate()
        {

            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
