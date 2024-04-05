using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Hippo.Core.Domain;
using Hippo.Core.Extensions;
using Hippo.Core.Validation;

namespace Hippo.Core.Models;

public class QueuedEventModel
{
    public int Id { get; set; }
    [Required]
    [StringOptions(QueuedEvent.Actions.RegexPattern)]
    public string Action { get; set; } = "";
    [Required]
    [StringOptions(QueuedEvent.Statuses.RegexPattern)]
    public string Status { get; set; } = "";
    [Required]
    public QueuedEventDataModel Data { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public static QueuedEventModel FromQueuedEvent(QueuedEvent queuedEvent)
    {
        return new QueuedEventModel
        {
            Id = queuedEvent.Id,
            Action = queuedEvent.Action,
            Status = queuedEvent.Status,
            Data = JsonSerializer.Deserialize<QueuedEventDataModel>(queuedEvent.Data, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }),
            CreatedAt = queuedEvent.CreatedAt,
            UpdatedAt = queuedEvent.UpdatedAt
        };
    }
}

public class QueuedEventDataModel
{
    [Required]
    public List<QueuedEventGroupModel> Groups { get; set; } = new();
    [Required]
    public List<QueuedEventAccountModel> Accounts { get; set; } = new();
    [Required]
    public string Cluster { get; set; } = "";
    [Required]
    public Dictionary<string, string> Metadata { get; set; } = new();

    public static QueuedEventDataModel FromRequestAndGroup(Request request, Group group)
    {
        return new QueuedEventDataModel
        {
            Groups = new List<QueuedEventGroupModel> { QueuedEventGroupModel.FromGroup(group) },
            Accounts = new List<QueuedEventAccountModel> { QueuedEventAccountModel.FromRequest(request) },
            Cluster = request.Cluster.Name,
        };
    }
    public static QueuedEventDataModel FromAccountAndGroup(Account account, Group group)
    {
        return new QueuedEventDataModel
        {
            Groups = new List<QueuedEventGroupModel> { QueuedEventGroupModel.FromGroup(group) },
            Accounts = new List<QueuedEventAccountModel> { QueuedEventAccountModel.FromAccount(account) },
            Cluster = account.Cluster.Name,
        };
    }

    public static QueuedEventDataModel FromAccount(Account account)
    {
        return new QueuedEventDataModel
        {
            Accounts = new List<QueuedEventAccountModel> { QueuedEventAccountModel.FromAccount(account) },
            Cluster = account.Cluster.Name,
        };
    }
}

public class QueuedEventGroupModel
{
    [Required]
    [MaxLength(32)]
    public string Name { get; set; } = "";

    public static QueuedEventGroupModel FromGroup(Group group)
    {
        return new QueuedEventGroupModel
        {
            Name = group.Name
        };
    }
}

public class QueuedEventAccountModel
{
    [Required]
    [MaxLength(20)]
    public string Kerberos { get; set; } = "";
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = "";
    [Required]
    [MaxLength(300)]
    public string Email { get; set; } = "";
    [Required]
    [MaxLength(10)]
    public string Iam { get; set; } = "";
    [Required]
    [MaxLength(20)]
    public string Mothra { get; set; } = "";
    public string Key { get; set; } = "";
    [ListOfStringsOptions(AccessType.Codes.RegexPattern, nonEmpty: true)]
    public List<string> AccessTypes { get; set; } = new();


    public static QueuedEventAccountModel FromAccount(Account account)
    {
        return new QueuedEventAccountModel
        {
            Kerberos = account.Owner.Kerberos,
            Name = account.Name,
            Email = account.Owner.Email,
            Iam = account.Owner.Iam,
            Mothra = account.Owner.MothraId,
            Key = account.SshKey,
            AccessTypes = account.AccessTypes.Select(at => at.Name).ToList()
        };
    }

    public static QueuedEventAccountModel FromRequest(Request request)
    {
        var requestData = request.GetAccountRequestData();
        return new QueuedEventAccountModel
        {
            Kerberos = request.Requester.Kerberos,
            Name = request.Requester.Name,
            Email = request.Requester.Email,
            Iam = request.Requester.Iam,
            Mothra = request.Requester.MothraId,
            Key = requestData.SshKey,
            AccessTypes = requestData.AccessTypes
        };
    }
}


