using Hippo.Core.Data;
using Hippo.Core.Models;
using Hippo.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Hippo.Web.Handlers
{
    public class VerifyRoleAccessHandler : AuthorizationHandler<VerifyRoleAccess>
    {
        private readonly AppDbContext _dbContext;

        private readonly IHttpContextAccessor _httpContext;

        public VerifyRoleAccessHandler(AppDbContext dbContext, IHttpContextAccessor httpContext)
        {
            _dbContext = dbContext;
            _httpContext = httpContext;
        }
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, VerifyRoleAccess requirement)
        {
            var userIamId = context.User.Claims.SingleOrDefault(c => c.Type == UserService.IamIdClaimType)?.Value;
            var kerbId = context.User.Claims.SingleOrDefault(a => a.Type == ClaimTypes.NameIdentifier)?.Value;


            if (string.IsNullOrWhiteSpace(userIamId))
            {
                return;
            }

            var systemUsers = new[] { "jsylvest", "postit", "cydoval", "sweber" }; //TODO: Change this to use the user.IsAdmin?
            if (systemUsers.Contains(kerbId))
            {
                context.Succeed(requirement);
                return;
            }

            if (requirement.RoleStrings.Contains(RoleCodes.AdminRole))
            {
                var clusterName = _httpContext?.HttpContext?.GetRouteValue("cluster") as string;
                if (string.IsNullOrWhiteSpace(clusterName))
                {
                    return;
                }
                if (await _dbContext.Accounts.AnyAsync(a => a.Cluster.Name == clusterName && a.IsAdmin && a.Owner.Iam == userIamId))
                {
                    context.Succeed(requirement);
                    return;
                }
            }
        }
    }
}
