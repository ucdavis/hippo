using Hippo.Core.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Hippo.Core.Domain.Product;

namespace Hippo.Core.Models.OrderModels
{
    public class OrderPostModel : ProductBase
    {

        [Required]
        [MaxLength(50)]
        public string ProductName { get; set; }

        [MaxLength(150)]
        public string ExternalReference { get; set; }

        [Required]
        [Range(0.0001, double.MaxValue)]
        public decimal Quantity { get; set; }

        public string InstallmentDate { get; set; }
        public string ExpirationDate { get; set; } //This would default to InstallmentDate + LifeCycle Months    

        public string PILookup { get; set; }
        public List<OrderMetaData> MetaData { get; set; } = new();

        public List<Billing> Billings { get; set; } = new();

        public decimal Adjustment { get; set; }
        public string AdjustmentReason { get; set; }
        public string Notes { get; set; }
        public string AdminNotes { get; set; }

    }
}
