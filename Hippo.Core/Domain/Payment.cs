using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hippo.Core.Domain
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }
        public string FinancialSystemId { get; set; }
        public string TrackingNumber { get; set; } // KFS tracking number
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public decimal Amount { get; set; }
        public string Status { get; set; }

        public string Details { get; set; } //chart strings, credit/debit, and amounts


        [Required]
        public int OrderId { get; set; }
        public Order Order { get; set; }

        //Optional createdBy. If not set, was crated by a job
        public int? CreatedById { get; set; }
        public User CreatedBy { get; set; }

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
            modelBuilder.Entity<Payment>().HasIndex(p => p.Status);
            modelBuilder.Entity<Payment>().HasIndex(p => p.OrderId);
        }
    }
}
