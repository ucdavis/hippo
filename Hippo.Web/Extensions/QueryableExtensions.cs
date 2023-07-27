using Hippo.Core.Data;
using Hippo.Core.Domain;

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

        public static IQueryable<Account> PendingApproval(this IQueryable<Account> accounts)
        {
            return accounts.Where(a => a.Status == Account.Statuses.PendingApproval);
        }

        public static IQueryable<Account> CanAccess(this IQueryable<Account> accounts, AppDbContext dbContext, string? cluster, string iamId)
        {
            // cluster isn't allowed to be null
            // we could also just use a false condition to return no results
            if (string.IsNullOrEmpty(cluster))
            {
                throw new ArgumentNullException(nameof(cluster));
            }

            return accounts.Where(a =>
                // cluster admin can access any account
                dbContext.Permissions.Any(p =>
                    p.User.Iam == iamId
                    && p.Cluster.Name == cluster
                    && p.Role.Name == Role.Codes.ClusterAdmin)
                ||
                // group admin can access accounts for groups they are in
                a.Group.Permissions.Any(p =>
                    p.User.Iam == iamId
                    && p.Cluster.Name == cluster
                    && p.Role.Name == Role.Codes.GroupAdmin)
            );
        }
    }
}