
using EFCore.BulkExtensions;
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

            // Get groups and their users from puppet for given domain
            var puppetGroups = (await _puppetService.GetPuppetGroups(cluster.Domain)).ToArray();
            var puppetUsers = puppetGroups.SelectMany(g => g.Users).Distinct().ToArray();
            var puppetGroupsUsers = puppetGroups.SelectMany(g => g.Users.Select(u => 
                new PuppetGroupPuppetUser
                {
                    UserKerberos = u.Kerberos,
                    GroupName = g.Name 
                })).ToArray();

            // refresh temp data in db
            await _dbContext.TruncateAsync<PuppetGroupPuppetUser>();
            // Can't truncate the following two because of foreign key constraints
            await _dbContext.PuppetGroups.BatchDeleteAsync();
            await _dbContext.PuppetUsers.BatchDeleteAsync();
            await _dbContext.BulkInsertAsync(puppetGroups);
            await _dbContext.BulkInsertAsync(puppetUsers);
            await _dbContext.BulkInsertAsync(puppetGroupsUsers);

            Log.Information("Found {Users} users and {Clusters} groups for cluster {Cluster}",
                puppetUsers.Length,
                puppetGroups.Length,
                cluster.Name);
        }
    }
}