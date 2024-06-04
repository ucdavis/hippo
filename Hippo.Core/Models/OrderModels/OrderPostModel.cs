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
    public class OrderPostModel
    {
        public int Id { get; set; }

        [Required]
        public string Category { get; set; }
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        [Required]
        [MaxLength(50)]
        public string ProductName { get; set; }
        [MaxLength(250)]
        public string Description { get; set; }
        [MaxLength(150)]
        public string ExternalReference { get; set; }
        public string Units { get; set; } //Informational like TB, or fairshair points
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal UnitPrice { get; set; }
        [Required]
        [Range(0.0001, double.MaxValue)]
        public decimal Quantity { get; set; }
        [Required]
        [Range(1, int.MaxValue)]
        public int Installments { get; set; }

        [Required]
        [MaxLength(10)]
        public string InstallmentType { get; set; } = InstallmentTypes.Monthly; //Monthly, Yearly

        public string PILookup { get; set; }
        public List<OrderMetaData> MetaData { get; set; } = new();



        public decimal Adjustment { get; set; }
        public string AdjustmentReason { get; set; }
        public string Notes { get; set; }
        public string AdminNotes { get; set; }

    }
}
