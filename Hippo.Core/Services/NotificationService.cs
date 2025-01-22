using Hippo.Core.Data;
using Hippo.Core.Domain;
using Hippo.Core.Models.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using Hippo.Core.Models.Email;
using Hippo.Core.Extensions;
using Razor.Templating.Core;
using Mjml.Net;
using Hippo.Core.Models;

namespace Hippo.Core.Services
{
    public interface INotificationService
    {
        Task<bool> AccountRequest(Request request);
        Task<bool> AccountDecision(Request request, bool isApproved, string decidedBy , string reason = null);
        Task<bool> AdminOverrideDecision(Request request, bool isApproved, User adminUser, string reason = null);
        Task<bool> SimpleNotification(SimpleNotificationModel simpleNotificationModel);

        Task<bool> AdminPaymentFailureNotification(string[] emails, string clusterName, int[] orderIds);
        Task<bool> SponsorPaymentFailureNotification(string[] emails, Order order); //Could possibly just pass the order Id, but there might be more order info we want to include
        Task<bool> OrderNotification(SimpleNotificationModel simpleNotificationModel, Order order, string[] emails, string[] ccEmails = null);
        Task<bool> OrderNotificationTwoButton(SimpleNotificationModel simpleNotificationModel, Order order, string[] emails, string[] ccEmails = null);
        Task<bool> OrderPaymentNotification(Order order, string[] emails, EmailOrderPaymentModel orderPaymentModel);
        Task<bool> OrderExpiredNotification(Order order, string[] emails);
    }

    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _dbContext;
        private readonly IEmailService _emailService;
        private readonly EmailSettings _emailSettings;
        private readonly IMjmlRenderer _mjmlRenderer;

        public NotificationService(AppDbContext dbContext, IEmailService emailService,
            IOptions<EmailSettings> emailSettings, IMjmlRenderer mjmlRenderer)
        {
            _dbContext = dbContext;
            _emailService = emailService;
            _emailSettings = emailSettings.Value;
            _mjmlRenderer = mjmlRenderer;
        }

        public async Task<bool> AccountDecision(Request request, bool isApproved, string decidedBy, string details = "")
        {

            try
            {

                var requestUrl = $"{_emailSettings.BaseUrl}/{request.Cluster.Name}"; //TODO: Only have button if approved?
                var emailTo = request.Requester.Email;

                var group = await _dbContext.Groups.Where(g => g.ClusterId == request.ClusterId && g.Name == request.Group).SingleAsync();
                var requestData = request.GetAccountRequestData();
                var message = details;
                if (!isApproved && string.IsNullOrWhiteSpace(message))
                    message = "Your account request has been rejected. If you believe this was done in error, please contact " +
                              "your sponsor directly. You will need to submit a new request, but contact your sponsor first.";

                var model = new DecisionModel()
                {
                    UcdLogoUrl = $"{_emailSettings.BaseUrl}/media/hpcLogo.png",
                    RequestedAction = request.Action.SplitCamelCase(),
                    GroupName = group.DisplayName,
                    RequesterName = request.Requester.Name,
                    RequestDate = request.CreatedOn.ToPacificTime().Date.Format("d"),
                    DecisionDate = request.UpdatedOn.ToPacificTime().Date.Format("d"),
                    RequestUrl = requestUrl,
                    Decision = isApproved ? "Approved" : "Rejected",
                    AdminName = decidedBy,
                    DecisionColor = isApproved ? DecisionModel.Colors.Approved : DecisionModel.Colors.Rejected,
                    DecisionDetails = message,
                    ClusterName = request.Cluster.Name,
                    AccessTypes = requestData.AccessTypes,
                    SupervisingPI = requestData.SupervisingPI
                };

                var emailBody = await _mjmlRenderer.RenderView("/Views/Emails/AccountDecision_mjml.cshtml", model);

                var emailModel = new EmailModel
                {
                    Emails = new[] { emailTo },
                    HtmlBody = emailBody,
                    TextBody = message,
                    Subject = $"Hippo Request ({model.RequestedAction}) {model.Decision}"
                };

                await _emailService.SendEmail(emailModel);

                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Error emailing Account Request", ex);
                return false;
            }
        }

        public async Task<bool> AccountRequest(Request request)
        {
            try
            {
                var group = await _dbContext.Groups.SingleAsync(g => g.ClusterId == request.ClusterId && g.Name == request.Group);
                var requestUrl = $"{_emailSettings.BaseUrl}/{request.Cluster.Name}/approve";
                var emails = await GetGroupAdminEmails(group);
                var requestData = request.GetAccountRequestData();

                var model = new NewRequestModel()
                {
                    UcdLogoUrl = $"{_emailSettings.BaseUrl}/media/hpcLogo.png",
                    GroupName = group.DisplayName,
                    RequesterName = request.Requester.Name,
                    RequestDate = request.CreatedOn.ToPacificTime().Date.Format("d"),
                    RequestUrl = requestUrl,
                    ClusterName = request.Cluster.Name,
                    SupervisingPI = requestData.SupervisingPI,
                    Action = request.Action.SplitCamelCase(),
                    AccessTypes = requestData.AccessTypes
                };

                var htmlBody = await _mjmlRenderer.RenderView("/Views/Emails/AccountRequest_mjml.cshtml", model);

                var emailModel = new EmailModel
                {
                    Emails = emails,
                    HtmlBody = htmlBody,
                    TextBody = $"A new request ({model.Action}) is ready for your approval",
                    Subject = $"Hippo Request ({model.Action}) Submitted"
                };

                await _emailService.SendEmail(emailModel);

                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Error emailing Account Request", ex);
                return false;
            }
        }

        public async Task<bool> SimpleNotification(SimpleNotificationModel simpleNotificationModel)
        {
            if (string.IsNullOrWhiteSpace(simpleNotificationModel.UcdLogoUrl))
            {
                simpleNotificationModel.UcdLogoUrl = $"{_emailSettings.BaseUrl}/media/hpcLogo.png";
            }
            try
            {
                var emailModel = new EmailModel
                {
                    Emails = simpleNotificationModel.Emails,
                    CcEmails = simpleNotificationModel.CcEmails ?? Array.Empty<string>(),
                    Subject = simpleNotificationModel.Subject,
                    TextBody = string.Join($"{Environment.NewLine}{Environment.NewLine}", simpleNotificationModel.Paragraphs),
                    HtmlBody = await _mjmlRenderer.RenderView("/Views/Emails/SimpleNotification_mjml.cshtml", simpleNotificationModel)
                };

                await _emailService.SendEmail(emailModel);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Error sending email", ex);
                return false;
            }

        }

        public async Task<bool> AdminOverrideDecision(Request request, bool isApproved, User adminUser, string details = "")
        {
            try
            {
                //var requestUrl = $"{_emailSettings.BaseUrl}"; //TODO: Only have button if approved?
                var group = await _dbContext.Groups.SingleAsync(g => g.ClusterId == request.ClusterId && g.Name == request.Group);
                var emails = await GetGroupAdminEmails(group);

                var message = string.IsNullOrWhiteSpace(details)
                    ? "An admin has acted on an account request on your behalf where you were listed as the sponsor."
                    : $"{details} (An admin has acted on an account request on your behalf where you were listed as the sponsor.)";

                var model = new DecisionModel()
                {
                    UcdLogoUrl = $"{_emailSettings.BaseUrl}/media/hpcLogo.png",
                    GroupName = group.DisplayName,
                    RequesterName = request.Requester.Name,
                    RequestDate = request.CreatedOn.ToPacificTime().Date.Format("d"),
                    DecisionDate = request.UpdatedOn.ToPacificTime().Date.Format("d"),
                    //RequestUrl = requestUrl,
                    Decision = isApproved ? "Approved" : "Rejected",
                    DecisionColor = isApproved ? DecisionModel.Colors.Approved : DecisionModel.Colors.Rejected,
                    DecisionDetails = message,
                    AdminName = adminUser.Name,
                    ClusterName = request.Cluster.Name,
                    RequestedAction = request.Action.SplitCamelCase(),
                };


                var htmlBody = await _mjmlRenderer.RenderView("/Views/Emails/AdminOverrideDecision_mjml.cshtml", model);

                await _emailService.SendEmail(new EmailModel
                {
                    Emails = emails,
                    CcEmails = new[] { adminUser.Email },
                    HtmlBody = htmlBody,
                    TextBody = message,
                    Subject = $"Hippo Request ({model.RequestedAction}) {model.Decision}"
                });

                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Error emailing Account Request", ex);
                return false;
            }
        }

        private async Task<string[]> GetGroupAdminEmails(Group group)
        {
            var groupAdminEmails = await _dbContext.Groups
                .Where(g => g.Id == group.Id)
                .SelectMany(g => g.AdminAccounts)
                .Select(a => a.Owner != null ? a.Owner.Email : a.Email)
                .Where(e => e != null)
                .ToArrayAsync();
            return groupAdminEmails;
        }

        public async Task<bool> AdminPaymentFailureNotification(string[] emails, string clusterName, int[] orderIds)
        {
            try
            {
                var message = "The payment for one or more orders in hippo have failed.";

                var model = new OrderNotificationModel()
                {
                    UcdLogoUrl = $"{_emailSettings.BaseUrl}/media/hpcLogo.png",
                    Subject = "Payment failed",
                    Header = "Order Payment Failed",
                    Paragraphs = new List<string>(),
                };
                foreach (var orderId in orderIds)
                {
                    model.Paragraphs.Add($"{_emailSettings.BaseUrl}/{clusterName}/order/details/{orderId}");

                }

                var htmlBody = await _mjmlRenderer.RenderView("/Views/Emails/OrderAdminPaymentFail_mjml.cshtml", model);

                await _emailService.SendEmail(new EmailModel
                {
                    Emails = emails,
                    CcEmails = null,
                    HtmlBody = htmlBody,
                    TextBody = message,
                    Subject = model.Subject,
                });

                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Error emailing Sponsor Payment Failure Notification", ex);
                return false;
            }
        }

        public async Task<bool> SponsorPaymentFailureNotification(string[] emails, Order order)
        {
            try
            {
                var message = "The payment for the following order has failed. Please update your billing information in Hippo.";

                var model = new OrderNotificationModel()
                {
                    UcdLogoUrl = $"{_emailSettings.BaseUrl}/media/hpcLogo.png",
                    ButtonUrl = $"{_emailSettings.BaseUrl}/{order.Cluster.Name}/order/details/{order.Id}",
                    Subject = "Payment failed",
                    Header = "Order Payment Failed",
                    Paragraphs = new List<string>(),
                };
                model.Paragraphs.Add($"Order: {order.Name}");
                model.Paragraphs.Add($"Order Id: {order.Id}");
                model.Paragraphs.Add("The payment for this order has failed.");
                model.Paragraphs.Add("This is most likely due to a Aggie Enterprise Chart String which is no longer valid.");
                model.Paragraphs.Add("The order details will have the validation message from Aggie Enterprise.");
                model.Paragraphs.Add("Please update your billing information in Hippo.");

                var htmlBody = await _mjmlRenderer.RenderView("/Views/Emails/OrderNotification_mjml.cshtml", model);

                await _emailService.SendEmail(new EmailModel
                {
                    Emails = emails,
                    CcEmails = null,
                    HtmlBody = htmlBody,
                    TextBody = message,
                    Subject = model.Subject,
                });

                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Error emailing Sponsor Payment Failure Notification", ex);
                return false;
            }
        }

        public async Task<bool> OrderNotification(SimpleNotificationModel simpleNotificationModel, Order order, string[] emails, string[] ccEmails = null)
        {
            try
            {
                var message = simpleNotificationModel.Paragraphs.FirstOrDefault();

                var model = new OrderNotificationModel()
                {
                    UcdLogoUrl = $"{_emailSettings.BaseUrl}/media/hpcLogo.png",
                    ButtonUrl = $"{_emailSettings.BaseUrl}/{order.Cluster.Name}/order/details/{order.Id}",
                    Subject = simpleNotificationModel.Subject,
                    Header = simpleNotificationModel.Header,
                    Paragraphs = simpleNotificationModel.Paragraphs,
                };


                var htmlBody = await _mjmlRenderer.RenderView("/Views/Emails/OrderNotification_mjml.cshtml", model);

                await _emailService.SendEmail(new EmailModel
                {
                    Emails = emails,
                    CcEmails = ccEmails,
                    HtmlBody = htmlBody,
                    TextBody = message,
                    Subject = simpleNotificationModel.Subject,
                });

                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Error emailing Order Notification", ex);
                return false;
            }
        }

        public async Task<bool> OrderNotificationTwoButton(SimpleNotificationModel simpleNotificationModel, Order order, string[] emails, string[] ccEmails = null)
        {
            try
            {
                //Join the simple notification paragraphs into a single string with new lines
                var message = string.Join(Environment.NewLine, simpleNotificationModel.Paragraphs);

                var model = new OrderNotificationModel()
                {
                    UcdLogoUrl = $"{_emailSettings.BaseUrl}/media/hpcLogo.png",
                    ButtonUrl = $"{_emailSettings.BaseUrl}/{order.Cluster.Name}/order/details/{order.Id}",
                    Subject = simpleNotificationModel.Subject,
                    Header = simpleNotificationModel.Header,
                    Paragraphs = simpleNotificationModel.Paragraphs,
                    ButtonTwoText = "Order Replacement",
                    ButtonTwoUrl = $"{_emailSettings.BaseUrl}/{order.Cluster.Name}/product/index",                    
                };


                var htmlBody = await _mjmlRenderer.RenderView("/Views/Emails/OrderNotificationTwoButton_mjml.cshtml", model);

                await _emailService.SendEmail(new EmailModel
                {
                    Emails = emails,
                    CcEmails = ccEmails,
                    HtmlBody = htmlBody,
                    TextBody = message,
                    Subject = simpleNotificationModel.Subject,
                });

                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Error emailing Order Notification", ex);
                return false;
            }
        }

        public async Task<bool> OrderPaymentNotification(Order order, string[] emails, EmailOrderPaymentModel orderPaymentModel)
        {
            try
            {
                var message = "Payment Processed";
                var model = new EmailOrderPaymentModel()
                {
                    UcdLogoUrl = $"{_emailSettings.BaseUrl}/media/hpcLogo.png",
                    ButtonUrl = $"{_emailSettings.BaseUrl}/{order.Cluster.Name}/order/details/{order.Id}",
                    Subject = orderPaymentModel.Subject,
                    Header = orderPaymentModel.Header,
                    Transfers = orderPaymentModel.Transfers,
                    ButtonText = "View Order",
                };

                var htmlBody = await _mjmlRenderer.RenderView("/Views/Emails/OrderPaymentNotification_mjml.cshtml", model);

                await _emailService.SendEmail(new EmailModel
                {
                    Emails = emails,
                    CcEmails = null,
                    HtmlBody = htmlBody,
                    TextBody = message,
                    Subject = model.Subject,
                });

                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Error emailing Order Payment Notification", ex);
                return false;
            }
        }

        /// <summary>
        /// This is for when the order has expired and we want to send a ticket to service now (or whereever the cluster email indicates)
        /// </summary>
        /// <param name="order"></param>
        /// <param name="emails">Get from Order.Cluster.Email</param>
        /// <returns></returns>
        public async Task<bool> OrderExpiredNotification(Order order, string[] emails)
        {
            try
            {
                var body = new List<string>();
                body.Add("Category: Request");
                body.Add("Subcategory: New");
                body.Add($"Caller: {order.PrincipalInvestigator.Name}");
                body.Add("ConfigurationItem: OOR HPC - High Performance Computing");
                body.Add($"Cluster Name: {order.Cluster.Name}");
                body.Add($"Email: {order.PrincipalInvestigator.Email}");
                body.Add($"Account Kerberos: {order.PrincipalInvestigator.Kerberos}");
                body.Add($"order: {_emailSettings.BaseUrl}/{order.Cluster.Name}/order/details/{order.Id}");
                body.Add($"Expiration Date: {order.ExpirationDate.ToPacificTime().Value.Date.Format("d")}");


                var emailModel = new EmailModel
                {
                    Emails = emails,
                    Subject = "Hippo Order Expired",
                    TextBody = string.Join(Environment.NewLine, body),
                };

                await _emailService.SendEmail(emailModel); //Send without html body
            }
            catch (Exception ex)
            {
                Log.Error("Error emailing Order Expired Notification", ex);
                return false;

            }
            return true;
        }

    }
}
