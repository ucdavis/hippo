using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Hippo.Core.Extensions;
using Hippo.Core.Data;

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

        public bool IsActive { get; set; } = true;

        [Required]
        public int ClusterId { get; set; }
        public Cluster Cluster { get; set; }

        // TODO: When C# gets type unions, update this to something more strongly typed
        public JsonElement? Data { get; set; }

        [JsonIgnore]
        public List<Account> MemberAccounts { get; set; } = new();

        [JsonIgnore]
        public List<Account> AdminAccounts { get; set; } = new();

        internal static void OnModelCreating(ModelBuilder modelBuilder, DbContext dbContext)
        {
            modelBuilder.Entity<Group>().HasQueryFilter(g => g.IsActive && g.Cluster.IsActive);
            modelBuilder.Entity<Group>().Property(g => g.IsActive).HasDefaultValue(true);
            modelBuilder.Entity<Group>().HasIndex(g => new { g.ClusterId, g.Name }).IsUnique();
            modelBuilder.Entity<Group>().Property(g => g.Data).HasJsonConversion(dbContext);

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