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

        public NotificationService(AppDbContext dbContext, IEmailService emailService, IOptions<EmailSettings> emailSettings, IUserService userService)
        {
            _dbContext = dbContext;
            _emailService = emailService;
            _emailSettings = emailSettings.Value;
            _userService = userService;
        }

        public async Task<bool> AccountDecision(Request request, bool isApproved, string overrideDecidedBy = null, string reason = null)
        {

            try
            {
                var account = request.Account;
                var decidedBy = String.Empty;
                if (!string.IsNullOrWhiteSpace(overrideDecidedBy))
                {
                    decidedBy = overrideDecidedBy;
                }
                else
                {
                    decidedBy = (await _userService.GetCurrentUser()).Name;
                }

                var requestUrl = $"{_emailSettings.BaseUrl}/{account.Cluster.Name}"; //TODO: Only have button if approved?
                var emailTo = account.Owner.Email;

                var model = new DecisionModel()
                {
                    GroupName = request.Group.DisplayName,
                    RequesterName = account.Owner.Name,
                    RequestDate = account.CreatedOn.ToPacificTime().Date.Format("d"),
                    DecisionDate = account.UpdatedOn.ToPacificTime().Date.Format("d"),
                    RequestUrl = requestUrl,
                    Decision = isApproved ? "Approved" : "Rejected",
                    AdminName = decidedBy,
                    DecisionColor = isApproved ? DecisionModel.Colors.Approved : DecisionModel.Colors.Rejected,
                    Reason = reason,
                    ClusterName = account.Cluster.Description,
                };

                if (!isApproved)
                {
                    model.Instructions = "Your account request has been rejected. If you believe this was done in error, please contact your sponsor directly. You will need to submit a new request, but contact your sponsor first.";
                }

                var emailBody = await RazorTemplateEngine.RenderAsync("/Views/Emails/AccountDecission.cshtml", model);

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
                var account = request.Account;
                var requestUrl = $"{_emailSettings.BaseUrl}/{account.Cluster.Name}/approve";
                var emails = await GetGroupAdminEmails(account);

                var model = new NewRequestModel()
                {
                    GroupName = request.Group.DisplayName,
                    RequesterName = account.Owner.Name,
                    RequestDate = account.CreatedOn.ToPacificTime().Date.Format("d"),
                    RequestUrl = requestUrl,
                    ClusterName = account.Cluster.Description,
                };

                var emailBody = await RazorTemplateEngine.RenderAsync("/Views/Emails/AccountRequest.cshtml", model);

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
                var account = request.Account;
                //var requestUrl = $"{_emailSettings.BaseUrl}"; //TODO: Only have button if approved?
                var emails = await GetGroupAdminEmails(account);

                var model = new DecisionModel()
                {
                    GroupName = request.Group.DisplayName,
                    RequesterName = account.Owner.Name,
                    RequestDate = account.CreatedOn.ToPacificTime().Date.Format("d"),
                    DecisionDate = account.UpdatedOn.ToPacificTime().Date.Format("d"),
                    //RequestUrl = requestUrl,
                    Decision = isApproved ? "Approved" : "Rejected",
                    DecisionColor = isApproved ? DecisionModel.Colors.Approved : DecisionModel.Colors.Rejected,
                    Reason = reason,
                    AdminName = adminUser.Name,
                    Instructions = "An admin has acted on an account request on your behalf where you were listed as the sponsor.",
                    ClusterName = account.Cluster.Description,
                };


                var emailBody = await RazorTemplateEngine.RenderAsync("/Views/Emails/AdminOverrideDecission.cshtml", model);

                await _emailService.SendEmail(emails, ccEmails: new[] { adminUser.Email }, emailBody, "An admin has acted on an account request on your behalf where you were listed as the sponsor.");

                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Error emailing Account Request", ex);
                return false;
            }
        }

        private async Task<string[]> GetGroupAdminEmails(Account account)
        {
            var groupAdminEmails = await _dbContext.Users
                .Where(u => u.Permissions.Any(p =>
                    account.GroupAccounts.Select(ga => (int?)ga.GroupId).Contains(p.GroupId)
                    && p.Role.Name == Role.Codes.GroupAdmin
                    && p.ClusterId == account.ClusterId))
                .Select(u => u.Email)
                .ToArrayAsync();
            return groupAdminEmails;
        }
    }
}
