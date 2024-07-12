using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Hippo.Core.Domain
{
    public class TempGroup
    {
        public int ClusterId { get; set; }
        public string Group { get; set; } = "";

        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TempGroup>().HasKey(a => new { a.ClusterId, a.Group });
        }
    }
}