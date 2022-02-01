using Hippo.Core.Services;
using Hippo.Email.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Razor.Templating.Core;
using Renci.SshNet;
using Microsoft.Extensions.Options;
using Hippo.Core.Models.Settings;
using System.Text;

namespace Hippo.Web.Controllers
{
    [Authorize]
    public class TestController : Controller
    {
        public INotificationService _notificationService { get; }
        private readonly SshSettings _sshSettings;

        public TestController(INotificationService notificationService, IOptions<SshSettings> sshSettings)
        {
            _notificationService = notificationService;
            _sshSettings = sshSettings.Value;
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

        public IActionResult TestSsh()
        {
            //read file into base64 string
            //var file = System.IO.File.ReadAllBytes("D:\\Work\\GitProjects\\hippo\\Hippo.Web\\remote-api-auth.pk");
            //var base64 = System.Convert.ToBase64String(file);

            //return null;


            var rsa = Convert.FromBase64String(_sshSettings.Key);
            var stream = new MemoryStream(rsa);
            var pkFile = new PrivateKeyFile(stream);




            using (var client = new SshClient(_sshSettings.Url, _sshSettings.Name, pkFile))
            {
                client.Connect();
                var result = client.RunCommand("ls -l");
                client.Disconnect();

                return(Content( result.Result));

                //var sb = new StringBuilder();
                //foreach (var item in result.Result.Split('\n'))
                //{
                //    sb.AppendLine(item.ToString());
                //}


                //return Content(sb.ToString());
            }
        }
    }
}
