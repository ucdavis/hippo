using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Hippo.Core.Domain
{
    public class GroupMemberAccount
    {
        [Required]
        public int GroupId { get; set; }
        [JsonIgnore]
        public Group Group { get; set; }
        [Required]
        public int AccountId { get; set; }
        [JsonIgnore]
        public Account Account { get; set; }

        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GroupMemberAccount>().HasQueryFilter(gm => gm.Account.Cluster.IsActive);
        }
    }
}
