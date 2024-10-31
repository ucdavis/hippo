using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Hippo.Core.Domain
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(128)]
        public string FinancialSystemId { get; set; } //For sloth, this is the transaction ID (guid)
        [MaxLength(20)]
        public string TrackingNumber { get; set; } // KFS tracking number. So probably only ever 10 characters...
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public decimal Amount { get; set; }
        [MaxLength(20)]
        public string Status { get; set; }
        [MaxLength(250)]
        public string Details { get; set; } //chart strings, credit/debit, and amounts


        [Required]
        public int OrderId { get; set; }
        public Order Order { get; set; }

        //Optional createdBy. If not set, was crated by a job
        public int? CreatedById { get; set; }
        public User CreatedBy { get; set; }

        public DateTime? CompletedOn { get; set; } //Date the payment was completed in Sloth

        public class Statuses
        {
            public const string Created = "Created";
            public const string Processing = "Processing";
            public const string Cancelled = "Cancelled";
            public const string Completed = "Completed";

            public static List<string> StatusTypes = new List<string>
            {
                Created,
                Processing,
                Cancelled,
                Completed
            };
        }

        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Payment>().HasIndex(p => p.OrderId);
            modelBuilder.Entity<Payment>().HasIndex(p => p.Status);
            //DOn't think I need this one
            //modelBuilder.Entity<Payment>().HasOne(p => p.CreatedBy).WithMany().HasForeignKey(a => a.CreatedById).OnDelete(DeleteBehavior.Restrict);
        }
    }


}
