using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Hippo.Core.Domain
{
    public class GroupAccount
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int GroupId { get; set; }
        public Group Group { get; set; }

        [Required]
        public int AccountId { get; set; }
        public Account Account { get; set; }


        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GroupAccount>().HasIndex(g => new { g.GroupId, g.AccountId }).IsUnique();
            modelBuilder.Entity<GroupAccount>().HasIndex(g => g.AccountId);
        }
    }
}