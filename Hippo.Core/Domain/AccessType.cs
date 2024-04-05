using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Hippo.Core.Domain
{
    public class AccessType
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(50)]
        [Required]
        public string Name { get; set; }

        public List<Cluster> Clusters { get; set; } = new();

        public List<Account> Accounts { get; set; } = new();

        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AccessType>().HasIndex(a => a.Name).IsUnique();
        }

        public class Codes
        {
            public const string SshKey = "SshKey";
            public const string OpenOnDemand = "OpenOnDemand";

            public const string RegexPattern = $"{SshKey}|{OpenOnDemand}";
        }
    }
}