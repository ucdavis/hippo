using System.Linq.Expressions;
using System.Text.Json;
using Hippo.Core.Domain;
using Hippo.Core.Models;

namespace Hippo.Web.Models
{
    public class AccountModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Kerberos { get; set; } = "";
        public DateTime CreatedOn { get; set; }
        public string Cluster { get; set; } = "";
        public User? Owner { get; set; }
        public List<GroupModel> MemberOfGroups { get; set; } = new();
        public List<GroupModel> AdminOfGroups { get; set; } = new();
        public List<string> AccessTypes { get; set; } = new();
        public DateTime UpdatedOn { get; set; }
        public JsonElement? Data { get; set; }
        public List<string> Tags { get; set; } = new();
        public DateTime? AcceptableUsePolicyAgreedOn { get; set; }

        public AccountModel()
        {

        }

        public AccountModel(Account account)
        {
            Id = account.Id;
            Name = account.Name;
            Email = account.Email;
            Kerberos = account.Kerberos;
            CreatedOn = account.CreatedOn;
            Cluster = account.Cluster.Name;
            Owner = account.Owner;
            AcceptableUsePolicyAgreedOn = account.AcceptableUsePolicyAgreedOn;
            MemberOfGroups = account.MemberOfGroups.Select(g => new GroupModel
            {
                Id = g.Id,
                DisplayName = g.DisplayName,
                Name = g.Name,
                Admins = g.AdminAccounts
                    .Select(a => new GroupAccountModel
                    {
                        Kerberos = a.Kerberos,
                        Name = a.Name,
                        Email = a.Email
                    }).ToList(),
                Data = g.Data
            })
            .OrderBy(x => x).ToList();
            AdminOfGroups = account.AdminOfGroups.Select(g => new GroupModel
            {
                Id = g.Id,
                DisplayName = g.DisplayName,
                Name = g.Name,
                Admins = g.AdminAccounts
                    .Select(a => new GroupAccountModel
                    {
                        Kerberos = a.Kerberos,
                        Name = a.Name,
                        Email = a.Email
                    }).ToList(),
                Data = g.Data
            })
            .OrderBy(x => x).ToList();
            UpdatedOn = account.UpdatedOn;
            AccessTypes = account.AccessTypes.Select(at => at.Name).ToList();
            Data = account.Data;
            Tags = account.Tags.Select(t => t.Name).ToList();
        }

        public static Expression<Func<Account, AccountModel>> GetProjection(bool isClusterOrSystemAdmin, int currentUserId)
        {
            return a => new AccountModel
            {
                Id = a.Id,
                Name = a.Name,
                Email = a.Email,
                Kerberos = a.Kerberos,
                CreatedOn = a.CreatedOn,
                Cluster = a.Cluster.Name,
                Owner = a.Owner,
                AcceptableUsePolicyAgreedOn = a.AcceptableUsePolicyAgreedOn,
                MemberOfGroups = a.MemberOfGroups.Select(g => new GroupModel
                {
                    Id = g.Id,
                    DisplayName = g.DisplayName,
                    Name = g.Name,
                    Admins = g.AdminAccounts
                        .Select(a => new GroupAccountModel
                        {
                            Kerberos = a.Kerberos,
                            Name = a.Name,
                            Email = a.Email
                        }).ToList(),
                    // permission to view account doesn't imply permission to view groups that account belongs to
                    Data = isClusterOrSystemAdmin
                        || g.AdminAccounts.Any(a => a.OwnerId == currentUserId)
                        || g.MemberAccounts.Any(a => a.OwnerId == currentUserId)
                        ? g.Data : null
                }).OrderBy(x => x.Name).ToList(),
                AdminOfGroups = a.AdminOfGroups.Select(g => new GroupModel
                {
                    Id = g.Id,
                    DisplayName = g.DisplayName,
                    Name = g.Name,
                    Admins = g.AdminAccounts
                        .Select(a => new GroupAccountModel
                        {
                            Kerberos = a.Kerberos,
                            Name = a.Name,
                            Email = a.Email
                        }).ToList(),
                    // permission to view account doesn't imply permission to view groups that account belongs to
                    Data = isClusterOrSystemAdmin
                        || g.AdminAccounts.Any(a => a.OwnerId == currentUserId)
                        || g.MemberAccounts.Any(a => a.OwnerId == currentUserId)
                        ? g.Data : null
                }).OrderBy(x => x.Name).ToList(),
                UpdatedOn = a.UpdatedOn,
                AccessTypes = a.AccessTypes.Select(at => at.Name).ToList(),
                Data = a.Data,
                Tags = a.Tags.Select(t => t.Name).ToList()
            };
        }
    }
}