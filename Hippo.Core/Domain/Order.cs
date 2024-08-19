using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static Hippo.Core.Domain.Product;

namespace Hippo.Core.Domain
{
    public class Order :ProductBase
    {

        [Required]
        [MaxLength(50)]
        public string ProductName { get; set; }

        [MaxLength(150)]
        public string ExternalReference { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Quantity { get; set; }

        public decimal Adjustment { get; set; }
        public string AdjustmentReason { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Total { get; set; }
        public decimal BalanceRemaining { get; set; } //We will also calculate this when we do a payment
        public string Notes { get; set; }
        public string AdminNotes { get; set; }
        [MaxLength(20)]
        public string Status { get; set; }
        
        public DateTime? InstallmentDate { get; set; }
        public DateTime? ExpirationDate { get; set; } //This would default to InstallmentDate + LifeCycle Months
                                                      
        public DateTime? NextPaymentDate { get; set; } //When we start payments, this will be set to trigger the auto creation of a payment. Onetime=tomorrow, Monthly=1st of next month, yearly= jan 1st of next year.

        public DateTime? NextNotificationDate { get; set; } //This will be used to send notification to the sponsor once the ExpirationDate is reached. This will be set to ExpirationDate - 30 days?

        public decimal InstallmentAmount => IsRecurring ? Math.Round(Total, 2) : Math.Round(Total / Installments, 2);

        [Required]
        public int ClusterId { get; set; }
        [JsonIgnore]
        public Cluster Cluster { get; set; }


        [Required]
        public int PrincipalInvestigatorId { get; set; }
        public Account PrincipalInvestigator { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public List<Billing> Billings { get; set; } = new();

        public List<OrderMetaData> MetaData { get; set; } = new();

        public void AddMetaData(string key, string value)
        {
            MetaData.Add(new OrderMetaData { Name = key, Value = value, Order = this });
        }
        [JsonIgnore]
        public List<Payment> Payments { get; set; } = new();

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
            public const string Archived = "Archived";

            public static List<string> StatusTypes = new List<string>
            {
                Created,
                Submitted,
                Processing,
                Cancelled,
                Active,
                Rejected,
                Completed,
                Archived,
            };
        }
        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>().HasQueryFilter(o => o.Cluster.IsActive);
            modelBuilder.Entity<Order>().HasIndex(o => o.PrincipalInvestigatorId);
            modelBuilder.Entity<Order>().HasIndex(o => o.ClusterId);
            modelBuilder.Entity<Order>().HasIndex(o => o.Status);
            modelBuilder.Entity<Order>().HasIndex(o => o.ExpirationDate);
            modelBuilder.Entity<Order>().HasIndex(o => o.NextNotificationDate);
            modelBuilder.Entity<Order>().HasIndex(o => o.NextPaymentDate);
            modelBuilder.Entity<Order>().HasIndex(o => o.IsRecurring);
            modelBuilder.Entity<Billing>().HasOne(o => o.Order).WithMany(o => o.Billings).HasForeignKey(o => o.OrderId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<OrderMetaData>().HasOne(o => o.Order).WithMany(o => o.MetaData).HasForeignKey(o => o.OrderId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Payment>().HasOne(o => o.Order).WithMany(o => o.Payments).HasForeignKey(o => o.OrderId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Order>()
                .HasOne(o => o.PrincipalInvestigator)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.PrincipalInvestigatorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
