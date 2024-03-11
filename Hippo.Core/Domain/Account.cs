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

        [MaxLength(300)]
        [EmailAddress]
        public string Email { get; set; }

        [MaxLength(20)]
        public string Kerberos { get; set; }        

        public string SshKey { get; set; }

        public int? OwnerId { get; set; }
        public User Owner { get; set; }

        [Required]
        public int ClusterId { get; set; }
        public Cluster Cluster { get; set; }

        [JsonIgnore]
        public List<Group> MemberOfGroups { get; set; } = new();

        [JsonIgnore]
        public List<Group> AdminOfGroups { get; set; } = new();

        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>().HasQueryFilter(a => a.Cluster.IsActive);
            modelBuilder.Entity<Account>().HasIndex(a => a.CreatedOn);
            modelBuilder.Entity<Account>().HasIndex(a => a.UpdatedOn);
            modelBuilder.Entity<Account>().HasIndex(a => a.OwnerId);
            modelBuilder.Entity<Account>().HasIndex(a => a.Name);
            modelBuilder.Entity<Account>().HasIndex(a => a.Email);
            modelBuilder.Entity<Account>().HasIndex(a => a.Kerberos);
        }
    }
}
