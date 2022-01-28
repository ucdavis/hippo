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
            var model = new SampleModel();
            model.Name = "Some Name, really.";
            model.SomeText = "This is some replaced text.";
            model.SomeText2 = "Even More replaced text";

            var emailBody = await RazorTemplateEngine.RenderAsync("/Views/Emails/Sample.cshtml", model);


            //await _notificationService.SendSampleNotificationMessage("jsylvestre@ucdavis.edu", emailBody);
            await _notificationService.SendNotification(new string[] { "jsylvestre@ucdavis.edu" }, null, emailBody, "Test", "Test 2");

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
