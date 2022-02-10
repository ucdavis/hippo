using Hippo.Core.Data;
using Hippo.Core.Services;
using Hippo.Email.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Razor.Templating.Core;
using System.Text;

namespace Hippo.Web.Controllers
{
    [Authorize]
    public class TestController : Controller
    {
        public INotificationService _notificationService { get; }
        public ISshService _sshService { get; }
        public IEmailService _emailService { get; }
        public AppDbContext _dbContext { get; }

        public TestController(INotificationService notificationService, ISshService sshService, IEmailService emailService, AppDbContext dbContext)
        {
            _notificationService = notificationService;
            _sshService = sshService;
            _emailService = emailService;
            _dbContext = dbContext;
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
            var model = new NewRequestModel();



            var results = await RazorTemplateEngine.RenderAsync("/Views/Emails/AccountRequest_mjml.cshtml", model);

            return Content(results);
        }

        public async Task<IActionResult> TestAccountRequest()
        {
            var account = await _dbContext.Accounts.SingleAsync(a => a.Id == 2);
            if(await _emailService.AccountRequested(account))
            {
                return Content("Email Sent");
            }
            return Content("Houston we have a problem");
        }

        public IActionResult TestSsh()
        {
            var testValue = _sshService.Test();
            var sb = new StringBuilder();
            foreach (var result in testValue)
            {
                sb.AppendLine(result);
            }

            return Content(sb.ToString());
        }

        public IActionResult TestScp()
        {
            _sshService.PlaceFile("This is a test file 123.", "/var/lib/remote-api/test.txt");
            return Content("file placed");
        }

        public IActionResult TestScd()
        {
            using var stream = _sshService.DownloadFile("jcstest.txt"); 
            
            return File(stream.ToArray(), "application/force-download");
        }
    }
}
