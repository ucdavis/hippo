﻿using Hippo.Core.Data;
using Hippo.Core.Extensions;
using Hippo.Core.Models;
using Hippo.Core.Models.Settings;
using Hippo.Core.Services;
using Hippo.Core.Models.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Mjml.Net;
using Razor.Templating.Core;
using System.Text;
using static Hippo.Core.Models.SlothModels.TransferViewModel;

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
        private readonly IMjmlRenderer _mjmlRenderer;
        private readonly EmailSettings _emailSettings;

        public ISlothService _slothService { get; }


        public IAggieEnterpriseService _aggieEnterpriseService { get; }

        public TestController(IEmailService emailService, ISshService sshService, INotificationService notificationService, AppDbContext dbContext,
            ISecretsService secretsService, IMjmlRenderer mjmlRenderer, IAggieEnterpriseService aggieEnterpriseService, IOptions<EmailSettings> emailSettings, ISlothService slothService)
        {
            _emailService = emailService;
            _sshService = sshService;
            _notificationService = notificationService;
            _dbContext = dbContext;
            _secretsService = secretsService;
            _mjmlRenderer = mjmlRenderer;
            _aggieEnterpriseService = aggieEnterpriseService;
            _emailSettings = emailSettings.Value;
            _slothService = slothService;
        }

        public async Task<IActionResult> UpdateMissingPaymentDetails()
        {
            var model = await _slothService.UpdateMissingPaymentDetails();
            return Content(model.ToString());
        }

        public async Task<IActionResult> TestEmail()
        {
            var model = new SampleModel
            {
                UcdLogoUrl = $"{_emailSettings.BaseUrl}/media/hpcLogo.png",
                Name = "Some Name, really.",
                SomeText = "This is some replaced text.",
                SomeText2 = "Even More replaced text"
            };

            var htmlBody = await _mjmlRenderer.RenderView("/Views/Emails/Sample_mjml.cshtml", model);

            await _emailService.SendEmail(new EmailModel
            {
                Emails = new string[] { "jsylvestre@ucdavis.edu" },
                HtmlBody = htmlBody,
                TextBody = "Test",
                Subject = "Test 2"
            });

            return Content("Done. Maybe...");
        }

        public async Task<IActionResult> TestBody()
        {
            var model = new DecisionModel();



            var results = await _mjmlRenderer.RenderView("/Views/Emails/AdminOverrideDecision_mjml.cshtml", model);

            return Content(results);
        }

        public async Task<IActionResult> TestBody2()
        {
            var model = new OrderNotificationModel();
            model.ButtonText = "Test Button";
            model.ButtonUrl = "https://www.ucdavis.edu";
            model.ButtonTwoText = "Test Button 2";
            model.ButtonTwoUrl = "https://www.ucdavis.edu";
            model.Subject = "Test Subject";
            model.Header = "Test Header";
            model.Paragraphs.Add("This is a test paragraph.");
            model.Paragraphs.Add("This is another test paragraph.");



            var results = await _mjmlRenderer.RenderView("/Views/Emails/OrderNotificationTwoButton_mjml.cshtml", model);

            return Content(results);
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
            await _secretsService.SetSecret(id, "Hello Secret!");

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

        public async Task<IActionResult> TestAggieEnterprise()
        {
            var model = await _aggieEnterpriseService.IsChartStringValid("3110-13U02-ADNO006-522201-43-000-0000000000-200504-0000-000000-000000", Directions.Debit);

            return Content(model.IsValid.ToString());

        }

        [Authorize(Policy = AccessCodes.ClusterAdminAccess)]
        public IActionResult TestAuth()
        {
            return Content("Success");
        }

        public async Task<IActionResult> TestNotification()
        {
            var order = await _dbContext.Orders
                .Include(a => a.Cluster)
                .Include(a => a.PrincipalInvestigator)
                // We don't want to filter on inactive PIs, we still do on inactive clusters
                .IgnoreQueryFilters().Where(o => o.Cluster.IsActive)
                .SingleAsync(a => a.Id == 41);

            var emails = new string[] { "apprequests@caes.ucdavis.edu" };

            var rtValue = await _notificationService.OrderExpiredNotification(order, emails);

            return Content($"Notification sent {rtValue}");
        }

        public async Task<IActionResult> TestNotification2()
        {
            return Content(await _notificationService.ProcessOrdersInCreatedStatus([DayOfWeek.Monday, DayOfWeek.Tuesday ]));
        }

        public async Task<IActionResult> TestNotification3()
        {

            return Content(await _notificationService.NagSponsorsAboutPendingAccounts([ DayOfWeek.Monday, DayOfWeek.Tuesday ]));
        }
    }
}
