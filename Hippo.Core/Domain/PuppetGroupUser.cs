using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Hippo.Core.Domain
{
    public class PuppetGroupPuppetUser
    {
        [MaxLength(20)]
        public string ClusterName { get; set; }
        [MaxLength(32)]
        public string GroupName { get; set; }
        [MaxLength(20)]
        public string UserKerberos { get; set; }

        internal static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<PuppetGroupPuppetUser>()
                .HasKey(gu => new { gu.ClusterName, gu.GroupName, gu.UserKerberos });
            builder.Entity<PuppetGroupPuppetUser>()
                .HasIndex(gu => gu.GroupName);
            builder.Entity<PuppetGroupPuppetUser>()
                .HasIndex(gu => gu.UserKerberos);
        }
    }
}
