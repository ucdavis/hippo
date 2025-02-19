using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using Hippo.Core.Data;
using Hippo.Core.Domain;
using Hippo.Core.Extensions;
using Hippo.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Hippo.Core.Domain
{
    public class Request
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string Action { get; set; } = "";
        [Required]
        [JsonIgnore]
        public User Requester { get; set; }
        [Required]
        public int RequesterId { get; set; }
        [JsonIgnore]
        public User Actor { get; set; }
        public int? ActorId { get; set; }
        [MaxLength(32)]
        [JsonIgnore]
        public string Group { get; set; }
        [Required]
        public Cluster Cluster { get; set; }
        [Required]
        public int ClusterId { get; set; }
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "";
        // TODO: When C# gets type unions, update this to something more strongly typed
        public JsonElement? Data { get; set; }
        [Obsolete($"Use {nameof(AccountRequestDataModel)}.{nameof(AccountRequestDataModel.SshKey)}")]
        public string SshKey { get; set; } = "";
        [MaxLength(100)]
        [Obsolete($"Use {nameof(AccountRequestDataModel)}.{nameof(AccountRequestDataModel.SupervisingPI)}")]
        public string SupervisingPI { get; set; } = "";
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedOn { get; set; } = DateTime.UtcNow;
        [JsonIgnore]
        public List<QueuedEvent> QueuedEvents { get; set; } = new();

        internal static void OnModelCreating(ModelBuilder modelBuilder, DbContext dbContext)
        {
            modelBuilder.Entity<Request>().HasQueryFilter(r => r.Cluster.IsActive);
            modelBuilder.Entity<Request>().HasIndex(r => r.Action);
            modelBuilder.Entity<Request>().HasIndex(r => r.Status);
            modelBuilder.Entity<Request>().HasIndex(r => r.Group);
            modelBuilder.Entity<Request>().HasIndex(r => r.RequesterId);
            modelBuilder.Entity<Request>().HasIndex(r => r.ActorId);
            modelBuilder.Entity<Request>().HasIndex(r => r.ClusterId);

            modelBuilder.Entity<Request>().HasOne(r => r.Requester)
                .WithMany()
                .HasForeignKey(r => r.RequesterId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Request>().HasOne(r => r.Actor)
                .WithMany()
                .HasForeignKey(r => r.ActorId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Request>().HasOne(r => r.Cluster)
                .WithMany()
                .HasForeignKey(r => r.ClusterId)
                .OnDelete(DeleteBehavior.Restrict);

                modelBuilder.Entity<Request>().Property(r => r.Data).HasJsonConversion(dbContext);
        }

        public static class Actions
        {
            public const string CreateAccount = "CreateAccount";
            public const string AddAccountToGroup = "AddAccountToGroup";
            public const string CreateGroup = "CreateGroup";
        }

        public static class Statuses
        {
            public const string PendingApproval = "PendingApproval";
            public const string Rejected = "Rejected";
            public const string Processing = "Processing";
            public const string Completed = "Completed";
            public const string Canceled = "Canceled";

            public static readonly string[] Pending = new[] { PendingApproval, Processing };
        }
    }
}