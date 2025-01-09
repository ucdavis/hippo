using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Hippo.Core.Domain;
using Hippo.Core.Models.SlothModels;
using Microsoft.EntityFrameworkCore;

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
        // Other product info

        public string OrderName { get; set; }
        // Other order info

        public string MetaDataString { get; set; } //Metadata from the order.

        public string Sponsor { get; set; } //Principal Investigator




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
                Amount = payment.Amount,
                Details = payment.Details,
                CreatedBy = payment.CreatedBy != null ? $"{payment.CreatedBy.Name} ({payment.CreatedBy.Email})" : "System",
                OrderId = payment.OrderId,
                ProductName = payment.Order.ProductName,
                Description = payment.Order.Description,
                OrderName = payment.Order.Name,
                MetaDataString = string.Join(", ", payment.Order.MetaData.Select(m => $"{m.Name}: {m.Value}")),
                Sponsor = $"{payment.Order.PrincipalInvestigator.Name} ({payment.Order.PrincipalInvestigator.Email})"
            };

        }
    }
}
