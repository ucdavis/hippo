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
    public interface IEmailService
    {
        Task<bool> AccountRequested(Account account);
        Task<bool> AccountDecission(Account account, bool isApproved);
    }

    public class EmailService : IEmailService
    {
        private readonly AppDbContext _dbContext;
        private readonly INotificationService _notificationService;
        private readonly EmailSettings _emailSettings;

        public EmailService(AppDbContext dbContext, INotificationService notificationService, IOptions<EmailSettings> emailSettings)
        {
            _dbContext = dbContext;
            _notificationService = notificationService;
            _emailSettings = emailSettings.Value;
        }

        public async Task<bool> AccountDecission(Account account, bool isApproved)
        {
            try
            {
                account = await GetCompleteAccount(account);
                var requestUrl = $"{_emailSettings.BaseUrl}/Fake/Request/"; //TODO: Replace when we know it
                var emailTo = account.Owner.Email;

                var model = new DecisionModel()
                {
                    SponsorName = !String.IsNullOrWhiteSpace(account.Sponsor.Name) ? account.Sponsor.Name : account.Sponsor.Owner.Name,
                    RequesterName = account.Owner.Name,
                    RequestDate = account.CreatedOn.ToPacificTime().Date.Format("d"),
                    DecisionDate = account.UpdatedOn.ToPacificTime().Date.Format("d"),
                    RequestUrl = $"{requestUrl}{account.Id}", //TODO: Use correct URL
                    Decision = isApproved ? "Approved" : "Rejected",
                    DecisionColor = isApproved ? DecisionModel.Colors.Approved : DecisionModel.Colors.Rejected,
                };

                if (!isApproved)
                {
                    model.Instructions = "Your account request has been rejected. If you believe this was done in error, please contact your sponsor directly. You will need to submit a new request, but contact your sponsor first.";
                }

                var emailBody = await RazorTemplateEngine.RenderAsync("/Views/Emails/AccountDecission.cshtml", model);

                await _notificationService.SendNotification(new[] { emailTo }, null, emailBody, $"Your account request has been {model.Decision}. {model.Instructions}");

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
                var requestUrl = $"{_emailSettings.BaseUrl}/Fake/Request/"; //TODO: Replace when we know it
                var emailTo = account.Sponsor.Owner.Email; 

                var model = new NewRequestModel()
                {
                    SponsorName = account.Sponsor.Owner.Name,
                    RequesterName = account.Owner.Name,
                    RequestDate = account.CreatedOn.ToPacificTime().Date.Format("d"),
                    RequestUrl = $"{requestUrl}{account.Id}", //TODO: Use correct URL
                };

                var emailBody = await RazorTemplateEngine.RenderAsync("/Views/Emails/AccountRequest.cshtml", model);

                await _notificationService.SendNotification(new[] { emailTo }, null, emailBody, "A new account request is ready for your approval");

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
