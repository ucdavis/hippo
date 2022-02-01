using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Hippo.Core.Domain
{
    public class Account    
    {
        public Account()
        {
            CreatedOn = DateTime.UtcNow;
            UpdatedOn = DateTime.UtcNow;
            IsActive = false;
            Status = Statuses.PendingApproval;
        }

        [Key]
        public int Id { get; set; }

        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }

        public bool CanSponsor { get; set; }
        public bool IsActive { get;set;}

        public string SshKey { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; }

        [Required]
        public int OwnerId { get; set; }
        [Required]
        public User Owner { get; set; }

        public int? SponsorId { get; set; }
        public Account Sponsor { get; set; }

        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>().HasIndex(a => a.CreatedOn);
            modelBuilder.Entity<Account>().HasIndex(a => a.UpdatedOn);
            modelBuilder.Entity<Account>().HasIndex(a => a.OwnerId);
            modelBuilder.Entity<Account>().HasIndex(a => a.SponsorId);
            //self referencing foreign key
            modelBuilder.Entity<Account>().HasOne(a => a.Sponsor).WithMany().HasForeignKey(a => a.SponsorId);
        }

        public class Statuses
        {
            public const string Processing = "Processing";
            public const string PendingApproval = "Pending Approval";
            public const string Rejected = "Rejected";
            public const string Active = "Active";

            public static List<string> TypeList = new List<string>
            {
                Processing,
                PendingApproval,
                Rejected,
                Active,
            }.ToList();
        }
    }
}
