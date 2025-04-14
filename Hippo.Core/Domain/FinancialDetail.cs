using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Hippo.Core.Domain
{
    public class FinancialDetail
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(200)]
        public string SecretAccessKey { get; set; } //Used to get the FinancialSystemApiKey from the secret service
        [Required]
        [MaxLength(50)]
        public string FinancialSystemApiSource { get; set; }
        [MaxLength(128)]
        public string ChartString { get; set; }
        public bool AutoApprove { get; set; }
        [Required]
        public int ClusterId { get; set; }
        public Cluster Cluster { get; set; }

        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FinancialDetail>().HasQueryFilter(fd => fd.Cluster.IsActive);
            modelBuilder.Entity<FinancialDetail>().Property(a => a.AutoApprove).HasDefaultValue(true);
        }
    }
}
