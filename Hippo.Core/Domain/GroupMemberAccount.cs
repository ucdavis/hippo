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
        public DateTime? RevokedOn { get; set; } = null;

        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            // The duplicate check on Cluster.IsActive from both Group and Account is necessary for how EF Core handles navigating relationships
            modelBuilder.Entity<GroupMemberAccount>().HasIndex(gm => gm.RevokedOn);
            modelBuilder.Entity<GroupMemberAccount>().HasQueryFilter(gm => 
                gm.Group.IsActive
                && gm.Group.Cluster.IsActive
                && gm.Account.Cluster.IsActive
                && gm.RevokedOn == null
                && gm.Account.DeactivatedOn == null);
        }
    }
}
