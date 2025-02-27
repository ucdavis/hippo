using System.ComponentModel.DataAnnotations;
using Hippo.Core.Data;
using Hippo.Core.Extensions;
using Hippo.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Hippo.Core.Domain;

public class QueuedEvent
{
    public int Id { get; set; }
    [MaxLength(50)]
    public string Action { get; set; } = "";
    [MaxLength(50)]
    public string Status { get; set; } = "";
    public QueuedEventDataModel Data { get; set; }
    public string ErrorMessage { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public int? RequestId { get; set; }
    public Request Request { get; set; }

    internal static void OnModelCreating(ModelBuilder modelBuilder, DbContext dbContext)
    {
        modelBuilder.Entity<QueuedEvent>().HasIndex(qe => qe.Action);
        modelBuilder.Entity<QueuedEvent>().HasIndex(qe => qe.Status);
        modelBuilder.Entity<QueuedEvent>().HasIndex(qe => qe.CreatedAt);
        modelBuilder.Entity<QueuedEvent>().HasIndex(qe => qe.UpdatedAt);
        modelBuilder.Entity<QueuedEvent>().HasOne(qe => qe.Request)
            .WithMany(r => r.QueuedEvents)
            .HasForeignKey(qe => qe.RequestId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<QueuedEvent>().Property(g => g.Data).HasJsonConversion(dbContext);
    }

    public static class Actions
    {
        public const string CreateAccount = "CreateAccount";
        public const string UpdateSshKey = "UpdateSshKey";
        public const string AddAccountToGroup = "AddAccountToGroup";
        public const string CreateGroup = "CreateGroup";
        public const string RemoveAccountFromGroup = "RemoveAccountFromGroup";

        public const string RegexPattern = CreateAccount
            + "|" + UpdateSshKey
            + "|" + AddAccountToGroup
            + "|" + CreateGroup
            + "|" + RemoveAccountFromGroup;
    }

    public static class Statuses
    {
        public const string Pending = "Pending";
        public const string Complete = "Complete";
        public const string Failed = "Failed";
        public const string Canceled = "Canceled";

        public const string RegexPattern = Pending
            + "|" + Complete
            + "|" + Failed
            + "|" + Canceled;

        public static List<string> AllStatuses = new()
        {
            Pending,
            Complete,
            Failed,
            Canceled
        };
    }
}
