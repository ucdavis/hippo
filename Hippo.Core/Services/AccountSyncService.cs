
using EFCore.BulkExtensions;
using Hippo.Core.Data;
using Hippo.Core.Domain;
using Hippo.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Hippo.Core.Services
{
    public interface IAccountSyncService
    {
        Task Run();
    }

    public class AccountSyncService : IAccountSyncService
    {
        private readonly IPuppetService _puppetService;
        private readonly AppDbContext _dbContext;
        private readonly IIdentityService _identityService;

        public AccountSyncService(IPuppetService puppetService, AppDbContext dbContext, IIdentityService identityService)
        {
            _puppetService = puppetService;
            _dbContext = dbContext;
            _identityService = identityService;
        }

        public async Task Run()
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            foreach (var cluster in await _dbContext.Clusters
                .Where(c => !string.IsNullOrEmpty(c.Domain))
                .AsNoTracking()
                .ToArrayAsync())
            {
                await Run(cluster);
            }

            await transaction.CommitAsync();
        }

        private async Task Run(Cluster cluster)
        {
            Log.Information("Syncing accounts for cluster {Cluster}", cluster.Name);

            var puppetData = await _puppetService.GetPuppetData(cluster.Name, cluster.Domain);

            var mapKerbsToUserIds = await _dbContext.Users
                .Where(u => _dbContext.TempKerberos.Any(tg => tg.Kerberos == u.Kerberos))
                .Select(u => new { u.Id, u.Kerberos })
                .ToDictionaryAsync(k => k.Kerberos, v => v.Id);

            // setup desired state of groups and accounts
            var now = DateTime.UtcNow;
            var desiredAccounts = puppetData.Users
                .Select(u => new Account
                {
                    Name = u.Name,
                    Email = u.Email,
                    Kerberos = u.Kerberos,
                    OwnerId = mapKerbsToUserIds.ContainsKey(u.Kerberos) ? mapKerbsToUserIds[u.Kerberos] : null,
                    ClusterId = cluster.Id,
                    CreatedOn = now,
                    UpdatedOn = now,
                })
                .ToArray();
            var desiredGroups = puppetData.GroupsWithSponsors
                .Select(groupName => new Group { Name = groupName, DisplayName = groupName, ClusterId = cluster.Id })
                .ToList();

            // clear temp table data
            await _dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE [TempGroups]");
            await _dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE [TempKerberos]");

            // determine what groups need to be deleted
            await _dbContext.BulkInsertAsync(desiredGroups.Select(g => new TempGroup { Group = g.Name }));
            var deleteGroups = await _dbContext.Groups
                .Where(g => g.ClusterId == cluster.Id)
                .Where(g => !_dbContext.TempGroups.Any(tg => tg.Group == g.Name))
                .ToArrayAsync();

            // determine what accounts need to be deleted
            await _dbContext.BulkInsertAsync(puppetData.Users.Select(u => new TempKerberos { Kerberos = u.Kerberos }));
            var deleteAccounts = await _dbContext.Accounts
                .Where(a => a.ClusterId == cluster.Id)
                .Where(a => !_dbContext.TempKerberos.Any(tg => tg.Kerberos == a.Kerberos))
                .ToArrayAsync();

            // insert/update groups and accounts
            Log.Information("Inserting/Updating {Count} groups and {Count} accounts for cluster {Cluster}", desiredGroups.Count, desiredAccounts.Length, cluster.Name);
            await _dbContext.BulkInsertOrUpdateAsync(desiredGroups, new BulkConfig
            {
                PropertiesToExcludeOnUpdate = new List<string> { nameof(Group.DisplayName) },
                UpdateByProperties = new List<string> { nameof(Group.ClusterId), nameof(Group.Name) }
            });
            await _dbContext.BulkInsertOrUpdateAsync(desiredAccounts, new BulkConfig
            {
                PropertiesToExcludeOnUpdate = new List<string> { nameof(Account.SshKey), nameof(Account.CreatedOn) },
                UpdateByProperties = new List<string> { nameof(Account.ClusterId), nameof(Account.Kerberos) }
            });

            // get ids for groups and accounts
            // NOTE: this must occur after the accounts have been inserted/updated to guarantee presence of ids
            var mapGroupNameToId = await _dbContext.Groups
                .Where(g => g.ClusterId == cluster.Id)
                .ToDictionaryAsync(g => g.Name, g => g.Id);

            var mapKerberosToAccountId = await _dbContext.Accounts
                .Where(a => a.ClusterId == cluster.Id && a.Kerberos != null)
                .ToDictionaryAsync(a => a.Kerberos, a => a.Id);

            // setup desired state of group memberships
            var desiredGroupAccounts = puppetData.Users
                .SelectMany(u => u.Groups.Select(g => new GroupMemberAccount
                {
                    GroupId = mapGroupNameToId[g],
                    AccountId = mapKerberosToAccountId[u.Kerberos]
                }));
            var desiredGroupAdminAccounts = puppetData.Users
                .SelectMany(u => u.SponsorForGroups.Select(g => new GroupAdminAccount
                {
                    GroupId = mapGroupNameToId[g],
                    AccountId = mapKerberosToAccountId[u.Kerberos]
                }));

            // insert/update group memberships
            Log.Information("Inserting/Updating {Count} group memberships and {Count} group admin memberships for cluster {Cluster}", desiredGroupAccounts.Count(), desiredGroupAdminAccounts.Count(), cluster.Name);
            await _dbContext.BulkInsertOrUpdateAsync(desiredGroupAccounts);
            await _dbContext.BulkInsertOrUpdateAsync(desiredGroupAdminAccounts);

            // determine what group memberships to remove
            var deleteGroupAccounts = await _dbContext.GroupMemberAccount
                .Where(ga => ga.Group.ClusterId == cluster.Id)
                .Where(ga => !_dbContext.TempKerberos.Any(tg => tg.Kerberos == ga.Account.Kerberos)
                    || !_dbContext.TempGroups.Any(tg => tg.Group == ga.Group.Name))
                .ToArrayAsync();
            var deleteGroupAdminAccounts = await _dbContext.GroupAdminAccount
                .Where(ga => ga.Group.ClusterId == cluster.Id)
                .Where(ga => !_dbContext.TempKerberos.Any(tg => tg.Kerberos == ga.Account.Kerberos)
                    || !_dbContext.TempGroups.Any(tg => tg.Group == ga.Group.Name))
                .ToArrayAsync();

            // delete group memberships
            Log.Information("Deleting {Count} group memberships and {Count} group admin memberships for cluster {Cluster}", deleteGroupAccounts.Length, deleteGroupAdminAccounts.Length, cluster.Name);
            await _dbContext.BulkDeleteAsync(deleteGroupAccounts);
            await _dbContext.BulkDeleteAsync(deleteGroupAdminAccounts);

            // delete groups and accounts
            Log.Information("Deleting {Count} groups and {Count} accounts for cluster {Cluster}", deleteGroups.Length, deleteAccounts.Length, cluster.Name);
            await _dbContext.BulkDeleteAsync(deleteGroups, new BulkConfig
            {
                UpdateByProperties = new List<string> { nameof(Group.ClusterId), nameof(Group.Name) }
            });
            await _dbContext.BulkDeleteAsync(deleteAccounts, new BulkConfig
            {
                UpdateByProperties = new List<string> { nameof(Account.ClusterId), nameof(Account.Kerberos) }
            });

            // clear temp table data
            await _dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE [TempGroups]");
            await _dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE [TempKerberos]");

            // find requests that need to be updated
            var requests = await _dbContext.Requests
                .Where(r => r.ClusterId == cluster.Id && r.Status == Request.Statuses.Processing)
                .LeftJoin(_dbContext.Accounts,
                    r => new { r.ClusterId, r.Requester.Kerberos },
                    a => new { a.ClusterId, a.Kerberos })
                .Where(r => r.Right != null && r.Right.MemberOfGroups.Any(g => g.Name == r.Left.Group))
                .Select(r => r.Left)
                .ToArrayAsync();

            if (requests.Any())
            {
                Log.Information("Updating {Count} requests to completed for cluster {Cluster}", requests.Length, cluster.Name);
                var updatedOn = DateTime.UtcNow;
                foreach (var request in requests)
                {
                    request.Status = Request.Statuses.Completed;
                    request.UpdatedOn = updatedOn;
                }
                await _dbContext.Requests.BatchUpdateAsync(requests, new List<string> { nameof(Request.Status), nameof(Request.UpdatedOn) });
            }
        }

    }
}