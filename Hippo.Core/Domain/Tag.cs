using System.ComponentModel.DataAnnotations;

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
}
