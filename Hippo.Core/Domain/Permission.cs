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

        public Role Role { get; set; }

        public User User { get; set; }

        public Cluster Cluster { get; set; }

        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Permission>().HasIndex(a => a.RoleId);
            modelBuilder.Entity<Permission>().HasIndex(a => a.UserId);
            modelBuilder.Entity<Permission>().HasIndex(a => a.ClusterId);
        }
    }

    public static class PermissionExtensions
    {
        public static bool IsClusterOrSystemAdmin(this IEnumerable<Permission> permissions, string cluster)
        {
            return permissions.Any(p =>
                p.Role.Name == Role.Codes.System
                || (p.Role.Name == Role.Codes.ClusterAdmin && p.Cluster?.Name == cluster));
        }

    }
}