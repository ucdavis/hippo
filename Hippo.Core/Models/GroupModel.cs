using System.Linq.Expressions;
using Hippo.Core.Domain;

namespace Hippo.Core.Models
{
    public class GroupModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string DisplayName { get; set; } = "";

        public List<GroupUserModel> Admins { get; set; } = new();

        public static Expression<Func<Group, GroupModel>> Projection
        {
            get
            {
                return g => new GroupModel 
                { 
                    Id = g.Id, 
                    DisplayName = g.DisplayName, 
                    Name = g.Name,
                    Admins = g.Permissions
                        .Where(p => p.Role.Name == Role.Codes.GroupAdmin)
                        .Select(p => new GroupUserModel 
                        { 
                            Kerberos = p.User.Kerberos,
                            Name = p.User.Name,
                            Email = p.User.Email
                        }).ToList(),
                };
            }
        }
    }

    public class GroupUserModel
    {
        public string Kerberos { get; set; } = "";
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
    }
}