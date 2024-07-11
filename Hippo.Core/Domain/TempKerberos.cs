using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Hippo.Core.Domain
{
    public class TempKerberos
    {
        public int ClusterId { get; set; }
        public string Kerberos { get; set; } = "";

        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TempKerberos>().HasKey(a => new { a.ClusterId, a.Kerberos });
        }
    }
}