using Hippo.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hippo.Web.Controllers
{
    [Authorize]
    public class TestController : Controller
    {
        public INotificationService _notificationService { get; }

        public TestController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public async Task<IActionResult> TestEmail()
        {
            await _notificationService.SendSampleNotificationMessage("jsylvestre@ucdavis.edu", "Test the body");

            return Content("Done. Maybe...");
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
