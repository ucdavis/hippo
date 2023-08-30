
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
        private readonly IIdentityService _identityService;

        public AccountSyncService(IPuppetService puppetService, AppDbContext dbContext, IIdentityService identityService)
        {
            _puppetService = puppetService;
            _dbContext = dbContext;
            _identityService = identityService;
        }

        public async Task SyncAccounts()
        {
            // clear temp data in db
            await _dbContext.TruncateAsync<PuppetGroupPuppetUser>();

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

            await RefreshPuppetData(cluster);
            await MakeSponsorsGroupAdmins(cluster);
            await AddNewUsers(cluster);
            await AddNewGroupAccounts(cluster);
            await RemoveOldGroupAccounts(cluster);
        }

        private async Task RefreshPuppetData(Cluster cluster)
        {
            // Get groups and their users from puppet for given domain
            var puppetGroupsUsers = (await _puppetService.GetPuppetGroupsUsers(cluster.Name, cluster.Domain)).ToArray();

            // refresh temp data in db
            await _dbContext.BulkInsertAsync(puppetGroupsUsers);

            Log.Information("Found {Users} users and {Groups} groups for cluster {Cluster}",
                puppetGroupsUsers.Select(x => x.UserKerberos).Distinct().Count(),
                puppetGroupsUsers.Select(x => x.GroupName).Distinct().Count(),
                cluster.Name);
        }

        private async Task RemoveOldGroupAccounts(Cluster cluster)
        {
            // check for group memberships that no longer exist
            var removeGroupsAccounts = await _dbContext.GroupsAccounts
                .AsNoTracking()
                .Where(ga => ga.Group.ClusterId == cluster.Id)
                // identify GroupAccount records that are no longer represented in puppet data (Left Join is via GroupJoin/SelectMany)
                .GroupJoin(_dbContext.PuppetGroupsPuppetUsers.Where(pgpu => pgpu.ClusterName == cluster.Name),
                    ga => new { kerb = ga.Account.Owner.Kerberos, GroupName = ga.Group.Name },
                    pgpu => new { kerb = pgpu.UserKerberos, pgpu.GroupName },
                    (ga, pgpus) => new { ga, pgpus })
                .SelectMany(x => x.pgpus.DefaultIfEmpty(), (x, pgpu) => new { x.ga, pgpu })
                .Where(x => x.pgpu == null)
                .Select(x => x.ga)
                .ToArrayAsync();

            if (removeGroupsAccounts.Any())
            {
                Log.Information("Removing {Count} GroupAccounts for cluster {Cluster}", removeGroupsAccounts.Length, cluster.Name);
                await _dbContext.BulkDeleteAsync(removeGroupsAccounts);

                // disable accounts that have no remaining active groups
                var inspectAccounts = await _dbContext.Accounts
                    .AsNoTracking()
                    .Include(a => a.GroupAccounts)
                    .Where(a => a.ClusterId == cluster.Id
                        && removeGroupsAccounts.Select(ga => ga.AccountId).Contains(a.Id))
                    .ToArrayAsync();

                var inactivateAccounts = new List<Account>();
                
                foreach (var account in inspectAccounts.Where(a => a.GroupAccounts.Any()))
                {
                    account.IsActive = false;
                    inactivateAccounts.Add(account);
                }
                
                if (inactivateAccounts.Any())
                {
                    Log.Information("Disabling {Count} accounts for cluster {Cluster}", inactivateAccounts.Count, cluster.Name);
                    await _dbContext.BulkUpdateAsync(inactivateAccounts);
                }
                
            }
        }

        private async Task AddNewGroupAccounts(Cluster cluster)
        {
            // check for new group memberships
            var addGroupsAccounts = await _dbContext.PuppetGroupsPuppetUsers
                // only sync accounts that are members of an existing group
                .Where(pgpu => pgpu.ClusterName == cluster.Name && _dbContext.Groups.Any(group => group.IsActive && group.Name == pgpu.GroupName && group.ClusterId == cluster.Id))
                .Join(_dbContext.Accounts, pgpu => pgpu.UserKerberos, account => account.Owner.Kerberos, (pgpu, account) => new { pgpu, account })
                .Join(_dbContext.Groups, x => x.pgpu.GroupName, group => group.Name, (x, group) => new { x.pgpu, x.account, group })
                // identify missing GroupAccounts (Left Join is via GroupJoin/SelectMany)
                .GroupJoin(_dbContext.GroupsAccounts,
                    x => new { AccountId = x.account.Id, GroupId = x.group.Id },
                    ga => new { ga.AccountId, ga.GroupId },
                    (x, groupAccounts) => new { x.pgpu, x.account, g = x.group, groupAccounts })
                .SelectMany(x => x.groupAccounts.DefaultIfEmpty(), (x, groupAccount) => new { x.pgpu, x.account, x.g, groupAccount })
                .Where(x => x.groupAccount == null)
                .Select(x => new GroupAccount { GroupId = x.g.Id, AccountId = x.account.Id })
                .ToArrayAsync();

            if (addGroupsAccounts.Any())
            {
                Log.Information("Adding {Count} new GroupMembership permissions for cluster {Cluster}", addGroupsAccounts.Length, cluster.Name);
                await _dbContext.BulkInsertAsync(addGroupsAccounts);
            }
        }

        private async Task AddNewUsers(Cluster cluster)
        {
            // check if any users need to be added
            var addUsers = await _dbContext.PuppetGroupsPuppetUsers
                // only sync users that are members of an existing group
                .Where(pgpu => pgpu.ClusterName == cluster.Name && _dbContext.Groups.Any(g => g.IsActive && g.Name == pgpu.GroupName && g.ClusterId == cluster.Id))
                // identify users that are not yet in the db (Left Join is via GroupJoin/SelectMany)
                .GroupJoin(_dbContext.Users, pgpu => pgpu.UserKerberos, u => u.Kerberos, (pgpu, users) => new { pgpu, users })
                .SelectMany(x => x.users.DefaultIfEmpty(), (x, u) => new { x.pgpu, u })
                .Where(x => x.u == null)
                .Select(x => x.pgpu.UserKerberos)
                .Distinct()
                .ToArrayAsync();

            var newUsers = new List<User>();
            var notFoundUsers = new List<string>();
            foreach (var kerb in addUsers)
            {
                var user = await _identityService.GetByKerberos(kerb);
                if (user != null)
                {
                    newUsers.Add(user);
                }
                else
                {
                    notFoundUsers.Add(kerb);
                }
            }

            if (notFoundUsers.Any())
            {
                Log.Warning("Users not found with identity service. For Kerberos: ({kerberos})", string.Join(", ", notFoundUsers));
            }

            if (newUsers.Any())
            {
                Log.Information("Adding {Count} new users for cluster {Cluster}", newUsers.Count(), cluster.Name);
                await _dbContext.BulkInsertAsync(newUsers);

                // create accounts for these new users
                var newAccounts = newUsers.Select(u => new Account { OwnerId = u.Id, ClusterId = cluster.Id, Status = Account.Statuses.Active }).ToArray();
                await _dbContext.BulkInsertAsync(newAccounts);

                // link new accounts to their groups
                var newGroupAccounts = await _dbContext.PuppetGroupsPuppetUsers
                    .Where(pgpu => pgpu.ClusterName == cluster.Name && newUsers.Select(u => u.Kerberos).Contains(pgpu.UserKerberos))
                    .Join(_dbContext.Accounts, pgpu => pgpu.UserKerberos, account => account.Owner.Kerberos, (pgpu, account) => new { pgpu, account })
                    .Join(_dbContext.Groups, x => x.pgpu.GroupName, group => group.Name, (x, group) => new { x.pgpu, x.account, group })
                    .Select(x => new GroupAccount { AccountId = x.account.Id, GroupId = x.group.Id })
                    .ToArrayAsync();
                await _dbContext.BulkInsertAsync(newGroupAccounts);
            }

        }

        // TODO: delete after Sponsor to Group migration is complete
        private async Task MakeSponsorsGroupAdmins(Cluster cluster)
        {
            // get all users that are sponsering other users in this cluster
            var usersThatShouldBeGroupAdmins = _dbContext.Accounts
                .Where(sponsor => sponsor.Cluster.Name == cluster.Name && _dbContext.Accounts.Any(a2 => a2.SponsorId == sponsor.Id))
                .Join(_dbContext.PuppetGroupsPuppetUsers.Where(pgpu => pgpu.ClusterName == cluster.Name), 
                    sponsor => sponsor.Owner.Kerberos, 
                    pgpu => pgpu.UserKerberos, 
                    (sponsor, pgpu) => new { sponsor, pgpu })
                .Select(x => new { UserId = x.sponsor.Owner.Id, x.pgpu.GroupName });

            // check if any groups need to be created
            var missingGroups = await usersThatShouldBeGroupAdmins
                .Where(u => !_dbContext.Groups.Any(g => g.IsActive && g.Name == u.GroupName && g.ClusterId == cluster.Id))
                .Select(u => u.GroupName)
                .Distinct()
                .Select(groupName => new Group { Name = groupName, DisplayName = groupName, ClusterId = cluster.Id })
                .ToArrayAsync();
            if (missingGroups.Any())
            {
                Log.Information("Adding {Count} missing groups for cluster {Cluster}", missingGroups.Count(), cluster.Name);
                await _dbContext.BulkInsertAsync(missingGroups);
            }

            // check if any users need to be added to groups
            var groupAdminRoleId = (await _dbContext.Roles.SingleAsync(r => r.Name == Role.Codes.GroupAdmin)).Id;         
            var missingPerms = await usersThatShouldBeGroupAdmins
                .Join(_dbContext.Groups.Where(g => g.IsActive), 
                    u => new { u.GroupName, cluster.Id }, 
                    g => new { GroupName = g.Name, Id = g.ClusterId }, 
                    (user, grp) => new { user, grp })
                .GroupJoin(_dbContext.Permissions, 
                    x => new { x.user.UserId, GroupId = (int?)x.grp.Id, RoleId = groupAdminRoleId, ClusterId = (int?)cluster.Id }, 
                    perm => new { perm.UserId, perm.GroupId, perm.RoleId, perm.ClusterId }, 
                    (x, perms) => new { x.user, x.grp, perms })
                .SelectMany(x => x.perms.DefaultIfEmpty(), (x, perm) => new { x.user, x.grp, perm })
                .Where(x => x.perm == null)
                .Select(x => new Permission { ClusterId = cluster.Id, GroupId = x.grp.Id, RoleId = groupAdminRoleId, UserId = x.user.UserId })
                .ToArrayAsync();

            if (missingPerms.Any())
            {
                Log.Information("Adding {Count} missing GroupAdmin permissions for cluster {Cluster}", missingPerms.Count(), cluster.Name);
                await _dbContext.BulkInsertAsync(missingPerms);
            }
        }
    }
}