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
        private readonly INotificationService _notificationService;

        public PaymentsService(AppDbContext dbContext, IHistoryService historyService, IAggieEnterpriseService aggieEnterpriseService, INotificationService notificationService)
        {
            _dbContext = dbContext;
            _historyService = historyService;
            _aggieEnterpriseService = aggieEnterpriseService;
            _notificationService = notificationService;
        }
        public async Task<bool> CreatePayments()
        {
            //Do a sanity check and make sure there are no non recurring orders with a negative balance
            var negativeBalanceOrders = await _dbContext.Orders.Include(a => a.Payments).Include(a => a.Cluster).Where(a => !a.IsRecurring && a.Status == Order.Statuses.Active && a.BalanceRemaining < 0).ToListAsync();
            foreach (var order in negativeBalanceOrders)
            {
                Log.Error("Order {0} has a negative balance. Balance: {1}", order.Id, order.BalanceRemaining);
                order.BalanceRemaining = order.Total - order.Payments.Where(a => a.Status == Payment.Statuses.Completed).Sum(a => a.Amount);
                if(order.BalanceRemaining < 0)
                {
                    Log.Error("Order {0} still has a negative balance. Setting to 0", order.Id);
                    order.BalanceRemaining = 0;
                    //Posibly set to completed? Or notify someone that there appears to be an over payment?
                    await _historyService.OrderUpdated(order, null, "Negative balance detected. Setting Balance to 0");
                }
                _dbContext.Orders.Update(order);
                await _dbContext.SaveChangesAsync();
            }


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

            //Next payment date should be a UTC date/Time, at 7AM UTC, which should be 12AM PST

            //If I add a history call here, I'll also need to get the cluster .Include(a => a.Cluster)
            var orders = await _dbContext.Orders.Include(a => a.Payments).Where(a => a.Status == Order.Statuses.Active && a.NextPaymentDate != null && a.NextPaymentDate.Value <= DateTime.UtcNow).ToListAsync();
            foreach (var order in orders) {

                if (!order.IsRecurring)
                {
                    if (order.Total <= order.Payments.Where(a => a.Status == Payment.Statuses.Completed).Sum(a => a.Amount))
                    {
                        order.Status = Order.Statuses.Completed;
                        order.NextPaymentDate = null;
                        order.BalanceRemaining = 0;
                        //TODO: A notification? This Shold happen when sloth updates, but just in case.
                        _dbContext.Orders.Update(order);
                        await _dbContext.SaveChangesAsync();
                        continue;
                    }
                }
                else
                {
                    //This is a recurring order where the next payment date has passed and the BalanceRemaining is 0. The balance should only be updated when the payment is completed. so we now want to set it for the next billing period.
                    if (order.BalanceRemaining <= 0)
                    {
                        SetNextPaymentDate(order);
                        //if for some reason the balance is a negative, it is probably because of an over payment, so it makes sense to just add the next payment ammount
                        order.BalanceRemaining += order.Total;
                        _dbContext.Orders.Update(order);
                        await _dbContext.SaveChangesAsync();
                        continue;
                    }
                }

                //Need to ignore manual ones...
                if (order.Payments.Any(a => a.CreatedById == null && (a.Status == Payment.Statuses.Created || a.Status == Payment.Statuses.Processing)))
                {
                    //This could happen if auto approve is not on, and they take a long time to approve in sloth. Not really a big deal for non recurring as the order will eventually get paid.
                    Log.Information("Skipping order {0} because it has a created or processing payment", order.Id);
                    continue;
                }

                if (!order.IsRecurring)
                {
                    //TODO: Recalculate balance remaining in case DB value is wrong?
                    var localBalance = Math.Round(order.Total - order.Payments.Where(a => a.Status == Payment.Statuses.Completed).Sum(a => a.Amount), 2);
                    if (localBalance != order.BalanceRemaining)
                    {
                        Log.Information("Order {0} has a balance mismatch. Local: {1} DB: {2}", order.Id, localBalance, order.BalanceRemaining);
                        order.BalanceRemaining = localBalance;
                    }
                }
                var pendingAmount = order.Payments.Where(a => a.Status == Payment.Statuses.Created || a.Status == Payment.Statuses.Processing).Sum(a => a.Amount);
                var balanceLessPending = order.BalanceRemaining - pendingAmount;
                if (balanceLessPending <= 0)
                {
                    Log.Information("Order {0} has a pending balance of 0. Skipping", order.Id);
                    continue;
                }

                var paymentAmount = order.InstallmentAmount;
                if (paymentAmount > Math.Round(balanceLessPending, 2))
                {
                    paymentAmount = Math.Round(balanceLessPending, 2);
                }
                var newBalance = Math.Round(balanceLessPending - paymentAmount, 2);

                //Check if we have a small amount remaining, if so just pay it all.
                if (newBalance > 0 && newBalance <= 1.0m)
                {
                    paymentAmount = Math.Round(balanceLessPending, 2);
                }

                var payment = new Payment
                {
                    Order = order,
                    Amount = paymentAmount,
                    Status = Payment.Statuses.Created,
                    CreatedOn = DateTime.UtcNow
                };

                order.Payments.Add(payment);
                //order.BalanceRemaining -= paymentAmount; //Should only get updated once payment is completed now

                SetNextPaymentDate(order); //This "should" set it to next month/year

                if (order.IsRecurring) //If the payment above gets rejected/canceled we should test this
                {
                    //The next payment date should be set now, so add the BalanceRemaining
                    order.BalanceRemaining += order.Total; //This should be ok, because the next payment date should be set to the next billing period
                }

                _dbContext.Orders.Update(order);
                await _dbContext.SaveChangesAsync();

            }


            // Create payments
            return true;
        }

        private void SetNextPaymentDate(Order order)
        {
            var nowPlusADay = DateTime.UtcNow.AddDays(1); //Bump it up a day, so we are in the next month/year
            var pacificNow = nowPlusADay.ToPacificTime();

            switch (order.InstallmentType)
            {
                case InstallmentTypes.Monthly:
                    //This should be 7AM UTC, which is 12AM PST and the job runs at 2-3 PST or 10AM UTC
                    order.NextPaymentDate = new DateTime(pacificNow.Year, pacificNow.Month, 1).AddMonths(1).AddDays(-1).Date.ToUniversalTime();
                    break;
                case InstallmentTypes.Yearly:
                    order.NextPaymentDate = new DateTime(pacificNow.Year, 1, 1).AddYears(1).Date.ToUniversalTime();
                    break;
                case InstallmentTypes.OneTime:
                    order.NextPaymentDate = pacificNow.Date.ToUniversalTime();
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
                        Log.Error("Order {0} has an invalid chart string", order.Id);
                        //TODO: Notify the sponsor
                        //Remember to add notification/email service to job
                        try
                        {
                           await _notificationService.SponsorPaymentFailureNotification(new string[] { order.PrincipalInvestigator.Email }, order);
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "Failed to notify sponsor for order {0}", order.Id);
                        }


                        invalidOrderIdsInCluster.Add(order.Id);
                    }
                }
                if (invalidOrderIdsInCluster.Count > 0)
                {
                    //Send email to cluster admins / financial admins with list of orders that have failed payments

                    var notificationList = await _dbContext.Users.AsNoTracking().Where(a => a.Permissions.Any(p => p.Cluster.Id == orderGroup.Key && (p.Role.Name == Role.Codes.ClusterAdmin || p.Role.Name == Role.Codes.FinancialAdmin))).Select(a => a.Email).Distinct().ToArrayAsync();

                    try
                    {
                        await _notificationService.AdminPaymentFailureNotification(notificationList, cluster.Name, invalidOrderIdsInCluster.ToArray());
                    }
                    catch (Exception ex)
                    {

                        Log.Error(ex, "Failed to notify cluster admins/financial for cluster {0}", cluster.Id);
                    }

                }

            }
            
            return true;
        }
    }
}
