using Hippo.Core.Data;
using Hippo.Core.Domain;
using Hippo.Core.Models.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public Task<bool> AccountRequested(Account account)
        {
            throw new NotImplementedException();
        }
    }
}
