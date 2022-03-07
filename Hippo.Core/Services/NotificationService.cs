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
        Task<bool> AccountRequested(Account account);
        Task<bool> AccountDecision(Account account, bool isApproved, string overrideSponsor = null, string reason = null);
    }

    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _dbContext;
        private readonly IEmailService _emailService;
        private readonly EmailSettings _emailSettings;

        public NotificationService(AppDbContext dbContext, IEmailService emailService, IOptions<EmailSettings> emailSettings)
        {
            _dbContext = dbContext;
            _emailService = emailService;
            _emailSettings = emailSettings.Value;
        }

        public async Task<bool> AccountDecision(Account account, bool isApproved, string overrideSponsor = null, string reason = null)
        {

            try
            {
                account = await GetCompleteAccount(account);
                var sponser = String.Empty;
                if (!string.IsNullOrWhiteSpace(overrideSponsor))
                {
                    sponser = overrideSponsor;
                }
                else
                {
                    sponser = !String.IsNullOrWhiteSpace(account.Sponsor.Name) ? account.Sponsor.Name : account.Sponsor.Owner.Name;
                }
                
                var requestUrl = $"{_emailSettings.BaseUrl}"; //TODO: Only have button if approved?
                var emailTo = account.Owner.Email;

                var model = new DecisionModel()
                {
                    SponsorName = sponser,
                    RequesterName = account.Owner.Name,
                    RequestDate = account.CreatedOn.ToPacificTime().Date.Format("d"),
                    DecisionDate = account.UpdatedOn.ToPacificTime().Date.Format("d"),
                    RequestUrl = requestUrl,
                    Decision = isApproved ? "Approved" : "Rejected",
                    DecisionColor = isApproved ? DecisionModel.Colors.Approved : DecisionModel.Colors.Rejected,
                    Reason = reason,
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

        public async Task<bool> AccountRequested(Account account)
        {
            try 
            { 
                account = await GetCompleteAccount(account);
                var requestUrl = $"{_emailSettings.BaseUrl}/approve";
                var emailTo = account.Sponsor.Owner.Email; 

                var model = new NewRequestModel()
                {
                    SponsorName = account.Sponsor.Owner.Name,
                    RequesterName = account.Owner.Name,
                    RequestDate = account.CreatedOn.ToPacificTime().Date.Format("d"),
                    RequestUrl = requestUrl, 
                };

                var emailBody = await RazorTemplateEngine.RenderAsync("/Views/Emails/AccountRequest.cshtml", model);

                await _emailService.SendEmail(new[] { emailTo }, null, emailBody, "A new account request is ready for your approval");

                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Error emailing Account Request", ex) ;
                return false;
            }
        }

        private async Task<Account> GetCompleteAccount(Account account)
        {
            if(account.Owner == null || account.Sponsor == null || account.Sponsor.Owner == null)
            {
                return await _dbContext.Accounts.AsNoTracking().AsSingleQuery().Include(a => a.Owner).Include(a => a.Sponsor).ThenInclude(a => a.Owner).SingleAsync(a => a.Id == account.Id);
            }
            return account;
        }
    }
}
