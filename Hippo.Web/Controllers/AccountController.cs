using Hippo.Web.Models.Settings;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Hippo.Web.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class AccountController : Controller
    {
        
        private readonly AuthSettings _authenticationSettings;

        public AccountController(IOptions<AuthSettings> authenticationSettings)
        {
            _authenticationSettings = authenticationSettings.Value;
        }
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return Redirect($"{_authenticationSettings.Authority}/logout"); //This clears out all the sessions....
        }
    }
}
