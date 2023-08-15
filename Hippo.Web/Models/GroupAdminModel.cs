using Hippo.Core.Domain;

namespace Hippo.Web.Models
{
    public class GroupAdminModel
    {
        public int PermissionId { get; set; }
        public string Group { get; set; } = "";
        public User User { get; set; } = new ();
    }
}