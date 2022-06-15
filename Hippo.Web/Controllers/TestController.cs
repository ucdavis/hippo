using Hippo.Core.Data;
using Hippo.Core.Models;
using Hippo.Core.Services;
using Hippo.Email.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.EntityFrameworkCore;
using Razor.Templating.Core;
using System.Text;

namespace Hippo.Web.Controllers
{
    [Authorize(Policy = AccessCodes.SystemAccess)]
    public class TestController : SuperController
    {
        public IEmailService _emailService { get; }
        public ISshService _sshService { get; }
        public INotificationService _notificationService { get; }
        public AppDbContext _dbContext { get; }
        public ISecretsService _secretsService { get; }

        public TestController(IEmailService emailService, ISshService sshService, INotificationService notificationService, AppDbContext dbContext,
            ISecretsService secretsService)
        {
            _emailService = emailService;
            _sshService = sshService;
            _notificationService = notificationService;
            _dbContext = dbContext;
            _secretsService = secretsService;
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
            if (await _notificationService.AccountRequested(account))
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

        public async Task<IActionResult> TestSsh()
        {
            if (Cluster == null)
            {
                return Content("No cluster in route");
            }
            var connectionInfo = await _dbContext.Clusters.GetSshConnectionInfo(Cluster);

            var testValue = await _sshService.Test(connectionInfo);
            var sb = new StringBuilder();
            foreach (var result in testValue)
            {
                sb.AppendLine(result);
            }

            return Content(sb.ToString());
        }

        public async Task<IActionResult> TestScp()
        {
            if (Cluster == null)
            {
                return Content("No cluster in route");
            }
            var connectionInfo = await _dbContext.Clusters.GetSshConnectionInfo(Cluster);

            await _sshService.PlaceFile("This is a test file 123.", "/var/lib/remote-api/test.txt", connectionInfo);
            return Content("file placed");
        }

        public async Task<IActionResult> TestScd()
        {
            if (Cluster == null)
            {
                return Content("No cluster in route");
            }
            var connectionInfo = await _dbContext.Clusters.GetSshConnectionInfo(Cluster);

            using var stream = await _sshService.DownloadFile("xxxx.txt", connectionInfo);

            return File(stream.ToArray(), "application/force-download");
        }

        public async Task<IActionResult> TestRename1()
        {
            if (Cluster == null)
            {
                return Content("No cluster in route");
            }
            var connectionInfo = await _dbContext.Clusters.GetSshConnectionInfo(Cluster);

            await _sshService.RenameFile("jsylvest.txt", ".jsylvest.txt", connectionInfo);

            var testValue = await _sshService.Test(connectionInfo);
            var sb = new StringBuilder();
            foreach (var result in testValue)
            {
                sb.AppendLine(result);
            }

            return Content(sb.ToString());
        }
        public async Task<IActionResult> TestRename2()
        {
            if (Cluster == null)
            {
                return Content("No cluster in route");
            }
            var connectionInfo = await _dbContext.Clusters.GetSshConnectionInfo(Cluster);

            await _sshService.RenameFile(".jsylvest.txt", "jsylvest.txt", connectionInfo);

            var testValue = await _sshService.Test(connectionInfo);
            var sb = new StringBuilder();
            foreach (var result in testValue)
            {
                sb.AppendLine(result);
            }

            return Content(sb.ToString());
        }

        public async Task<IActionResult> TestSecret()
        {
            var sb = new StringBuilder();
            var id = "test-" + Guid.NewGuid().ToString();
            await _secretsService.SetSecret("id", "Hello Secret!");

            var secret = await _secretsService.GetSecret(id);
            sb.AppendLine(secret);
            await _secretsService.DeleteSecret(id);
            try
            {
                secret = await _secretsService.GetSecret(id);
                sb.Append(secret);
                sb.AppendLine(" (This should not be here)");
            }
            catch (KeyVaultErrorException ex) when (ex.Message.Contains("NotFound"))
            {
                sb.AppendLine("Secret successfully deleted");
            }

            return Content(sb.ToString());
        }

        [Authorize(Policy = AccessCodes.AdminAccess)]
        public IActionResult TestAuth()
        {
            return Content("Success");
        }
    }
}
