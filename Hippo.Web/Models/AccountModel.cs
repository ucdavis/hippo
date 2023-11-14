using System.Linq.Expressions;
using Hippo.Core.Domain;
using Hippo.Core.Models;

namespace Hippo.Web.Models
{
    public class AccountModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public DateTime CreatedOn { get; set; }
        public string Cluster { get; set; } = "";
        public User? Owner { get; set; }
        public List<GroupModel> Groups { get; set; } = new();
        public DateTime UpdatedOn { get; set; }

        public AccountModel()
        {

        }

        public AccountModel(Account account)
        {
            Id = account.Id;
            Name = account.Name;
            Email = account.Email;
            CreatedOn = account.CreatedOn;
            Cluster = account.Cluster.Name;
            Owner = account.Owner;
            Groups = account.MemberOfGroups.Select(g => new GroupModel
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
            })
            .OrderBy(x => x).ToList();
            UpdatedOn = account.UpdatedOn;
        }

        public static Expression<Func<Account, AccountModel>> Projection
        {
            get
            {
                return a => new AccountModel
                {
                    Id = a.Id,
                    Name = a.Name,
                    Email = a.Email,
                    CreatedOn = a.CreatedOn,
                    Cluster = a.Cluster.Name,
                    Owner = a.Owner,
                    Groups = a.MemberOfGroups.Select(g => new GroupModel
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
                    }).OrderBy(x => x.Name).ToList(),
                    UpdatedOn = a.UpdatedOn
                };
            }
        }
    }
}