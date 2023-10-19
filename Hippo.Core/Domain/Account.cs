﻿using System;
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
            IsActive = true;
            Status = Statuses.PendingApproval;
            Histories = new List<History>();
        }

        [Key]
        public int Id { get; set; }

        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }

        /// <summary>
        /// Sponsor must resolve to a user (or at least an email?) 
        /// if the department wants a specific name, it can be added here. 
        /// Otherwise we should use the User.Name
        /// This is only when account CanSponsor is true
        /// </summary>
        [MaxLength(100)]
        public string Name { get; set; }
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

        public int ClusterId { get;set;}
        [Required]
        public Cluster Cluster { get; set; }


        [JsonIgnore]
        public List<History> Histories { get; set; }

        [JsonIgnore]
        public List<GroupAccount> GroupAccounts { get; set; } = new();

        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>().HasQueryFilter(a => a.IsActive);
            modelBuilder.Entity<Account>().HasIndex(a => a.CreatedOn);
            modelBuilder.Entity<Account>().HasIndex(a => a.UpdatedOn);
            modelBuilder.Entity<Account>().HasIndex(a => a.OwnerId);
            modelBuilder.Entity<Account>().HasIndex(a => a.SponsorId);
            modelBuilder.Entity<Account>().HasIndex(a => a.Name);

            //self referencing foreign key
            modelBuilder.Entity<Account>().HasOne(a => a.Sponsor).WithMany().HasForeignKey(a => a.SponsorId);

            modelBuilder.Entity<GroupAccount>()
                .HasOne(ga => ga.Account)
                .WithMany(a => a.GroupAccounts)
                .HasForeignKey(ga => ga.AccountId)
                .OnDelete(DeleteBehavior.Restrict);                
        }

        public class Statuses
        {
            public const string Processing = "Processing";
            public const string PendingApproval = "PendingApproval";
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
