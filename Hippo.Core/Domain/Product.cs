using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Hippo.Core.Domain
{
    public class Product : ProductBase
    {
        //Other fields in Product Base   

        public bool IsActive { get; set; }

        [Required]
        public int ClusterId { get; set; }
        public Cluster Cluster { get; set; }

        public class InstallmentTypes
        {
            public const string Monthly = "Monthly";
            public const string Yearly = "Yearly";
            public const string OneTime = "OneTime";
            public static List<string> Types = new List<string> { Monthly, Yearly, OneTime };
        }

        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>().Property(a => a.IsActive).HasDefaultValue(true);
            modelBuilder.Entity<Product>().HasQueryFilter(a => a.IsActive);
        }
    }
}
