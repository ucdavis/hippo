using System.Linq.Expressions;
using Hippo.Core.Domain;
using Hippo.Core.Models;

namespace Hippo.Web.Models
{
    public class RequestModel
    {
        public int Id { get; set; }
        public string Action { get; set; } = "";
        public string RequesterEmail { get; set; } = "";
        public string RequesterName { get; set; } = "";

        public GroupModel GroupModel { get; set; } = new();

        public static Expression<Func<Request, RequestModel>> Projection
        {
            get
            {
                return r => new RequestModel
                {
                    Id = r.Id,
                    Action = r.Action,
                    RequesterEmail = r.Requester.Email,
                    RequesterName = $"{r.Requester.LastName}, {r.Requester.FirstName}",
                    GroupModel = new GroupModel
                    {
                        Id = r.Group.Id,
                        DisplayName = r.Group.DisplayName,
                        Name = r.Group.Name,
                        Admins = r.Group.Permissions
                            .Where(p => p.Role.Name == Role.Codes.GroupAdmin)
                            .Select(p => new GroupUserModel
                            {
                                Kerberos = p.User.Kerberos,
                                Name = p.User.Name,
                                Email = p.User.Email
                            }).ToList(),
                    }
                };
            }
        }
    }
}