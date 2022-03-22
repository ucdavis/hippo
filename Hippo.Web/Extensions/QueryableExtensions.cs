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
    }
}