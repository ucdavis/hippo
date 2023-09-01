using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Hippo.Core.Domain
{
    public class Group
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(32)]
        public string Name { get; set; } = "";

        [MaxLength(250)]
        public string DisplayName { get; set; } = "";

        public bool IsActive { get; set; }

        [Required]
        public int ClusterId { get; set; }
        [Required]
        public Cluster Cluster { get; set; }

        public List<Permission> Permissions { get; set; } = new();

        [JsonIgnore]
        public List<GroupAccount> GroupAccounts { get; set; } = new();

        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Group>().HasIndex(g => new { g.ClusterId, g.Name }).IsUnique();
            modelBuilder.Entity<Group>().HasQueryFilter(g => g.IsActive);

            modelBuilder.Entity<Permission>()
                .HasOne(p => p.Group)
                .WithMany(g => g.Permissions)
                .HasForeignKey(p => p.GroupId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GroupAccount>()
                .HasOne(ga => ga.Group)
                .WithMany(g => g.GroupAccounts)
                .HasForeignKey(ga => ga.GroupId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}