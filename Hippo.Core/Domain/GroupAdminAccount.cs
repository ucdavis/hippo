using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Hippo.Core.Domain
{
    public class GroupAdminAccount
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
            // The duplicate check on Cluster.IsActive from both Group and Account is necessary for how EF Core handles navigating relationships
            modelBuilder.Entity<GroupAdminAccount>().HasQueryFilter(ga => 
                ga.Group.IsActive
                && ga.Group.Cluster.IsActive
                && ga.Account.DeactivatedOn == null
                && ga.Account.Cluster.IsActive);
        }
    }
}
