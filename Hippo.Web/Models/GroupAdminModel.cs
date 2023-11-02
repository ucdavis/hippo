using System.Linq.Expressions;
using Hippo.Core.Domain;
using Hippo.Core.Models;

namespace Hippo.Web.Models
{
    public class GroupAdminModel
    {
        public GroupModel Group { get; set; } = new();
        public GroupAccountModel Account { get; set; } = new();

        public static Expression<Func<Account, IEnumerable<GroupAdminModel>>> ProjectFromAccount
        {
            get
            {
                return a => a.AdminOfGroups.Select(g => new GroupAdminModel
                {
                    Group = new GroupModel
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
                    },
                    Account = new GroupAccountModel
                    {
                        Kerberos = a.Kerberos,
                        Name = a.Name,
                        Email = a.Email
                    }
                });
            }
        }
    }
}