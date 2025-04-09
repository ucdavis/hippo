using System;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Text.Json.Serialization;
using Hippo.Core.Domain;
using Hippo.Core.Validation;

namespace Hippo.Core.Models;

public class ClusterModel
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(20)]
    public string Name { get; set; } = String.Empty;
    [Required]
    [MaxLength(250)]
    public string Description { get; set; } = String.Empty;
    [MaxLength(250)]
    public string SshName { get; set; } = String.Empty;
    [MaxLength(40)]
    public string SshKeyId { get; set; } = String.Empty;
    [MaxLength(250)]
    public string SshUrl { get; set; } = String.Empty;
    public bool IsActive { get; set; } = true;
    [MaxLength(250)]
    public string Domain { get; set; } = String.Empty;
    [MaxLength(250)]
    [EmailAddress]
    public string Email { get; set; } = String.Empty;
    public string SshKey { get; set; } = String.Empty;
    [ListOfStringsOptions(AccessType.Codes.RegexPattern, nonEmpty: true)]
    public List<string> AccessTypes { get; set; } = new();
    [MaxLength(250)]
    [Url]
    public string AcceptableUsePolicyUrl { get; set; } = null;
    public DateTime? AcceptableUsePolicyUpdatedOn { get; set; }
    public bool AllowOrders { get; set; } = false;

    public ClusterModel()
    {

    }

    public ClusterModel(Cluster cluster, string sshKey)
    {
        Id = cluster.Id;
        Name = cluster.Name;
        Description = cluster.Description;
        SshName = cluster.SshName;
        SshKeyId = cluster.SshKeyId;
        SshUrl = cluster.SshUrl;
        IsActive = cluster.IsActive;
        Domain = cluster.Domain;
        Email = cluster.Email;
        AccessTypes = cluster.AccessTypes.Select(at => at.Name).ToList();
        SshKey = sshKey;
        AcceptableUsePolicyUrl = cluster.AcceptableUsePolicyUrl;
        AcceptableUsePolicyUpdatedOn = cluster.AcceptableUsePolicyUpdatedOn;
    }

    public static Expression<Func<Cluster, ClusterModel>> Projection
    {
        get
        {
            return c => new ClusterModel
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                SshName = c.SshName,
                SshKeyId = c.SshKeyId,
                SshUrl = c.SshUrl,
                IsActive = c.IsActive,
                Domain = c.Domain,
                Email = c.Email,
                AllowOrders = c.FinancialDetail != null,
                AccessTypes = c.AccessTypes.Select(at => at.Name).ToList(),
                AcceptableUsePolicyUrl = c.AcceptableUsePolicyUrl,
                AcceptableUsePolicyUpdatedOn = c.AcceptableUsePolicyUpdatedOn
            };
        }
    }

}

