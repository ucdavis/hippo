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
using static Hippo.Core.Models.SlothModels.TransferViewModel;

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
        private readonly IAggieEnterpriseService _aggieEnterpriseService;

        public PaymentsService(AppDbContext dbContext, IHistoryService historyService, IAggieEnterpriseService aggieEnterpriseService)
        {
            _dbContext = dbContext;
            _historyService = historyService;
            _aggieEnterpriseService = aggieEnterpriseService;
        }
        public async Task<bool> CreatePayments()
        {
            //Do a check on all active orders that don't have a next payment date and a balance > 0 
            var orderCheck = await _dbContext.Orders.Include(a => a.Payments).Include(a => a.Cluster).Include(a => a.PrincipalInvestigator).Where(a => a.Status == Order.Statuses.Active && a.NextPaymentDate == null && a.BalanceRemaining > 0).ToListAsync();
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
            foreach (var order in orders)
            {

                if (order.Total <= order.Payments.Where(a => a.Status == Payment.Statuses.Completed).Sum(a => a.Amount))
                {
                    order.Status = Order.Statuses.Completed;
                    order.NextPaymentDate = null;
                    //TODO: A notification? This Shold happen when sloth updates, but just in case.
                    _dbContext.Orders.Update(order);
                    await _dbContext.SaveChangesAsync();
                    continue;
                }

                //Need to ignore manual ones...
                if (order.Payments.Any(a => a.CreatedById == null && (a.Status == Payment.Statuses.Created || a.Status == Payment.Statuses.Processing)))
                {
                    Log.Information("Skipping order {0} because it has a created or processing payment", order.Id);
                    continue;
                }

                //TODO: Recalculate balance remaining in case DB value is wrong?
                var localBalance = Math.Round(order.Total - order.Payments.Where(a => a.Status == Payment.Statuses.Completed || a.Status == Payment.Statuses.Processing).Sum(a => a.Amount), 2);
                if (localBalance != order.BalanceRemaining)
                {
                    Log.Information("Order {0} has a balance mismatch. Local: {1} DB: {2}", order.Id, localBalance, order.BalanceRemaining);
                    order.BalanceRemaining = localBalance;
                }
                if (order.BalanceRemaining <= 0)
                {
                    Log.Information("Order {0} has a balance of 0. Skipping", order.Id);
                    continue;
                }

                var paymentAmount = order.InstallmentAmount;
                if (paymentAmount > Math.Round(order.BalanceRemaining, 2))
                {
                    paymentAmount = Math.Round(order.BalanceRemaining, 2);
                }
                var newBalance = Math.Round(order.BalanceRemaining - paymentAmount, 2);

                //Check if we have a small amount remaining, if so just pay it all.
                if (newBalance > 0 && newBalance <= 1.0m)
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
            var yesterday = DateTime.UtcNow.AddDays(-1); //If this was run right after the sloth service, any in created probably failed, but will give it some slack to make sure
            //This should be a list of all payments that have a bad chart string, but it is possible there is some other issue, so we will want to validate the chart string before notifying sponsors.
            var orderIdsWithFailedPayments = await _dbContext.Payments.Where(a => a.Status == Payment.Statuses.Created && a.CreatedOn <= yesterday).Select(a => a.OrderId).Distinct().ToListAsync();

            var allOrders = await _dbContext.Orders.Include(a => a.Billings).Include(a => a.PrincipalInvestigator).Where(a => orderIdsWithFailedPayments.Contains(a.Id)).ToListAsync();
            var orderGrouping = allOrders.GroupBy(a => a.ClusterId);


            foreach (var orderGroup in orderGrouping)
            {
                var cluster = await _dbContext.Clusters.SingleAsync(a => a.Id == orderGroup.Key);
                var invalidOrderIdsInCluster = new List<int>();
                foreach (var order in orderGroup)
                {
                    var invalidChartStrings = false;
                    //Validate chart string
                    foreach (var billing in order.Billings)
                    {
                        var validation = await _aggieEnterpriseService.IsChartStringValid(billing.ChartString, Directions.Debit);
                        if (!validation.IsValid)
                        {
                            invalidChartStrings = true;
                            break;
                        }
                    }
                    if (invalidChartStrings)
                    {
                        //TODO: Notify the sponsor
                        //Remember to add notification/email service to job


                        invalidOrderIdsInCluster.Add(order.Id);
                    }
                }
                if (invalidOrderIdsInCluster.Count > 0)
                {
                    //Get lists of cluster admins
                    //Send email to cluster admins with list of orders that have failed payments
                    //https://localhost:44371/caesfarm/order/details/46

                    var clusterAdmins = await _dbContext.Users.AsNoTracking().Where(u => u.Permissions.Any(p => p.Cluster.Id == orderGroup.Key && p.Role.Name == Role.Codes.ClusterAdmin)).OrderBy(u => u.LastName).ThenBy(u => u.FirstName).ToArrayAsync();
                    //TODO: Notify the cluster admins with a single email

                }

            }

            //var orders = await _dbContext.Orders.Include(a => a.Payments).Include(a => a.Cluster).Include(a => a.PrincipalInvestigator).Where(a => a.Status == Order.Statuses.Active && a.NextPaymentDate != null && a.NextPaymentDate.Value.Date < now.Date).ToListAsync();
            return true;
        }
    }
}
