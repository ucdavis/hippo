using Hippo.Core.Data;
using Hippo.Core.Domain;
using Microsoft.EntityFrameworkCore;
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
            var orders = await _dbContext.Orders.Include(a => a.Payments).Where(a => a.Status == Order.Statuses.Active && a.NextPaymentDate != null && a.NextPaymentDate.Value.Date <= DateTime.UtcNow.Date).ToListAsync();
            foreach (var order in orders) {
                //Check if the installment amount is greater than the balance remaining
                //Check if there is a payment already created 
                //check if we have a small amount remaining, if so just pay it all.

                var paymentAmount = order.InstallmentAmount;
                if(paymentAmount > Math.Round(order.BalanceRemaining, 2))
                {
                    paymentAmount = Math.Round(order.BalanceRemaining, 2);
                }
                var newBalance = Math.Round(order.BalanceRemaining - paymentAmount, 2);
                if(newBalance > 0 && newBalance < order.InstallmentAmount)
                {
                    paymentAmount = Math.Round(order.BalanceRemaining, 2);
                }


                //var payment = new Payment
                //{
                //    Order = order,
                //    Amount =  Math.Round( order.InstallmentAmount, 2),
                //    Status = Payment.Statuses.Created
                //};
                //_dbContext.Payments.Add(payment);
                //order.NextPaymentDate = order.NextPaymentDate.Value.AddMonths(1);
                //_dbContext.Orders.Update(order);
            }


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
