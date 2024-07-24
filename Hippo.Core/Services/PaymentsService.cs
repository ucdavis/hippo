using Hippo.Core.Data;
using Hippo.Core.Domain;
using Hippo.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Hippo.Core.Domain.Product;

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
        private readonly IHistoryService _historyService;

        public PaymentsService(AppDbContext dbContext, IHistoryService historyService)
        {
            _dbContext = dbContext;
            _historyService = historyService;
        }
        public async Task<bool> CreatePayments()
        {
            //Do a check on all active orders that don't have a next payment date and a balance > 0 
            var orderCheck = await _dbContext.Orders.Where(a => a.Status == Order.Statuses.Active && a.NextPaymentDate == null && a.BalanceRemaining > 0).ToListAsync();
            foreach (var order in orderCheck)
            {
                if (order.Payments.Any(a => a.CreatedById == null && (a.Status == Payment.Statuses.Created || a.Status == Payment.Statuses.Processing)))
                {
                    Log.Information("Skipping order {0} because it has a created or processing payment", order.Id);
                    continue;
                }
                SetNextPaymentDate(order);

                _dbContext.Orders.Update(order);
                await _dbContext.SaveChangesAsync();
            }
            
            var orders = await _dbContext.Orders.Include(a => a.Payments).Include(a => a.Cluster).Include(a => a.PrincipalInvestigator).Where(a => a.Status == Order.Statuses.Active && a.NextPaymentDate != null && a.NextPaymentDate.Value.Date <= DateTime.UtcNow.Date).ToListAsync();
            foreach (var order in orders) {

                if(order.Total <= order.Payments.Where(a => a.Status == Payment.Statuses.Completed).Sum(a => a.Amount))
                {
                    order.Status = Order.Statuses.Completed;
                    order.NextPaymentDate = null;
                    //TODO: A notification? This Shold happen when sloth updates, but just in case.
                    _dbContext.Orders.Update(order);
                    await _dbContext.SaveChangesAsync();
                    continue;
                }

                //Need to ignore manual ones...
                if(order.Payments.Any(a => a.CreatedById == null && (a.Status == Payment.Statuses.Created || a.Status == Payment.Statuses.Processing)))
                {
                    Log.Information("Skipping order {0} because it has a created or processing payment", order.Id);
                    continue;
                }

                //TODO: Recalculate balance remaining in case DB value is wrong?
                var localBalance = Math.Round( order.Total - order.Payments.Where(a => a.Status == Payment.Statuses.Completed || a.Status == Payment.Statuses.Processing).Sum(a => a.Amount), 2);
                if (localBalance != order.BalanceRemaining)
                {
                    Log.Information("Order {0} has a balance mismatch. Local: {1} DB: {2}", order.Id, localBalance, order.BalanceRemaining);
                    order.BalanceRemaining = localBalance;
                }
                if(order.BalanceRemaining <= 0)
                {
                    Log.Information("Order {0} has a balance of 0. Skipping", order.Id);
                    continue;
                }

                var paymentAmount = order.InstallmentAmount;
                if(paymentAmount > Math.Round(order.BalanceRemaining, 2))
                {
                    paymentAmount = Math.Round(order.BalanceRemaining, 2);
                }
                var newBalance = Math.Round(order.BalanceRemaining - paymentAmount, 2);
                
                //Check if we have a small amount remaining, if so just pay it all.
                if(newBalance > 0 && newBalance <= 1.0m)
                {
                    paymentAmount = Math.Round(order.BalanceRemaining, 2);
                }

                var payment = new Payment
                {
                    Order = order,
                    Amount = paymentAmount,
                    Status = Payment.Statuses.Created,
                    CreatedOn = DateTime.UtcNow
                };

                order.Payments.Add(payment);
                order.BalanceRemaining -= paymentAmount;

                SetNextPaymentDate(order);

                _dbContext.Orders.Update(order);
                await _dbContext.SaveChangesAsync();

            }


            // Create payments
            return true;
        }

        private void SetNextPaymentDate(Order order)
        {
            var now = DateTime.UtcNow;
            switch (order.InstallmentType)
            {
                case InstallmentTypes.Monthly:
                    order.NextPaymentDate = new DateTime(now.Year, now.Month, 1).AddMonths(1).Date.FromPacificTime();
                    break;
                case InstallmentTypes.Yearly:
                    order.NextPaymentDate = new DateTime(now.Year, 1, 1).AddYears(1).Date.FromPacificTime();
                    break;
                case InstallmentTypes.OneTime:
                    order.NextPaymentDate = now.AddDays(1).ToPacificTime().Date.FromPacificTime();
                    break;
            }
        }

        public async Task<bool> NotifyAboutFailedPayments()
        {
            // Notify about failed payments
            return true;
        }
    }
}
