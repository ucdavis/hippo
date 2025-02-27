using Hippo.Core.Models;

namespace Hippo.Web.Models
{

    public class GroupMembersModel
    {
        public GroupModel Group { get; set; } = new();
        public List<AccountModel> Accounts { get; set; } = new();
        public List<string> KerberosPendingRemoval { get; set; } = new();
    }
}