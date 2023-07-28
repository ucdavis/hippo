
using Hippo.Core.Data;
using Hippo.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Hippo.Core.Services
{
    public interface IAccountSyncService
    {
        Task SyncAccounts();
    }

    public class AccountSyncService : IAccountSyncService
    {
        private readonly IPuppetService _puppetService;
        private readonly AppDbContext _dbContext;

        public AccountSyncService(IPuppetService puppetService, AppDbContext dbContext)
        {
            _puppetService = puppetService;
            _dbContext = dbContext;
        }

        public async Task SyncAccounts()
        {
            foreach (var cluster in await _dbContext.Clusters
                .Where(c => !string.IsNullOrEmpty(c.Domain))
                .AsNoTracking()
                .ToArrayAsync())
            {
                await SyncAccounts(cluster);
            }
        }

        private async Task SyncAccounts(Cluster cluster)
        {
            Log.Information("Syncing accounts for cluster {Cluster}", cluster.Name);

            var puppetGroups = await _puppetService.GetPuppetGroups(cluster.Domain);

            Log.Information("Found {Users} users and {Clusters} groups for cluster {Cluster}",
                puppetGroups.SelectMany(g => g.Users).Distinct().Count(),
                puppetGroups.Count(),
                cluster.Name);
        }
    }
}