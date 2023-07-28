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
        [MaxLength(32)]
        public string GroupName { get; set; }
        [MaxLength(20)]
        public string UserKerberos { get; set; }
        public PuppetGroup Group { get; set; }
        public PuppetUser User { get; set; }

        internal static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<PuppetGroupPuppetUser>()
                .HasKey(gu => new { gu.GroupName, gu.UserKerberos });
            builder.Entity<PuppetGroupPuppetUser>()
                .HasOne(gu => gu.Group)
                .WithMany()
                .HasForeignKey(gu => gu.GroupName);
            builder.Entity<PuppetGroupPuppetUser>()
                .HasOne(gu => gu.User)
                .WithMany()
                .HasForeignKey(gu => gu.UserKerberos);
        }
    }
}
