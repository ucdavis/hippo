using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Hippo.Core.Domain
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        bool IsActive { get; set; }
        [Required]
        public string Category { get; set; }
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        [MaxLength(250)]
        public string Description { get; set; }
        public string Units { get; set; } //Informational like TB, or fairshair points
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal UnitPrice { get; set; }
        //Not sure if we want to do this, but it lets a default number of payment installments to be specified
        public int Installments { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        [Required]
        public int ClusterId { get; set; }
        public Cluster Cluster { get; set; }
        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>().Property(a => a.IsActive).HasDefaultValue(true);
            modelBuilder.Entity<Product>().HasQueryFilter(a => a.IsActive);
        }
    }
}
