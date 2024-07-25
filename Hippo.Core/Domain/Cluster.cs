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
    public class Cluster
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string Name { get; set; } = String.Empty;
        [Required]
        [MaxLength(250)]
        public string Description { get; set; } = String.Empty;
        [MaxLength(250)]
        public string SshName { get; set; } = String.Empty;
        [MaxLength(40)]
        public string SshKeyId { get; set; } = String.Empty;
        [MaxLength(250)]
        public string SshUrl { get; set; } = String.Empty;
        public bool IsActive { get; set; } = true;
        [MaxLength(250)]
        public string Domain { get; set; } = String.Empty;
        [MaxLength(250)]
        [EmailAddress]
        public string Email { get; set; } = String.Empty;


        [JsonIgnore]
        public List<Account> Accounts { get; set; } = new();

        [JsonIgnore]
        public List<Group> Groups { get; set; } = new();

        [JsonIgnore]
        [MinLength(1)]
        public List<AccessType> AccessTypes { get; set; } = new();

        [JsonIgnore]
        public FinancialDetail FinancialDetail { get; set; }

        [JsonIgnore]
        public List<Product> Products { get; set; } = new();

        [JsonIgnore]
        public List<Order> Orders { get; set; } = new();

        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cluster>().HasQueryFilter(a => a.IsActive);
            modelBuilder.Entity<Cluster>().Property(a => a.IsActive).HasDefaultValue(true);
            modelBuilder.Entity<Cluster>().HasIndex(a => a.Name);

            modelBuilder.Entity<Account>()
                .HasOne(a => a.Cluster)
                .WithMany(a => a.Accounts)
                .HasForeignKey(a => a.ClusterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Group>()
                .HasOne(a => a.Cluster)
                .WithMany(a => a.Groups)
                .HasForeignKey(a => a.ClusterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Permission>()
                .HasOne(p => p.Cluster)
                .WithMany()
                .HasForeignKey(p => p.ClusterId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cluster has a one to one relationship with FinancialDetail where the financial detail is nullable
            modelBuilder.Entity<Cluster>()
                .HasOne(c => c.FinancialDetail)
                .WithOne(c => c.Cluster)
                .HasForeignKey<FinancialDetail>(fd => fd.ClusterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Cluster)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.ClusterId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Cluster)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.ClusterId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
