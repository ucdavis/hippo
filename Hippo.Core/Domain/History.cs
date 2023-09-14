using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hippo.Core.Domain
{
    public class History
    {
        public History()
        {
            ActedDate = DateTime.UtcNow;
        }


        [Key]
        public int Id { get; set; }
        public DateTime ActedDate { get; set; }
        public int ActedById { get; set; }
        public User ActedBy { get; set; }

        public bool AdminAction { get; set; } //If we want to dump non admin actions in here

        [MaxLength(100)]
        public string Action { get; set; } = String.Empty;

        public string Details { get; set; } = String.Empty;

        public int? AccountId { get; set; }
        public Account Account { get; set; }

        public int ClusterId { get; set; } //When we have a cluster identifier 
        public Cluster Cluster { get; set; }

        [MaxLength(50)]
        public string AccountStatus { get; set; }

        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<History>().HasOne(h => h.ActedBy).WithMany().HasForeignKey(a => a.ActedById).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<History>().HasOne(h => h.Account).WithMany(a => a.Histories).HasForeignKey(a => a.AccountId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<History>().HasOne(h => h.Cluster).WithMany().HasForeignKey(a => a.ClusterId).OnDelete(DeleteBehavior.Restrict);
        }

        public class Actions
        {
            public const string Requested = "Requested";
            public const string Approved = "Approved";
            public const string Updated = "Updated";
            public const string Rejected = "Rejected";
            public const string AdminApproved = "Admin Approved";
            public const string AdminRejected = "Admin Rejected";
            public const string Other = "Other";

            public static List<string> TypeList = new List<string>
            {
                Requested,
                Approved,
                Updated,
                Rejected,
                AdminApproved,
                AdminRejected,
                Other,
            }.ToList();
        }
    }
}
