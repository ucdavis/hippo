using Hippo.Core.Domain;
using System.Linq.Expressions;

namespace Hippo.Web.Models.OrderModels
{
    public class OrderHistoryModel
    {
        public int Id { get; set; }
        public User? ActedBy { get; set; }
        public string Action { get; set; } = String.Empty;
        public string Status { get; set; } = string.Empty;
        public string Details { get; set; } = String.Empty;
        public DateTime ActedDate { get; set; }

        public static Expression<Func<History, OrderHistoryModel>> Projection()
        {
            return history => new OrderHistoryModel
            {
                Id = history.Id,
                ActedBy = history.ActedBy,
                Action = history.Action,
                Status = history.Status,
                Details = history.Details,
                ActedDate = history.ActedDate
            };
        }
    }
}
