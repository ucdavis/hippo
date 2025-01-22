using Hippo.Core.Data;
using Hippo.Core.Domain;
using Hippo.Core.Extensions;
using Hippo.Core.Models.Email;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Hippo.Core.Services
{
    public interface IExpiringOrdersService
    {
        public Task<string> ProcessExpiringOrderNotifications();
    }

    public class ExpiringOrdersService : IExpiringOrdersService
    {
        private readonly AppDbContext _dbContext;
        private readonly INotificationService _notificationService;

        public ExpiringOrdersService(AppDbContext dbContext, INotificationService notificationService)
        {
            _dbContext = dbContext;
            _notificationService = notificationService;
        }

        public async Task<string> ProcessExpiringOrderNotifications()
        {
            var countSuccessful = 0;
            var countFailed = 0;
            //Need to get orders that are expiring in 30 days, or have expired.
            var orderStatus = new List<string> { Order.Statuses.Active, Order.Statuses.Completed };
            var compareDate = DateTime.UtcNow.AddDays(31);
            var utcNow = DateTime.UtcNow;

            //Filter out by next notification date too? grab when null or less that now?

            var orders = await _dbContext.Orders
                .Include(a => a.PrincipalInvestigator).Include(a => a.Cluster)
                .Where(a => orderStatus.Contains(a.Status) && a.ExpirationDate != null && a.ExpirationDate <= compareDate).ToListAsync();

            foreach (var order in orders)
            {

                //The next notification date should get set a month before it expires. 
                if (utcNow >= order.ExpirationDate && order.NextNotificationDate.HasValue)
                {
                    try
                    {
                        //Final notification for expired order
                        await _notificationService.OrderExpiredNotification(order, new[] { order.Cluster.Email });
                        Log.Information("Order {OrderId} has expired. Final notification sent.", order.Id);
                        countSuccessful++;
                        order.NextNotificationDate = null;
                        _dbContext.Orders.Update(order);
                        //Save here or at the end?
                        await _dbContext.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error sending final email regarding order expiration.");
                        countFailed++;
                    }
                    continue;
                }

                if (utcNow >= order.ExpirationDate)
                {
                    //We only want to send the final notification once.
                    continue;
                }

                if (order.NextNotificationDate != null && order.NextNotificationDate > utcNow)
                {
                    Log.Information("Skipping order {OrderId} as it has already been notified.", order.Id);
                    continue;
                }

                try
                {
                    var clusterAdmins = await _dbContext.Users.AsNoTracking().Where(u => u.Permissions.Any(p => p.Cluster.Id == order.ClusterId && p.Role.Name == Role.Codes.ClusterAdmin)).Select(a => a.Email).ToArrayAsync();
                    var emailModel = new SimpleNotificationModel
                    {
                        Subject = "Hippo Order Expiring",
                        Header = $"Hippo order expires on {order.ExpirationDate.Value.ToPacificTime().Date.ToShortDateString()}.",
                        Paragraphs = new List<string>
                        {
                            $"Order {order.Id} {order.Name} of type {order.Category} will reach the end of its life span on {order.ExpirationDate.Value.ToPacificTime().Date.ToShortDateString()}.",
                            $"You may want to contact the cluster admin(s) to purchase new equipment to avoid any downtime. Or use the Order Replacement button below.",
                            $"Order ID: {order.Id}",
                            $"Product: {order.ProductName}",
                            $"Category: {order.Category}",
                            $"Cluster: {order.Cluster.Name}",
                        }
                    };

                    await _notificationService.OrderNotificationTwoButton(emailModel, order, clusterAdmins);
                    Log.Information("Order {OrderId} notification sent.", order.Id);
                    order.NextNotificationDate = utcNow.AddDays(7);
                    _dbContext.Orders.Update(order);
                    await _dbContext.SaveChangesAsync(); //Save here or at the end?
                    countSuccessful++;

                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error sending email to regarding order expiration.");
                }
            }

            return $"Successfully emailed {countSuccessful} orders. Failed to email {countFailed} orders.";
        }
    }
}
