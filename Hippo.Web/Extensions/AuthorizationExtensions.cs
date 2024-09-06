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
                // System requirement can only be fulfilled by a system user
                AccessCodes.SystemAccess => new[] { Role.Codes.System },
                // ClusterAdmin requirement can be fulfilled by a system user or a cluster admin
                AccessCodes.ClusterAdminAccess => new[] { Role.Codes.System, Role.Codes.ClusterAdmin },
                // FinanceAdmin requirement can be fulfilled by a system user or a finance admin
                AccessCodes.FinancialAdminAccess => new[] { Role.Codes.System, Role.Codes.FinancialAdmin },
                // GroupAdmin requirement can be fulfilled by a system user, cluster admin, or group admin
                AccessCodes.GroupAdminAccess => new[] { Role.Codes.System, Role.Codes.ClusterAdmin, Role.Codes.GroupAdmin },
                _ => throw new ArgumentException($"{nameof(accessCode)} is not a valid {nameof(AccessCodes)} constant")
            };
        }
    }
}
