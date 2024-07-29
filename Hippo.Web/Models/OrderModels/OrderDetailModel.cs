using Hippo.Core.Domain;
using Hippo.Core.Extensions;
using Hippo.Core.Services;
using System.Linq;
using System.Linq.Expressions;

namespace Hippo.Web.Models.OrderModels
{
    public class OrderDetailModel : ProductBase
    {
        public string ProductName { get; set; } = string.Empty;

        public decimal Quantity { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ExternalReference { get; set; } = string.Empty;

        public string Notes { get; set; } = string.Empty;
        public string AdminNotes { get; set; } = string.Empty;

        public decimal Adjustment { get; set; }
        public string AdjustmentReason { get; set; } = string.Empty;

        public string SubTotal { get; set; } = string.Empty;
        public string Total { get; set; } = string.Empty;
        public string BalanceRemaining { get; set; } = string.Empty;
        public string BalancePending { get; set; } = string.Empty;

        public string InstallmentDate { get; set; } = string.Empty;
        public string ExpirationDate { get; set; } = string.Empty; //This would default to InstallmentDate + LifeCycle Months                                                                   
        public DateTime? NextPaymentDate { get; set; } //No idea why I needs to add 1 day below... Couldn't get the conversion correct. Seems to skip time portion
        public string NextPaymentAmount { get; set; } = string.Empty;

        public int HistoryCount { get; set; }
        public int PaymentCount { get; set; }

        public User? PiUser { get; set; }

        public List<OrderMetaData> MetaData { get; set; } = new();
        public List<Billing> Billings { get; set; } = new();


        public static Expression<Func<Order, OrderDetailModel>> Projection()
        {
            return order => new OrderDetailModel
            {
                Id = order.Id,
                PiUser = order.PrincipalInvestigator.Owner != null ? order.PrincipalInvestigator.Owner : new User { Email = order.PrincipalInvestigator.Email},
                Category = order.Category,
                Name = order.Name,
                ProductName = order.ProductName,
                Description = order.Description,
                Units = order.Units,
                UnitPrice = order.UnitPrice,
                Installments = order.Installments,
                InstallmentType = order.InstallmentType,
                LifeCycle = order.LifeCycle,
                InstallmentDate = order.InstallmentDate != null ? order.InstallmentDate.Value.ToPacificTime().ToString("yyyy-MM-dd") : string.Empty,
                ExpirationDate = order.ExpirationDate != null ? order.ExpirationDate.Value.ToPacificTime().ToString("yyyy-MM-dd") : string.Empty,
                NextPaymentDate = order.NextPaymentDate,
                NextPaymentAmount = order.BalanceRemaining < order.InstallmentAmount ? order.BalanceRemaining.ToString() : order.InstallmentAmount.ToString(),
                Quantity = order.Quantity,
                Status = order.Status,
                ExternalReference = order.ExternalReference,
                Notes = order.Notes,
                Adjustment = order.Adjustment,
                AdjustmentReason = order.AdjustmentReason,
                AdminNotes = order.AdminNotes,
                SubTotal = order.SubTotal.ToString("F2"),
                Total = order.Total.ToString("F2"),
                BalanceRemaining = order.BalanceRemaining.ToString("F2"), //if I do this with a currency, it will add a $ sign and that makes it a little harder to work with UI side
                BalancePending = order.Payments.Where(a => a.Status != Payment.Statuses.Completed && a.Status != Payment.Statuses.Cancelled).Sum(a => a.Amount).ToString("F2"),
                MetaData = order.MetaData,
                Billings = order.Billings,
                HistoryCount = order.History.Count,
                PaymentCount = order.Payments.Count
            };
        }

        public static Expression<Func<Product, OrderDetailModel>> ProductProjection()
        {
            return product => new OrderDetailModel
            {
                Id = 0,
                Category = product.Category,
                Name = product.Name,
                ProductName = product.Name,
                Description = product.Description,
                Units = product.Units,
                UnitPrice = product.UnitPrice,
                Installments = product.Installments,
                InstallmentType = product.InstallmentType,
                Quantity = 0,
                Status = Order.Statuses.Created,
                ExternalReference = string.Empty,
                Notes = string.Empty,
                Adjustment = 0,
                AdjustmentReason = string.Empty,
                AdminNotes = string.Empty,
                SubTotal = "0.00",
                Total = "0.00",
                BalanceRemaining = "0.00", //if I do this with a currency, it will add a $ sign and that makes it a little harder to work with UI side
                BalancePending = "0.00",
                MetaData = new List<OrderMetaData>(),
                Billings = new List<Billing>(),
                HistoryCount = 0,
                PaymentCount = 0
            };
        }
    }
}
