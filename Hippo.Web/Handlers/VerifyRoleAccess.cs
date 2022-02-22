using Microsoft.AspNetCore.Authorization;

namespace Hippo.Web.Handlers
{
    public class VerifyRoleAccess : IAuthorizationRequirement
    {
        public readonly string[] RoleStrings;

        public VerifyRoleAccess(params string[] roleStrings)
        {
            RoleStrings = roleStrings;
        }
    }
}
