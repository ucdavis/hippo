using System.Linq.Expressions;
using System.Text.Json;
using Hippo.Core.Domain;

namespace Hippo.Core.Models
{
    public class GroupModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public JsonElement? Data { get; set; }
        // RevokedOn only used for AccountDeactivation report. It's being shoved here because the Account model's
        // MemberOfGroups navigation property skips the GroupMemberAccount model, and it would take significant 
        // refactoring to change that.
        public DateTime? RevokedOn { get; set; } 

        public List<GroupAccountModel> Admins { get; set; } = new();

        public static Expression<Func<Group, GroupModel>> GetProjection(bool isClusterOrSystemAdmin, int currentUserId = 0)
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
                Data = isClusterOrSystemAdmin
                    || g.AdminAccounts.Any(a => a.OwnerId == currentUserId)
                    || g.MemberAccounts.Any(a => a.OwnerId == currentUserId)
                    ? g.Data : null
            };

        }
    }

    public class GroupAccountModel
    {
        public string Kerberos { get; set; } = "";
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
    }
}