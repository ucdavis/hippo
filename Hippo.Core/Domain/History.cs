﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hippo.Core.Domain
{
    public class History
    {
        public History()
        {
            ActedDate = DateTime.UtcNow;
        }


        [Key]
        public int Id { get; set; }
        public DateTime ActedDate { get; set; }
        public int? ActedById { get; set; }
        public User ActedBy { get; set; }

        public bool AdminAction { get; set; } //If we want to dump non admin actions in here

        [MaxLength(100)]
        public string Action { get; set; } = String.Empty;

        public string Details { get; set; } = String.Empty;

        public int ClusterId { get; set; } //When we have a cluster identifier 
        public Cluster Cluster { get; set; }

        [MaxLength(50)]
        public string Status { get; set; }

        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<History>().HasQueryFilter(h => h.Cluster.IsActive);
            modelBuilder.Entity<History>().HasIndex(h => h.ActedDate);
            modelBuilder.Entity<History>().HasIndex(h => h.Action);
            modelBuilder.Entity<History>().HasIndex(h => h.ClusterId);
            modelBuilder.Entity<History>().HasOne(h => h.ActedBy).WithMany().HasForeignKey(a => a.ActedById).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<History>().HasOne(h => h.Cluster).WithMany().HasForeignKey(a => a.ClusterId).OnDelete(DeleteBehavior.Restrict);
        }

        public class Actions
        {
            public const string Requested = "Requested";
            public const string Approved = "Approved";
            public const string Updated = "Updated";
            public const string Rejected = "Rejected";
            public const string Completed = "Completed";
            public const string Processing = "Processing";
            public const string PuppetDataSynced = "Puppet Data Synced";
            public const string RoleRemoved = "Role Removed";
            public const string RoleAdded = "Role Added";
            public const string QueuedEventCreated = "Queued Event Created";
            public const string QueuedEventUpdated = "Queued Event Updated";

            public static List<string> TypeList = new List<string>
            {
                Requested,
                Approved,
                Updated,
                Rejected,
                Completed,
                Processing,
                PuppetDataSynced,
                RoleRemoved,
                RoleAdded,
                QueuedEventCreated,
                QueuedEventUpdated
            }.ToList();
        }
    }
}
