using System.Linq.Expressions;
using Hippo.Core.Data;
using Hippo.Core.Domain;
using Hippo.Core.Models;

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

        public static IQueryable<Request> CanAccess(this IQueryable<Request> requests, AppDbContext dbContext, string? cluster, string iamId)
        {
            if (string.IsNullOrEmpty(cluster))
            {
                throw new ArgumentNullException(nameof(cluster));
            }

            return requests
                .Where(r =>
                    r.Cluster.Name == cluster
                    && dbContext.Permissions.Any(p =>
                        p.User.Iam == iamId
                        && (
                            // system admin can access any request
                            p.Role.Name == Role.Codes.System
                            || (
                                // remaining roles need to be in the same cluster
                                p.Cluster.Name == cluster
                                && (
                                    // cluster admin can access any request in the cluster
                                    p.Role.Name == Role.Codes.ClusterAdmin
                                    || (
                                        // group admin can access requests for the given group
                                        r.Group != null && dbContext.Groups.Any(g =>
                                            g.Name == r.Group && g.AdminAccounts.Any(a => a.Owner.Iam == iamId)
                                        )
                                    )
                                )
                            )
                        )
                    )
                );
        }

        public static IQueryable<Account> CanAccess(this IQueryable<Account> accounts, AppDbContext dbContext, string? cluster, string iamId)
        {
            return accounts
                .InCluster(cluster)
                .Where(a =>
                    a.Cluster.Name == cluster
                    && dbContext.Permissions.Any(p =>
                        p.User.Iam == iamId
                        && (
                            // system admin can access any account
                            p.Role.Name == Role.Codes.System
                            || (
                                // remaining roles need to be in the same cluster
                                p.Cluster.Name == cluster
                                && (
                                    // cluster admin can access any account in the cluster
                                    p.Role.Name == Role.Codes.ClusterAdmin
                                    || (
                                        // group admin can access requests for the given group
                                        a.AdminOfGroups.Any(g => g.AdminAccounts.Any(a => a.Owner.Iam == iamId))
                                    )
                                )
                            )
                        )
                    )
                );
        }
    }
}