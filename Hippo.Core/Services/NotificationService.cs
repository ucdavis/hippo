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
using Hippo.Email.Models;
using Hippo.Core.Extensions;
using Razor.Templating.Core;
using Mjml.Net;

namespace Hippo.Core.Services
{
    public interface INotificationService
    {
        Task<bool> AccountRequest(Request request);
        Task<bool> AccountDecision(Request request, bool isApproved, string overrideSponsor = null, string reason = null);
        Task<bool> AdminOverrideDecision(Request request, bool isApproved, User adminUser, string reason = null);
    }

    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _dbContext;
        private readonly IEmailService _emailService;
        private readonly EmailSettings _emailSettings;
        private readonly IUserService _userService;
        private readonly IMjmlRenderer _mjmlRenderer;

        public NotificationService(AppDbContext dbContext, IEmailService emailService, 
            IOptions<EmailSettings> emailSettings, IUserService userService, IMjmlRenderer mjmlRenderer)
        {
            _dbContext = dbContext;
            _emailService = emailService;
            _emailSettings = emailSettings.Value;
            _userService = userService;
            _mjmlRenderer = mjmlRenderer;
        }

        public async Task<bool> AccountDecision(Request request, bool isApproved, string overrideDecidedBy = null, string reason = null)
        {

            try
            {
                var decidedBy = String.Empty;
                if (!string.IsNullOrWhiteSpace(overrideDecidedBy))
                {
                    decidedBy = overrideDecidedBy;
                }
                else
                {
                    decidedBy = (await _userService.GetCurrentUser()).Name;
                }

                var requestUrl = $"{_emailSettings.BaseUrl}/{request.Cluster.Name}"; //TODO: Only have button if approved?
                var emailTo = request.Requester.Email;

                var group = await _dbContext.Groups.Where(g => g.ClusterId == request.ClusterId && g.Name == request.Group).SingleAsync();

                var model = new DecisionModel()
                {
                    RequestedAction = request.Action.SplitCamelCase(),
                    GroupName = group.DisplayName,
                    RequesterName = request.Requester.Name,
                    RequestDate = request.CreatedOn.ToPacificTime().Date.Format("d"),
                    DecisionDate = request.UpdatedOn.ToPacificTime().Date.Format("d"),
                    RequestUrl = requestUrl,
                    Decision = isApproved ? "Approved" : "Rejected",
                    AdminName = decidedBy,
                    DecisionColor = isApproved ? DecisionModel.Colors.Approved : DecisionModel.Colors.Rejected,
                    Reason = reason,
                    ClusterName = request.Cluster.Name,
                };

                if (!isApproved)
                {
                    model.Instructions = "Your account request has been rejected. If you believe this was done in error, please contact your sponsor directly. You will need to submit a new request, but contact your sponsor first.";
                }

                var emailBody = await _mjmlRenderer.RenderView("/Views/Emails/AccountDecision_mjml.cshtml", model);

                await _emailService.SendEmail(new[] { emailTo }, null, emailBody, $"Your account request has been {model.Decision}. {model.Instructions}");

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

                var model = new NewRequestModel()
                {
                    GroupName = group.DisplayName,
                    RequesterName = request.Requester.Name,
                    RequestDate = request.CreatedOn.ToPacificTime().Date.Format("d"),
                    RequestUrl = requestUrl,
                    ClusterName = request.Cluster.Name,
                    SupervisingPI = request.SupervisingPI,
                    Action = request.Action.SplitCamelCase(),
                };

                var emailBody = await _mjmlRenderer.RenderView("/Views/Emails/AccountRequest_mjml.cshtml", model);

                await _emailService.SendEmail(emails, null, emailBody, "A new account request is ready for your approval");

                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Error emailing Account Request", ex);
                return false;
            }
        }

        public async Task<bool> AdminOverrideDecision(Request request, bool isApproved, User adminUser, string reason = null)
        {
            try
            {
                //var requestUrl = $"{_emailSettings.BaseUrl}"; //TODO: Only have button if approved?
                var group = await _dbContext.Groups.SingleAsync(g => g.ClusterId == request.ClusterId && g.Name == request.Group);
                var emails = await GetGroupAdminEmails(group);

                var model = new DecisionModel()
                {
                    GroupName = group.DisplayName,
                    RequesterName = request.Requester.Name,
                    RequestDate = request.CreatedOn.ToPacificTime().Date.Format("d"),
                    DecisionDate = request.UpdatedOn.ToPacificTime().Date.Format("d"),
                    //RequestUrl = requestUrl,
                    Decision = isApproved ? "Approved" : "Rejected",
                    DecisionColor = isApproved ? DecisionModel.Colors.Approved : DecisionModel.Colors.Rejected,
                    Reason = reason,
                    AdminName = adminUser.Name,
                    Instructions = "An admin has acted on an account request on your behalf where you were listed as the sponsor.",
                    ClusterName = request.Cluster.Name,
                    RequestedAction = request.Action.SplitCamelCase(),
                };


                var emailBody = await _mjmlRenderer.RenderView("/Views/Emails/AdminOverrideDecision_mjml.cshtml", model);

                await _emailService.SendEmail(emails, ccEmails: new[] { adminUser.Email }, emailBody, "An admin has acted on an account request on your behalf where you were listed as the sponsor.");

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
    }
}
