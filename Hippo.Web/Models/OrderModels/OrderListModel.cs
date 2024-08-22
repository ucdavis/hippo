using Hippo.Core.Domain;
using Hippo.Core.Extensions;
using System.Linq.Expressions;

namespace Hippo.Web.Models.OrderModels
{
    public class OrderListModel
    {

        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Units { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public DateTime CreatedOn { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public decimal BalanceRemaining { get; set; }

        public decimal PendingAmount { get; set; }

        public string SponsorName { get; set; } = string.Empty;

        public static Expression<Func<Core.Domain.Order, OrderListModel>> Projection()
        {
            return order => new OrderListModel
            {
                Id = order.Id,
                Name = order.Name,
                Description = order.Description,
                Units = order.Units,
                Quantity = order.Quantity,
                CreatedOn = order.CreatedOn,
                Status = order.Status,
                Total = order.IsRecurring ? (order.Payments.Where(a => a.Status == Payment.Statuses.Completed).Sum(a => a.Amount) + order.BalanceRemaining) : order.Total,
                BalanceRemaining = order.BalanceRemaining,
                PendingAmount = order.Payments.Where(a => a.Status == Payment.Statuses.Created || a.Status == Payment.Statuses.Processing).Sum(a => a.Amount),
                SponsorName = order.PrincipalInvestigator.Name
            };
        }
    }
}
