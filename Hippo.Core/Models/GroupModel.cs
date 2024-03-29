using System.Linq.Expressions;
using Hippo.Core.Domain;

namespace Hippo.Core.Models
{
    public class GroupModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string DisplayName { get; set; } = "";

        public List<GroupAccountModel> Admins { get; set; } = new();

        public static Expression<Func<Group, GroupModel>> Projection
        {
            get
            {
                return g => new GroupModel 
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
                };
            }
        }
    }

    public class GroupAccountModel
    {
        public string Kerberos { get; set; } = "";
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
    }
}