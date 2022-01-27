using Hippo.Core.Services;
using Hippo.Email.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Razor.Templating.Core;

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

        public async Task<IActionResult> TestBody()
        {
            var model = new SampleModel();



            var results = await RazorTemplateEngine.RenderAsync("/Views/Emails/Sample_mjml.cshtml", model);

            return Content(results);
        }
    }
}
