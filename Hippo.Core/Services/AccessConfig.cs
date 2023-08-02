using Hippo.Core.Domain;
using Hippo.Core.Models;


namespace Hippo.Core.Services
{
    public class AccessConfig
    {
        public static string[] GetRoles(string accessCode)
        {
            return accessCode switch
            {
                // System can access anything
                AccessCodes.SystemAccess => new[] { Role.Codes.System },
                AccessCodes.ClusterAdminAccess => new[] { Role.Codes.ClusterAdmin, Role.Codes.GroupAdmin },
                AccessCodes.GroupAdminAccess => new[] { Role.Codes.GroupAdmin },
                AccessCodes.GroupAccess => new[] { Role.Codes.GroupMember },
                _ => throw new ArgumentException($"{nameof(accessCode)} is not a valid {nameof(AccessCodes)} constant")
            };
        }
    }
}
