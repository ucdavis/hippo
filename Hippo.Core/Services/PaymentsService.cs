using Hippo.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hippo.Core.Services
{
    public interface IPaymentsService
    {
        public Task<bool> CreatePayments();
        public Task<bool> NotifyAboutFailedPayments();
    }

    public class PaymentsService : IPaymentsService
    {
        private readonly AppDbContext _dbContext;

        public PaymentsService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<bool> CreatePayments()
        {
            //Do a check on all active orders that don't have a next payment date and a balance > 0



            // Create payments
            return true;
        }

        public async Task<bool> NotifyAboutFailedPayments()
        {
            // Notify about failed payments
            return true;
        }
    }
}
