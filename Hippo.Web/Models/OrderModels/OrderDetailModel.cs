﻿using Hippo.Core.Domain;
using System.Linq.Expressions;

namespace Hippo.Web.Models.OrderModels
{
    public class OrderDetailModel
    {
        public int Id { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Units { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Installments { get; set; }
        public string InstallmentType { get; set; } = string.Empty;
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

        public User? PiUser { get; set; }

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
                PiUser = order.PrincipalInvestigator.Owner != null ? order.PrincipalInvestigator.Owner : new User { Email = order.PrincipalInvestigator.Email},
                Category = order.Category,
                Name = order.Name,
                ProductName = order.ProductName,
                Description = order.Description,
                Units = order.Units,
                UnitPrice = order.UnitPrice,
                Installments = order.Installments,
                InstallmentType = order.InstallmentType,
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
                MetaData = order.MetaData,
                History = (List<History>)order.History.Where(a => a.Type == Hippo.Core.Domain.History.HistoryTypes.Primary),
                Billings = order.Billings,
                Payments = order.Payments
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
                MetaData = new List<OrderMetaData>(),
                History = new List<History>(),
                Billings = new List<Billing>(),
                Payments = new List<Payment>()
            };
        }
    }
}
