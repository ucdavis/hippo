using Hippo.Core.Data;
using Hippo.Core.Domain;
using Hippo.Core.Models.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using Hippo.Core.Models.Email;
using Hippo.Core.Extensions;
using Mjml.Net;



namespace Hippo.Core.Services
{
    public interface INotificationService
    {
        Task<bool> AccountRequest(Request request);
        Task<bool> GroupRequest(Request request);
        Task<bool> AccountDecision(Request request, bool isApproved, string decidedBy, string reason = null);
        Task<bool> GroupDecision(Request request, bool isApproved, string decidedBy, string reason = null);
        Task<bool> AdminOverrideDecision(Request request, bool isApproved, User adminUser, string reason = null);
        Task<bool> SimpleNotification(SimpleNotificationModel simpleNotificationModel);

        Task<bool> AdminPaymentFailureNotification(string[] emails, string clusterName, int[] orderIds);
        Task<bool> SponsorPaymentFailureNotification(string[] emails, Order order); //Could possibly just pass the order Id, but there might be more order info we want to include
        Task<bool> OrderNotification(SimpleNotificationModel simpleNotificationModel, Order order, string[] emails, string[] ccEmails = null);
        Task<bool> OrderNotificationTwoButton(SimpleNotificationModel simpleNotificationModel, Order order, string[] emails, string[] ccEmails = null);
        Task<bool> OrderPaymentNotification(Order order, string[] emails, EmailOrderPaymentModel orderPaymentModel);
        Task<bool> OrderExpiredNotification(Order order, string[] emails);

        Task<string> ProcessOrdersInCreatedStatus(DayOfWeek[] daysOfWeekToRun);
        Task<string> NagSponsorsAboutPendingAccounts(DayOfWeek[] daysOfWeekToRun);
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

                var accessTypes = new List<string>();
                var supervisingPI = "";
                var groupName = "";

                switch (request.Action)
                {
                    case Request.Actions.CreateAccount:
                    case Request.Actions.AddAccountToGroup:
                        var accountData = request.GetAccountRequestData();
                        accessTypes = accountData.AccessTypes;
                        supervisingPI = accountData.SupervisingPI;
                        groupName = await _dbContext.Groups
                            .Where(g => g.ClusterId == request.ClusterId && g.Name == request.Group)
                            .Select(g => g.Name == g.DisplayName ? g.DisplayName : $"{g.DisplayName} ({g.Name})")
                            .SingleAsync();
                        break;
                }


                var message = details;
                if (!isApproved && string.IsNullOrWhiteSpace(message))
                    message = "Your account request has been rejected. If you believe this was done in error, please contact " +
                              "your sponsor directly. You will need to submit a new request, but contact your sponsor first.";

                var model = new DecisionModel()
                {
                    UcdLogoUrl = $"{_emailSettings.BaseUrl}/media/hpcLogo.png",
                    RequestedAction = request.Action.SplitCamelCase(),
                    GroupName = groupName,
                    RequesterName = request.Requester.Name,
                    RequestDate = request.CreatedOn.ToPacificTime().Date.Format("d"),
                    DecisionDate = request.UpdatedOn.ToPacificTime().Date.Format("d"),
                    RequestUrl = requestUrl,
                    Decision = isApproved ? "Approved" : "Rejected",
                    AdminName = decidedBy,
                    DecisionColor = isApproved ? DecisionModel.Colors.Approved : DecisionModel.Colors.Rejected,
                    DecisionDetails = message,
                    ClusterName = request.Cluster.Name,
                    AccessTypes = accessTypes,
                    SupervisingPI = supervisingPI
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
                Log.Error("Error emailing Account Request Decision", ex);
                return false;
            }
        }

        public async Task<bool> GroupDecision(Request request, bool isApproved, string decidedBy, string details = "")
        {

            try
            {

                var requestUrl = $"{_emailSettings.BaseUrl}/{request.Cluster.Name}"; //TODO: Only have button if approved?
                var emailTo = request.Requester.Email;

                var groupName = "";

                switch (request.Action)
                {
                    case Request.Actions.CreateGroup:
                        var createGroupData = request.GetCreateGroupRequestData();
                        groupName = createGroupData.Name == createGroupData.DisplayName
                            ? createGroupData.Name
                            : $"{createGroupData.DisplayName} ({createGroupData.Name})";
                        break;
                }

                var message = details;
                if (!isApproved && string.IsNullOrWhiteSpace(message))
                    message = "Your group request has been rejected. If you believe this was done in error, please contact " +
                              "your cluster admin.";

                var model = new DecisionModel()
                {
                    UcdLogoUrl = $"{_emailSettings.BaseUrl}/media/hpcLogo.png",
                    RequestedAction = request.Action.SplitCamelCase(),
                    GroupName = groupName,
                    RequesterName = request.Requester.Name,
                    RequestDate = request.CreatedOn.ToPacificTime().Date.Format("d"),
                    DecisionDate = request.UpdatedOn.ToPacificTime().Date.Format("d"),
                    RequestUrl = requestUrl,
                    Decision = isApproved ? "Approved" : "Rejected",
                    AdminName = decidedBy,
                    DecisionColor = isApproved ? DecisionModel.Colors.Approved : DecisionModel.Colors.Rejected,
                    DecisionDetails = message,
                    ClusterName = request.Cluster.Name,
                };

                var emailBody = await _mjmlRenderer.RenderView("/Views/Emails/GroupDecision_mjml.cshtml", model);

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
                Log.Error("Error emailing Group Request Decision", ex);
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
                    AccessTypes = requestData.AccessTypes,
                    Instructions = "As a group admin who can sponsor new accounts and group memberships on this cluster, you have received this request. Please click on the View Request button to approve or deny this request."
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

        public async Task<bool> GroupRequest(Request request)
        {
            try
            {
                var requestUrl = $"{_emailSettings.BaseUrl}/{request.Cluster.Name}/approve";
                var emails = await GetClusterAdminEmails(request.ClusterId);
                var requestData = request.GetCreateGroupRequestData();

                var model = new NewRequestModel()
                {
                    UcdLogoUrl = $"{_emailSettings.BaseUrl}/media/hpcLogo.png",
                    GroupName = requestData.Name + $"(Display Name: {requestData.DisplayName})",
                    RequesterName = request.Requester.Name,
                    RequestDate = request.CreatedOn.ToPacificTime().Date.Format("d"),
                    RequestUrl = requestUrl,
                    ClusterName = request.Cluster.Name,
                    Action = request.Action.SplitCamelCase(),
                    Instructions = $"A user on cluster {request.Cluster.Name} has requested creation of a group."
                };

                var htmlBody = await _mjmlRenderer.RenderView("/Views/Emails/GroupRequest_mjml.cshtml", model);

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

        private async Task<string[]> GetClusterAdminEmails(int clusterId)
        {
            var clusterAdminEmails = await _dbContext.Permissions
                .Where(p => p.ClusterId == clusterId && p.Role.Name == Role.Codes.ClusterAdmin)
                .Select(p => p.User.Email)
                .Distinct()
                .ToArrayAsync();
            return clusterAdminEmails;
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

        public async Task<string> ProcessOrdersInCreatedStatus(DayOfWeek[] daysOfWeekToRun)
        {
            //if daysOfWeekToRun isn't today, return
            if (!daysOfWeekToRun.Contains(DateTime.UtcNow.DayOfWeek))
            {
                return "Not the correct day of the week to run this process";
            }


            //Get all orders in created status across all clusters, group them by cluster and by sponsor
            var orders = await _dbContext.Orders
                .Include(o => o.Cluster)
                .Include(o => o.PrincipalInvestigator)
                // We don't want to filter on inactive PIs, we still do on inactive clusters
                .IgnoreQueryFilters().Where(o => o.Cluster.IsActive)
                .Where(o => o.Status == Order.Statuses.Created)
                .ToListAsync();

            if (orders.Count == 0)
            {
                return "No orders in created status today";
            }

            var exceptionsEncountered = false;


            var groupedOrders = orders.GroupBy(o => new { o.ClusterId, o.PrincipalInvestigatorId });
            foreach (var group in groupedOrders)
            {
                var cluster = group.First().Cluster;
                var sponsor = group.First().PrincipalInvestigator;
                var sponsorEmail = sponsor.Email;
                var sponsorName = sponsor.Name;
                var orderIds = group.Select(o => o.Id).ToArray();
                var clusterName = cluster.Name;
                var emails = new[] { sponsorEmail };

                try
                {
                    var message = "You have one or more orders in the Created status awaiting your action.";

                    var model = new OrderNotificationModel()
                    {
                        UcdLogoUrl = $"{_emailSettings.BaseUrl}/media/hpcLogo.png",
                        Subject = "Orders awaiting your action",
                        Header = "Orders in Created Status",
                        ButtonText = "View Orders",
                        ButtonUrl = $"{_emailSettings.BaseUrl}/{clusterName}/order/myorders",
                        Paragraphs = new List<string>(),
                    };
                    foreach (var orderId in orderIds)
                    {
                        model.Paragraphs.Add($"{_emailSettings.BaseUrl}/{clusterName}/order/details/{orderId}");

                    }

                    var htmlBody = await _mjmlRenderer.RenderView("/Views/Emails/OrdersInCreated_mjml.cshtml", model);

                    await _emailService.SendEmail(new EmailModel
                    {
                        Emails = emails,
                        CcEmails = null,
                        HtmlBody = htmlBody,
                        TextBody = message,
                        Subject = model.Subject,
                    });

                }
                catch (Exception ex)
                {
                    Log.Error("Error emailing Sponsor Nag email", ex);
                    exceptionsEncountered = true;
                }

            }

            return exceptionsEncountered ? "Exceptions Encountered" : "Success";
        }

        public async Task<string> NagSponsorsAboutPendingAccounts(DayOfWeek[] daysOfWeekToRun)
        {
            //if daysOfWeekToRun isn't today, return
            if (!daysOfWeekToRun.Contains(DateTime.UtcNow.DayOfWeek))
            {
                return "Not the correct day of the week to run this process";
            }
            //Get all the pending account requests
            var requests = await _dbContext.Requests
                .Include(r => r.Cluster)
                .Where(r => r.Status == Request.Statuses.PendingApproval)
                .ToListAsync();

            if (requests.Count == 0)
            {
                return "No pending account requests today";
            }

            var exceptionsEncountered = false;

            foreach (var request in requests.GroupBy(a => new { a.Cluster, a.Group }))
            {
                try
                {

                    var cluster = request.First().Cluster;

                    //Looks like I can't include the group in the Request call and have to specifically get it
                    var group = await _dbContext.Groups.SingleAsync(g => g.ClusterId == cluster.Id && g.Name == request.Key.Group);
                    var groupAdmins = await GetGroupAdminEmails(group);

                    var model = new PendingDecisionsModel()
                    {
                        UcdLogoUrl = $"{_emailSettings.BaseUrl}/media/hpcLogo.png",
                        Subject = "Pending Account Requests",
                        Header = "Pending Account Requests",
                        ButtonText = "View Requests",
                        ButtonUrl = $"{_emailSettings.BaseUrl}/{cluster.Name}/approve",
                        GroupName = group.DisplayName,
                        ClusterName = cluster.Name,
                    };

                    var htmlBody = await _mjmlRenderer.RenderView("/Views/Emails/PendingDecisions_mjml.cshtml", model);

                    await _emailService.SendEmail(new EmailModel
                    {
                        Emails = groupAdmins,
                        CcEmails = null,
                        HtmlBody = htmlBody,
                        TextBody = $"You have pending account requests in {cluster.Name}, please review them.",
                        Subject = model.Subject,
                    });
                }
                catch (Exception ex)
                {
                    Log.Error("Error emailing Pending Requests Nag email", ex);
                    exceptionsEncountered = true;
                }
            }
            return exceptionsEncountered ? "Exceptions Encountered" : "Success";

        }
    }
}
