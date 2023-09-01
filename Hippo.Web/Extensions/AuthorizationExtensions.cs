using Hippo.Core.Domain;
using Hippo.Core.Models;
using Hippo.Web.Handlers;
using Microsoft.AspNetCore.Authorization;

namespace Hippo.Web.Extensions
{
    public static class AuthorizationExtensions
    {
        public static void AddAccessPolicy(this AuthorizationOptions options, string policy)
        {
            options.AddPolicy(policy, builder => builder.Requirements.Add(new VerifyRoleAccess(GetRoles(policy))));
        }

        public static string[] GetRoles(string accessCode)
        {
            return accessCode switch
            {
                // System can access anything
                AccessCodes.SystemAccess => new[] { Role.Codes.System },
                AccessCodes.ClusterAdminAccess => new[] { Role.Codes.ClusterAdmin, Role.Codes.GroupAdmin },
                AccessCodes.GroupAdminAccess => new[] { Role.Codes.GroupAdmin },
                _ => throw new ArgumentException($"{nameof(accessCode)} is not a valid {nameof(AccessCodes)} constant")
            };
        }
    }
}
