using Hippo.Core.Data;
using Hippo.Core.Models;
using Hippo.Core.Services;
using Hippo.Email.Models;
using Hippo.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Razor.Templating.Core;
using System.Text;

namespace Hippo.Web.Controllers
{
    [Authorize(Policy = AccessCodes.SystemAccess)]
    public class TestController : Controller
    {
        public IEmailService _emailService { get; }
        public ISshService _sshService { get; }
        public INotificationService _notificationService { get; }
        public AppDbContext _dbContext { get; }
        public IBulkLoadService BulkLoadService { get; }

        public TestController(IEmailService emailService, ISshService sshService, INotificationService notificationService, AppDbContext dbContext, IBulkLoadService bulkLoadService)
        {
            _emailService = emailService;
            _sshService = sshService;
            _notificationService = notificationService;
            _dbContext = dbContext;
            BulkLoadService = bulkLoadService;
        }

        public async Task<IActionResult> TestEmail()
        {
            var model = new SampleModel();
            model.Name = "Some Name, really.";
            model.SomeText = "This is some replaced text.";
            model.SomeText2 = "Even More replaced text";

            var emailBody = await RazorTemplateEngine.RenderAsync("/Views/Emails/Sample.cshtml", model);


            //await _notificationService.SendSampleNotificationMessage("jsylvestre@ucdavis.edu", emailBody);
            await _emailService.SendEmail(new string[] { "jsylvestre@ucdavis.edu" }, null, emailBody, "Test", "Test 2");

            return Content("Done. Maybe...");
        }

        public async Task<IActionResult> TestBody()
        {
            var model = new DecisionModel();



            var results = await RazorTemplateEngine.RenderAsync("/Views/Emails/AdminOverrideDecission_mjml.cshtml", model);

            return Content(results);
        }

        public async Task<IActionResult> TestAccountRequest()
        {
            var account = await _dbContext.Accounts.SingleAsync(a => a.Id == 2);
            if(await _notificationService.AccountRequested(account))
            {
                return Content("Email Sent");
            }
            return Content("Houston we have a problem");
        }

        public async Task<IActionResult> TestAccountDecision()
        {
            var account = await _dbContext.Accounts.SingleAsync(a => a.Id == 4);
            if (await _notificationService.AccountDecision(account, true))
            {
                await _notificationService.AccountDecision(account, false, reason: "Fake reject Reason here.");
                return Content("Emails Sent");
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
            using var stream = _sshService.DownloadFile("xxxx.txt"); 
            
            return File(stream.ToArray(), "application/force-download");
        }

        public IActionResult TestRename1()
        {

            _sshService.RenameFile("jcstest.txt", ".jcstest.txt");

            var testValue = _sshService.Test();
            var sb = new StringBuilder();
            foreach (var result in testValue)
            {
                sb.AppendLine(result);
            }

            return Content(sb.ToString());
        }
        public IActionResult TestRename2()
        {

            _sshService.RenameFile(".jcstest.txt", "jcstest.txt");

            var testValue = _sshService.Test();
            var sb = new StringBuilder();
            foreach (var result in testValue)
            {
                sb.AppendLine(result);
            }

            return Content(sb.ToString());
        }

        [HttpGet]
        public IActionResult BulkLoad()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> BulkLoad(string id)
        {
            var cluster = await _dbContext.Clusters.Where(c => c.Name == "caesfarm").SingleAsync();
            //var data = "tucoterv-cslupsky,dsondag-cslupsky,nacuriel-cslupsky,marikab-clmark,cshiang-clmark,tmfranci-lgmastac,jdowen-clmark,jshkaur-clmark,amzepeda-lgmastac,siocrubs-lgmastac,jchillho-lgmastac,mdhawort-lgmastac,helvic-lgmastac,orendain-lgmastac,noelle-lgmastac,dwfujino-lgmastac,tdickins-lgmastac,nmreynol-lgmastac,vjebanez-lgmastac,aeht-lgmastac";

            var data = id.Replace("\r", string.Empty);
            data = data.Replace("\n", string.Empty);

            var count = await BulkLoadService.Load(cluster, data);

            return Content(count.ToString());
        }

        [Authorize(Policy = AccessCodes.AdminAccess)]
        public IActionResult TestAuth()
        {
            return Content("Success");
        }
    }
}
