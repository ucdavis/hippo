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

        [JsonIgnore]
        public List<Account> Accounts { get; set; }

        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cluster>().HasIndex(a => a.Name );

            modelBuilder.Entity<Account>()
                .HasOne(a => a.Cluster)
                .WithMany(a => a.Accounts)
                .HasForeignKey(a => a.ClusterId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
