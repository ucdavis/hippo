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
                AccessCodes.SystemAccess => new[] { RoleCodes.SystemRole },
                AccessCodes.AdminAccess => new[] { RoleCodes.AdminRole },
                _ => throw new ArgumentException($"{nameof(accessCode)} is not a valid {nameof(AccessCodes)} constant")
            };
        }
    }
}
