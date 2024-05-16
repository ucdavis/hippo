using Hippo.Core.Domain;
using System.Linq.Expressions;

namespace Hippo.Web.Models.OrderModels
{
    public class OrderDetailModel
    {
        public int Id { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Units { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Installments { get; set; }
        public decimal Quantity { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ExternalReference { get; set; } = string.Empty;

        public List<OrderMetaData> MetaData { get; set; } = new();
        public List<History> History { get; set; } = new();
        public List<Billing> Billings { get; set; } = new();
        public List<Payment> Payments { get; set; } = new();
        //TODO: will need to add more

        public static Expression<Func<Order, OrderDetailModel>> Projection()
        {
            return order => new OrderDetailModel
            {
                Id = order.Id,
                Category = order.Category,
                Name = order.Name,
                Description = order.Description,
                Units = order.Units,
                UnitPrice = order.UnitPrice,
                Installments = order.Installments,
                Quantity = order.Quantity,
                Status = order.Status,
                ExternalReference = order.ExternalReference,
                MetaData = order.MetaData,
                History = (List<History>)order.History.Where(a => a.Type == Hippo.Core.Domain.History.HistoryTypes.Primary),
                Billings = order.Billings,
                Payments = order.Payments
            };
        }
    }
}
