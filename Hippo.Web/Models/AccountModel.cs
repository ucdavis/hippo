using System.Linq.Expressions;
using Hippo.Core.Domain;
using Hippo.Core.Models;

namespace Hippo.Web.Models
{
    public class AccountModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Status { get; set; } = "";
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
            Status = account.Status.ToString();
            CreatedOn = account.CreatedOn;
            Cluster = account.Cluster.Name;
            Owner = account.Owner;
            Groups = account.GroupAccounts.Select(ga => new GroupModel
            {
                Id = ga.Group.Id,
                DisplayName = ga.Group.DisplayName,
                Name = ga.Group.Name,
                Admins = ga.Group.Permissions
                    .Where(p => p.Role.Name == Role.Codes.GroupAdmin)
                    .Select(p => new GroupUserModel
                    {
                        Kerberos = p.User.Kerberos,
                        Name = p.User.Name,
                        Email = p.User.Email
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
                    Status = a.Status.ToString(),
                    CreatedOn = a.CreatedOn,
                    Cluster = a.Cluster.Name,
                    Owner = a.Owner,
                    Groups = a.GroupAccounts.Select(ga => new GroupModel
                    {
                        Id = ga.Group.Id,
                        DisplayName = ga.Group.DisplayName,
                        Name = ga.Group.Name,
                        Admins = ga.Group.Permissions
                            .Where(p => p.Role.Name == Role.Codes.GroupAdmin)
                            .Select(p => new GroupUserModel
                            {
                                Kerberos = p.User.Kerberos,
                                Name = p.User.Name,
                                Email = p.User.Email
                            }).ToList(),
                    }).OrderBy(x => x.Name).ToList(),
                    UpdatedOn = a.UpdatedOn
                };
            }
        }
    }
}