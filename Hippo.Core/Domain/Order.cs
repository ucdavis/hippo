using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Hippo.Core.Domain
{
    public class Order
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
        public string ExternalReference { get; set; }
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        public int Installments { get; set; }


        public decimal Adjustments { get; set; }
        public string AdjustmentReason { get; set; }
        public decimal Total { get; set; }
        public string Notes { get; set; }
        public string AdminNotes { get; set; }
        public string Status { get; set; }


        [Required]
        public int ClusterId { get; set; }
        public Cluster Cluster { get; set; }

        [Required]
        public int CreatedById { get; set; }
        public User CreatedBy { get; set; }

        [Required]
        public int PrincipalInvestigatorId { get; set; }
        public User PrincipalInvestigator { get; set; }

        [JsonIgnore]
        public List<History> History { get; set; } = new();

        public class Statuses
        {
            public const string Created = "Created";
            public const string Submitted = "Submitted";
            public const string Processing = "Processing";
            public const string Cancelled = "Cancelled";
            public const string Active = "Active";
            public const string Rejected = "Rejected"; //Not sure if we need this
            public const string Completed = "Completed";

            public static List<string> StatusTypes = new List<string>
            {
                Created,
                Submitted,
                Processing,
                Cancelled,
                Active,
                Rejected,
                Completed
            };
        }
        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>().HasQueryFilter(o => o.Cluster.IsActive);
            modelBuilder.Entity<Order>().HasIndex(o => o.CreatedById);
            modelBuilder.Entity<Order>().HasIndex(o => o.PrincipalInvestigatorId);
            modelBuilder.Entity<Order>().HasOne(o => o.CreatedBy).WithMany().HasForeignKey(o => o.CreatedById).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<History>().HasOne(o => o.Order).WithMany().HasForeignKey(o => o.OrderId).OnDelete(DeleteBehavior.Restrict);
            //The Principal Investigator reference is declared in the User class
        }
    }
}
