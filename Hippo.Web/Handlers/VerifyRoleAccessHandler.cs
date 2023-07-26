using Hippo.Core.Data;
using Hippo.Core.Domain;
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

            if (await _dbContext.Permissions.AnyAsync(p
                => p.User.Iam == userIamId
                && p.Role.Name == Role.Codes.System))
            {
                context.Succeed(requirement);
                return;
            }

            // remaining roles require a cluster
            var clusterName = _httpContext?.HttpContext?.GetRouteValue("cluster") as string;
            if (string.IsNullOrWhiteSpace(clusterName))
            {
                return;
            }

            if (requirement.RoleStrings.Contains(Role.Codes.ClusterAdmin))
            {
                if (await _dbContext.Permissions.AnyAsync(p
                    => p.User.Iam == userIamId
                    && p.Role.Name == Role.Codes.ClusterAdmin
                    && p.Cluster.Name == clusterName))
                {
                    context.Succeed(requirement);
                    return;
                }
            }

            // remaining roles involve an optional route value (Group, GroupAdmin)
            var groupName = _httpContext?.HttpContext?.GetRouteValue("group") as string;
            
            if (string.IsNullOrWhiteSpace(groupName))
            {
                // if no group is provided, just ensure there is at least one permission for the given role
                // and let group-specific filtering be performed by the controller action
                if (await _dbContext.Permissions.AnyAsync(p
                => p.User.Iam == userIamId
                && requirement.RoleStrings.Contains(p.Role.Name)
                && p.Cluster.Name == clusterName))
                {
                    context.Succeed(requirement);
                }
                return;
            }

            if (await _dbContext.Permissions.AnyAsync(p
                => p.User.Iam == userIamId
                && requirement.RoleStrings.Contains(p.Role.Name)
                && p.Cluster.Name == clusterName
                && p.Group.Name == groupName))
            {
                context.Succeed(requirement);
                return;
            }

        }
    }
}
