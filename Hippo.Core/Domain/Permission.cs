using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Hippo.Core.Domain
{
    public class Permission
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int RoleId { get; set; }

        [Required]
        public int UserId { get; set; }
        
        /// <summary>
        /// Optional for cross-cluster roles (currently only the "System" role)
        /// </summary>
        public int? ClusterId { get; set; }

        /// <summary>
        /// Optional for cross-group roles (currently only "ClusterAdmin" role)
        /// </summary>
        public int? GroupId { get; set; }

        public Role Role { get; set; }

        public User User { get; set; }

        public Cluster Cluster { get; set; }

        public Group Group { get; set; }
        
        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Permission>().HasIndex(a => a.RoleId);
            modelBuilder.Entity<Permission>().HasIndex(a => a.UserId);
            modelBuilder.Entity<Permission>().HasIndex(a => a.ClusterId);
            modelBuilder.Entity<Permission>().HasIndex(a => a.GroupId);
        }
    }    
}