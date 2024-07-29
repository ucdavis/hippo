using Hippo.Core.Domain;
using System.Linq.Expressions;

namespace Hippo.Web.Models.OrderModels
{
    public class OrderPaymentModel
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }

        //EntryAmount is just for the posting manual payments

        public string Status { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
        public User? CreatedBy { get; set; }

        public static Expression<Func<Payment, OrderPaymentModel>> Projection()
        {
            return payment => new OrderPaymentModel
            {
                Id = payment.Id,
                Amount = payment.Amount,
                Status = payment.Status,
                CreatedOn = payment.CreatedOn,
                CreatedBy = payment.CreatedBy
            };
        }
    }
}
