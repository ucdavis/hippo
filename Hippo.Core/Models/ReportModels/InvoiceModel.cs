using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Hippo.Core.Domain;
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

        public string BillingInfo { get; set; } //Billing info from the payment details. Need to deserialize it and then join that into a string
        public string CreatedBy { get; set; } //User who created the payment or Null if System created

        public int OrderId { get; set; }
        
        public string ProductName { get; set; }
        public string Description { get; set; }
        // Other product info

        public string OrderName { get; set; }
        public string OrderDescription { get; set; }
        // Other order info

        public string MetaDataString { get; set; } //Metadata from the order.




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
                BillingInfo = payment.Details,
                CreatedBy = payment.CreatedBy != null ? payment.CreatedBy.Email : "System",
                OrderId = payment.OrderId,
                ProductName = payment.Order.ProductName,
                Description = payment.Order.Description,
                OrderName = payment.Order.Name,
                OrderDescription = payment.Order.Description,
                MetaDataString = string.Join(", ", payment.Order.MetaData.Select(m => $"{m.Name}: {m.Value}"))
            };

        }
    }
}
