using Hippo.Core.Domain;
using System.Linq.Expressions;

namespace Hippo.Web.Models.OrderModels
{
    public class OrderBillingModel
    {
        public int Id { get; set; }
        public string ChartString { get; set; } = string.Empty;
        public string Percentage { get; set; } = string.Empty;
        public BillingChartStringValidationModel? ChartStringValidation { get; set; }

    }

    public class BillingChartStringValidationModel
    {
        public bool IsValid { get; set; }
        public string Description { get; set; } = string.Empty;
        public string AccountManager { get; set; } = string.Empty;
        public string AccountManagerEmail { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Warning { get; set; } = string.Empty;
    }
}
