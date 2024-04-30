using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Hippo.Core.Domain
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [MaxLength(300)]
        [EmailAddress]
        public string Email { get; set; }

        [MaxLength(10)]
        public string Iam { get; set; }

        [MaxLength(20)]
        public string Kerberos { get; set; }

        [MaxLength(20)] //It probably isn't this long....
        public string MothraId { get; set; }

        [JsonIgnore]
        public List<Account> Accounts { get; set; } = new();

        [Display(Name = "Name")]
        public string Name => FirstName + " " + LastName;

        [JsonIgnore]
        public List<Permission> Permissions { get; set; } = new();

        [JsonIgnore]
        public List<Order> Orders { get; set; } = new();


        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasIndex(a => a.Iam).IsUnique();
            modelBuilder.Entity<User>().HasIndex(a => a.Email);

            modelBuilder.Entity<Account>()
                .HasOne(a => a.Owner)
                .WithMany(a => a.Accounts)
                .HasForeignKey(a => a.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Permission>()
                .HasOne(p => p.User)
                .WithMany(u => u.Permissions)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Order>()
                .HasOne(o => o.PrincipalInvestigator)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.PrincipalInvestigatorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
