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

        [Required]
        public int ClusterId { get; set; }
        public Cluster Cluster { get; set; }

        [JsonIgnore]
        public List<Account> MemberAccounts { get; set; } = new();

        [JsonIgnore]
        public List<Account> AdminAccounts { get; set; } = new();

        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Group>().HasQueryFilter(g => g.Cluster.IsActive);
            modelBuilder.Entity<Group>().HasIndex(g => new { g.ClusterId, g.Name }).IsUnique();

            modelBuilder.Entity<Account>()
                .HasMany(a => a.MemberOfGroups)
                .WithMany(g => g.MemberAccounts)
                .UsingEntity<GroupMemberAccount>();

            modelBuilder.Entity<Account>()
                .HasMany(a => a.AdminOfGroups)
                .WithMany(g => g.AdminAccounts)
                .UsingEntity<GroupAdminAccount>();
        }
    }
}