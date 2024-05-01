﻿using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Hippo.Core.Domain
{
    public class FinancialDetail
    {
        [Key]
        public int Id { get; set; }
        [StringLength(128)] //Probably doesn't need to be this big...
        public string FinancialSystemApiKey { get; set; }
        [MaxLength(50)]
        public string FinancialSystemApiSource { get; set; }
        public string ChartString { get; set; }
        public bool AutoApprove { get; set; }
        [Required]
        public int ClusterId { get; set; }
        public Cluster Cluster { get; set; }

        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FinancialDetail>().Property(a => a.AutoApprove).HasDefaultValue(true);
        }
    }
}
