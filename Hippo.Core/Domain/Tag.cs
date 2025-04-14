using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Hippo.Core.Domain;

public class Tag
{
    public int Id { get; set; }
    [MaxLength(50)]
    [Required]
    public string Name { get; set; }

    public int ClusterId { get; set; }
    public Cluster Cluster { get; set; }

    public List<Account> Accounts { get; set; } = new();

    internal static void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tag>().HasQueryFilter(t => t.Cluster.IsActive);
    }
}

