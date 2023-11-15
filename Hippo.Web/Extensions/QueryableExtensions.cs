using System.Linq.Expressions;
using Hippo.Core.Data;
using Hippo.Core.Domain;
using Hippo.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Hippo.Web.Extensions
{
    public static class QueryableExtensions
    {
        public static IQueryable<Account> InCluster(this IQueryable<Account> accounts, string? cluster)
        {
            // cluster isn't allowed to be null
            // we could also just use a false condition to return no results
            if (string.IsNullOrEmpty(cluster))
            {
                throw new ArgumentNullException(nameof(cluster));
            }

            return accounts.Where(a => a.Cluster.Name == cluster);
        }

        public static IQueryable<Request> CanAccess(this IQueryable<Request> requests, AppDbContext dbContext, string? cluster, string iamId, bool isClusterOrSystemAdmin)
        {
            if (string.IsNullOrEmpty(cluster))
            {
                throw new ArgumentNullException(nameof(cluster));
            }

            return requests
                .Where(req =>
                    req.Cluster.Name == cluster
                    && (
                        isClusterOrSystemAdmin
                        // group admin can access requests for the given group
                        || (req.Group != null && dbContext.Groups.Any(g =>
                            g.ClusterId == req.ClusterId && g.Name == req.Group && g.AdminAccounts.Any(a => a.Owner.Iam == iamId)
                        ))
                    )
                );
        }

        public static IQueryable<Account> CanAccess(this IQueryable<Account> accounts, string? cluster, string iamId, bool isClusterOrSystemAdmin)
        {
            return accounts
                //.InCluster(cluster)
                .Where(a =>
                    a.Cluster.Name == cluster
                    && (
                        isClusterOrSystemAdmin
                        // group admin can access any account that's a member of their group(s)
                        || 
                        a.MemberOfGroups.Any(g => g.AdminAccounts.Any(a => a.Owner.Iam == iamId))
                    )
                );
        }
    }
}