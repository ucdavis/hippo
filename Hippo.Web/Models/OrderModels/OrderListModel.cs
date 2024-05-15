using Hippo.Core.Extensions;
using System.Linq.Expressions;

namespace Hippo.Web.Models.OrderModels
{
    public class OrderListModel
    {

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Units { get; set; }
        public decimal Quantity { get; set; }
        public DateTime CreatedOn { get; set; }
        public string Status { get; set; }
        public decimal Total { get; set; }
        public decimal InstallmentAmount { get; set; }
        public decimal BalanceRemaining { get; set; }

        public static Expression<Func<Core.Domain.Order, OrderListModel>> Projection()
        {
            return order => new OrderListModel
            {
                Id = order.Id,
                Name = order.Name,
                Description = order.Description,
                Units = order.Units,
                Quantity = order.Quantity,
                CreatedOn = order.CreatedOn.ToPacificTime(),
                Status = order.Status,
                Total = order.Total,
                InstallmentAmount = order.InstallmentAmount,
                BalanceRemaining = order.BalanceRemaining
            };
        }
    }
}
