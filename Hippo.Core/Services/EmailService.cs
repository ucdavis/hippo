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

        public Task<bool> AccountDecission(Account account, bool isApproved)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> AccountRequested(Account account)
        {
            try 
            { 
                account = await CheckForMissingData(account);
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

                await _notificationService.SendNotification(new [] {emailTo },null, emailBody, emailTo, "A new account request is ready for your approval");  

                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Error emailing Account Request", ex) ;
                return false;
            }
        }

        private async Task<Account> CheckForMissingData(Account account)
        {
            if(account.Owner == null || account.Sponsor == null || account.Sponsor.Owner == null)
            {
                return await _dbContext.Accounts.AsNoTracking().Include(a => a.Owner).Include(a => a.Sponsor).ThenInclude(a => a.Owner).SingleAsync(a => a.Id == account.Id);
            }
            return account;
        }
    }
}
