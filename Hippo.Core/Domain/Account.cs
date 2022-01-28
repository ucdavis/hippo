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
        [Key]
        public int Id { get; set; }

        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }

        public bool CanSponsor { get; set; }

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
    }
}
