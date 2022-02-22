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
                // FieldManager can access anything restricted to FieldManager, Supervisor, Worker or PI roles
                AccessCodes.AdminAccess => new[] { RoleCodes.AdminRole },
                // Supervisor can access anything restricted to Supervisor, Worker or PI roles
                _ => throw new ArgumentException($"{nameof(accessCode)} is not a valid {nameof(AccessCodes)} constant")
            };
        }
    }
}
