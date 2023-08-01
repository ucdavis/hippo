
using EFCore.BulkExtensions;
using Hippo.Core.Data;
using Hippo.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
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

            await MakeSponsorsGroupAdmins(cluster);
        }

        // TODO: delete after Sponsor to Group migration is complete
        private async Task MakeSponsorsGroupAdmins(Cluster cluster)
        {
            // find account sponsors in this cluster
            var sponsors = _dbContext.Accounts
                .Where(a => a.ClusterId == cluster.Id && a.SponsorId != null)
                .Select(a => new { a.Sponsor.Owner.Id, a.Sponsor.Owner.Kerberos });

            var usersThatShouldBeGroupAdmins =
                from sponser in _dbContext.Accounts
                where sponser.Cluster.Name == cluster.Name && _dbContext.Accounts.Any(a => a.SponsorId == sponser.Id)
                join pgpu in _dbContext.PuppetGroupsPuppetUsers on sponser.Owner.Kerberos equals pgpu.UserKerberos
                select new { UserId = sponser.Owner.Id, pgpu.GroupName };

            // ensure that all groups exist in the db
            var missingGroups = await usersThatShouldBeGroupAdmins
                .Where(u => !_dbContext.Groups.Any(g => g.Name == u.GroupName))
                .Select(u => u.GroupName)
                .Distinct()
                .Select(groupName => new Group { Name = groupName, DisplayName = groupName, ClusterId = cluster.Id })
                .ToArrayAsync();

            if (missingGroups.Any())
            {
                Log.Information("Adding {Count} missing groups for cluster {Cluster}", missingGroups.Count(), cluster.Name);
                await _dbContext.BulkInsertAsync(missingGroups);
            }

            var missingPerms =
                from user in usersThatShouldBeGroupAdmins
                join grp in _dbContext.Groups on user.GroupName equals grp.Name
                join perm in _dbContext.Permissions on
                    // ugh, multiple join conditions in linq are ugly
                    new
                    {
                        GroupId = (int?)grp.Id,
                        RoleName = Role.Codes.GroupAdmin,
                        ClusterId = (int?)grp.ClusterId,
                        user.UserId
                    }
                    equals new
                    {
                        perm.GroupId,
                        RoleName = perm.Role.Name,
                        perm.ClusterId,
                        perm.UserId
                    }
                // make it a left join to identify needed perms...
                into missingPermissions
                from missingPerm in missingPermissions.DefaultIfEmpty()
                where missingPerm == null
                select new Permission
                {
                    UserId = user.UserId,
                    GroupId = grp.Id,
                    RoleId = _dbContext.Roles.Single(r => r.Name == Role.Codes.GroupAdmin).Id,
                    ClusterId = grp.ClusterId
                };

            if (missingPerms.Any())
            {
                Log.Information("Adding {Count} missing group admin permissions for cluster {Cluster}", missingPerms.Count(), cluster.Name);
                await _dbContext.BulkInsertAsync(missingPerms);
            }
        }
    }
}