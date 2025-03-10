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

        public DateTime CreatedOn { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public decimal BalanceRemaining { get; set; }

        public decimal PendingAmount { get; set; }

        public bool IsRecurring { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public DateTime? InstallmentDate { get; set; }

        public string SponsorName { get; set; } = string.Empty;

        public string Messages { get; set; } = string.Empty; //Specialty message for problematic orders

        public static Expression<Func<Core.Domain.Order, OrderListModel>> Projection()
        {
            return order => new OrderListModel
            {
                Id = order.Id,
                Name = order.Name,
                Description = order.Description,
                CreatedOn = order.CreatedOn,
                Status = order.Status,
                IsRecurring = order.IsRecurring,
                ExpirationDate = order.ExpirationDate,
                InstallmentDate = order.InstallmentDate,
                Total = order.IsRecurring ? (order.Payments.Where(a => a.Status == Payment.Statuses.Completed).Sum(a => a.Amount) + order.BalanceRemaining) : order.Total,
                BalanceRemaining = order.BalanceRemaining,
                PendingAmount = order.Payments.Where(a => a.Status == Payment.Statuses.Created || a.Status == Payment.Statuses.Processing).Sum(a => a.Amount),
                SponsorName = order.PrincipalInvestigator.Name
            };
        }
    }
}
