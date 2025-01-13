using Hippo.Core.Domain;
using Hippo.Core.Models.SlothModels;
using System.Linq.Expressions;
using System.Text.Json;

namespace Hippo.Core.Models.ReportModels
{
    //This is a model for the invoice report
    public class InvoiceModel
    {
        public int Id { get; set; } //Payment Id
        public string TrackingNumber { get; set; }
        public DateTime CreatedOn { get; set; }
        public decimal Amount { get; set; }

        public string Details { get; set; } //chart strings, credit/debit, and amounts

        //Possibly could just do this in the Projection...
        public string BillingInfo
        {
            get
            {
                if (Details == null)
                {
                    return "No billing info";
                }
                try
                {
                    return string.Join(", ", JsonSerializer.Deserialize<TransferResponseModel[]>(Details).Where(a => a.Direction == "Debit").Select(t => $"Chart: {t.FinancialSegmentString} Amount: {t.Amount}"));
                }
                catch (Exception)
                {
                    return "Err";
                }
            }
        }

        public string CreatedBy { get; set; } //User who created the payment or Null if System created

        public int OrderId { get; set; }

        public string ProductName { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Units { get; set; }
        public decimal UnitPrice { get; set; }
        public int Installments { get; set; }
        public string InstallmentType { get; set; }
        public bool IsRecurring { get; set; }

        public string OrderName { get; set; }
        // Other order info

        public string MetaDataString { get; set; } //Metadata from the order.

        public string Sponsor { get; set; } //Principal Investigator
        public DateTime? CompletedOn { get; set; } //Date the payment was completed in Sloth

        public string ExternalReference { get; set; }
        public decimal Quantity { get; set; }
        public decimal Total { get; set; }
        public decimal BalanceRemaining { get; set; }
        public string Notes { get; set; }
        public string AdminNotes { get; set; }
        public string OrderStatus { get; set; }
        public DateTime? InstallmentDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public DateTime? NextPaymentDate { get; set; }

        public DateTime OrderCreatedOn { get; set; }

        /// <summary>
        /// Need to include the user, order, and the metadata for the order
        /// </summary>
        /// <returns></returns>
        public static Expression<Func<Payment, InvoiceModel>> Projection()
        {

            return payment => new InvoiceModel
            {
                Id = payment.Id,
                TrackingNumber = payment.TrackingNumber,
                CreatedOn = payment.CreatedOn,
                CompletedOn = payment.CompletedOn,
                Amount = payment.Amount,
                Details = payment.Details,
                CreatedBy = payment.CreatedBy != null ? $"{payment.CreatedBy.Name} ({payment.CreatedBy.Email})" : "System",
                OrderId = payment.OrderId,
                ProductName = payment.Order.ProductName,
                Description = payment.Order.Description,
                OrderName = payment.Order.Name,
                MetaDataString = string.Join(", ", payment.Order.MetaData.Select(m => $"{m.Name}: {m.Value}")),
                Sponsor = $"{payment.Order.PrincipalInvestigator.Name} ({payment.Order.PrincipalInvestigator.Email})",
                Category = payment.Order.Category,
                Units = payment.Order.Units,
                UnitPrice = payment.Order.UnitPrice,
                Installments = payment.Order.Installments,
                InstallmentType = payment.Order.InstallmentType,
                IsRecurring = payment.Order.IsRecurring,
                ExternalReference = payment.Order.ExternalReference,
                Quantity = payment.Order.Quantity,
                Total = payment.Order.Total,
                BalanceRemaining = payment.Order.BalanceRemaining,
                Notes = payment.Order.Notes,
                AdminNotes = payment.Order.AdminNotes,
                OrderStatus = payment.Order.Status,
                InstallmentDate = payment.Order.InstallmentDate,
                ExpirationDate = payment.Order.ExpirationDate,
                NextPaymentDate = payment.Order.NextPaymentDate,
                OrderCreatedOn = payment.Order.CreatedOn,

            };

        }
    }
}
