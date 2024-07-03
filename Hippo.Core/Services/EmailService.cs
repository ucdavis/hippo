using Hippo.Core.Models.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using Microsoft.Extensions.Options;

namespace Hippo.Core.Services
{
    public class EmailModel
    {
        public string[] Emails { get; set; } = Array.Empty<string>();
        public string[] CcEmails { get; set; } = Array.Empty<string>();
        public string TextBody { get; set; } = "";
        public string HtmlBody { get; set; } = "";
        public string Subject { get; set; } = "";
    }

    public interface IEmailService
    {
        Task SendEmail(EmailModel emailModel);
    }

    public class EmailService : IEmailService
    {
        private readonly SmtpClient _client;
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
            _client = new SmtpClient(_emailSettings.Host, _emailSettings.Port) { Credentials = new NetworkCredential(_emailSettings.UserName, _emailSettings.Password), EnableSsl = true };
        }

        public async Task SendEmail(EmailModel emailModel)
        {
            if (_emailSettings.DisableSend.Equals("Yes", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            using (var message = new MailMessage
            {
                From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                Subject = emailModel.Subject
            })
            {
                foreach (var email in emailModel.Emails)
                {
                    message.To.Add(new MailAddress(email, email));
                }

                foreach (var ccEmail in emailModel.CcEmails)
                {
                    message.CC.Add(new MailAddress(ccEmail));
                }

                if (!string.IsNullOrWhiteSpace(_emailSettings.BccEmail))
                {
                    message.Bcc.Add(new MailAddress(_emailSettings.BccEmail));
                }

                message.Body = emailModel.TextBody;

                if (!string.IsNullOrWhiteSpace(emailModel.HtmlBody))
                {
                    var htmlView = AlternateView.CreateAlternateViewFromString(emailModel.HtmlBody, new ContentType(MediaTypeNames.Text.Html));
                    message.AlternateViews.Add(htmlView);
                }

                await _client.SendMailAsync(message);
            }
        }

        public async Task SendSampleEmailMessage(string email, string body)
        {
            if (_emailSettings.DisableSend.Equals("Yes", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            using (var message = new MailMessage { From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName), Subject = "Hippo Notification" })
            {
                message.To.Add(new MailAddress(email, email));

                // body is our fallback text and we'll add an HTML view as an alternate.
                message.Body = "Sample Email Text";

                var htmlView = AlternateView.CreateAlternateViewFromString(body, new ContentType(MediaTypeNames.Text.Html));
                message.AlternateViews.Add(htmlView);

                await _client.SendMailAsync(message);
            }
        }
    }

}
