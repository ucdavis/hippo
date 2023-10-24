using System.Linq.Expressions;
using Hippo.Core.Domain;
using Hippo.Core.Models;

namespace Hippo.Web.Models
{
    public class GroupAdminModel
    {
        public int PermissionId { get; set; }
        public GroupModel Group { get; set; } = new();
        public User User { get; set; } = new();

        public static Expression<Func<Permission, GroupAdminModel>> ProjectFromPermission
        {
            get
            {
                return p => new GroupAdminModel
                {
                    PermissionId = p.Id,
                    Group = new GroupModel
                    {
                        Id = p.Group.Id,
                        DisplayName = p.Group.DisplayName,
                        Name = p.Group.Name,
                        Admins = p.Group.Permissions
                        .Where(p => p.Role.Name == Role.Codes.GroupAdmin)
                        .Select(p => new GroupUserModel
                        {
                            Kerberos = p.User.Kerberos,
                            Name = p.User.Name,
                            Email = p.User.Email
                        }).ToList(),
                    },
                    User = p.User
                };
            }
        }
    }
}