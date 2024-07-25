using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Hippo.Core.Domain.Product;

namespace Hippo.Core.Domain
{
    public class ProductBase
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Category { get; set; }
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        [MaxLength(250)]
        public string Description { get; set; }
        [MaxLength(50)]
        public string Units { get; set; } //Informational like TB, or fair share points
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal UnitPrice { get; set; }
        //Not sure if we want to do this, but it lets a default number of payment installments to be specified
        public int Installments { get; set; }

        public int LifeCycle { get; set; } = 60; //Number of months or years the product is active for

        [Required]
        [MaxLength(10)]
        public string InstallmentType { get; set; } = InstallmentTypes.Monthly; //Monthly, Yearly, OneTime


    }
}
