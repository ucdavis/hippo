
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
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            
            // clear temp data in db
            await _dbContext.TruncateAsync<PuppetGroupPuppetUser>();

            foreach (var cluster in await _dbContext.Clusters
                .Where(c => !string.IsNullOrEmpty(c.Domain))
                .AsNoTracking()
                .ToArrayAsync())
            {
                await SyncAccounts(cluster);
            }

            await transaction.CommitAsync();
        }

        private async Task SyncAccounts(Cluster cluster)
        {
            Log.Information("Syncing accounts for cluster {Cluster}", cluster.Name);

            await RefreshPuppetData(cluster);
            await AddNewUsers(cluster);
            await AddNewGroups(cluster);
            await RemoveOldGroups(cluster);
            await AddNewGroupAccounts(cluster);
            await RemoveOldGroupAccounts(cluster);
            await MakeSponsorsGroupAdmins(cluster);
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

        private async Task AddNewGroups(Cluster cluster)
        {
            // check if any groups need to be created
            var missingGroups = await _dbContext.PuppetGroupsPuppetUsers
                .Where(pgpu => pgpu.ClusterName == cluster.Name)
                // identify missing groups
                .Where(pgpu => !_dbContext.Groups.IgnoreQueryFilters().Any(g => g.Name == pgpu.GroupName && g.ClusterId == cluster.Id))
                .Select(pgpu => pgpu.GroupName)
                .Distinct()
                .Select(groupName => new Group { Name = groupName, DisplayName = groupName, ClusterId = cluster.Id })
                .ToArrayAsync();
            if (missingGroups.Any())
            {
                Log.Information("Adding {Count} missing groups for cluster {Cluster}", missingGroups.Count(), cluster.Name);
                await _dbContext.BulkInsertAsync(missingGroups);
            }

            // check if any groups need to be reactivated
            var reactivateGroups = await _dbContext.Groups
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Where(g => g.ClusterId == cluster.Id && !g.IsActive)
                .Where(g => _dbContext.PuppetGroupsPuppetUsers.Any(pgpu => pgpu.ClusterName == cluster.Name && pgpu.GroupName == g.Name))
                .ToArrayAsync();

            if (reactivateGroups.Any())
            {
                Log.Information("Re-enabling {Count} groups for cluster {Cluster}", reactivateGroups.Length, cluster.Name);
                foreach (var group in reactivateGroups)
                {
                    group.IsActive = true;
                }
                await _dbContext.BulkUpdateAsync(reactivateGroups);
            }
        }

        private async Task RemoveOldGroups(Cluster cluster)
        {
            // check for groups that no longer exist
            var deactivateGroups = await _dbContext.Groups
                .AsNoTracking()
                .Where(g => g.ClusterId == cluster.Id)
                .Where(g => !_dbContext.PuppetGroupsPuppetUsers.Any(pgpu => pgpu.ClusterName == cluster.Name && pgpu.GroupName == g.Name))
                .ToArrayAsync();

            if (deactivateGroups.Any())
            {
                Log.Information("Removing {Count} groups for cluster {Cluster}", deactivateGroups.Length, cluster.Name);
                foreach (var group in deactivateGroups)
                {
                    group.IsActive = false;
                }
                await _dbContext.BulkUpdateAsync(deactivateGroups);

                // deactivate accounts that have no remaining active groups
                var inspectAccounts = await _dbContext.Accounts
                    .AsNoTracking()
                    .Include(a => a.GroupAccounts)
                    .Where(a => a.ClusterId == cluster.Id
                        && deactivateGroups.Select(g => g.Id).Contains(a.GroupAccounts.Select(ga => ga.GroupId).FirstOrDefault()))
                    .ToArrayAsync();

                var deactivateAccounts = new List<Account>();

                foreach (var account in inspectAccounts.Where(a => a.GroupAccounts.Any()))
                {
                    account.IsActive = false;
                    deactivateAccounts.Add(account);
                }

                if (deactivateAccounts.Any())
                {
                    Log.Information("Disabling {Count} accounts for cluster {Cluster}", deactivateAccounts.Count, cluster.Name);
                    await _dbContext.BulkUpdateAsync(deactivateAccounts);
                }
            }
        }

        private async Task RemoveOldGroupAccounts(Cluster cluster)
        {
            // check for group memberships that no longer exist
            var removeGroupsAccounts = await _dbContext.GroupsAccounts
                .AsNoTracking()
                .Where(ga => ga.Group.ClusterId == cluster.Id)
                // identify GroupAccount records that are no longer represented in puppet data
                .Where(ga => !_dbContext.PuppetGroupsPuppetUsers.Any(pgpu => pgpu.ClusterName == cluster.Name && pgpu.UserKerberos == ga.Account.Owner.Kerberos && pgpu.GroupName == ga.Group.Name))
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
                // identify missing GroupAccounts
                .Where(x => !_dbContext.GroupsAccounts.Any(ga => ga.AccountId == x.account.Id && ga.GroupId == x.group.Id))
                .Select(x => new GroupAccount { GroupId = x.group.Id, AccountId = x.account.Id })
                .ToArrayAsync();


            if (addGroupsAccounts.Any())
            {
                Log.Information("Adding {Count} new GroupAccounts for cluster {Cluster}", addGroupsAccounts.Length, cluster.Name);
                await _dbContext.BulkInsertAsync(addGroupsAccounts);
            }
        }

        private async Task AddNewUsers(Cluster cluster)
        {
            // check if any users need to be added
            var addUsers = await _dbContext.PuppetGroupsPuppetUsers
                // only sync users that are members of an existing group
                .Where(pgpu => pgpu.ClusterName == cluster.Name && _dbContext.Groups.Any(g => g.IsActive && g.Name == pgpu.GroupName && g.ClusterId == cluster.Id))
                // identify missing users
                .Where(pgpu => !_dbContext.Users.Any(u => u.Kerberos == pgpu.UserKerberos))
                .Select(pgpu => pgpu.UserKerberos)
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
                Log.Information("Adding {Count} new users and accounts for cluster {Cluster}", newUsers.Count(), cluster.Name);
                await _dbContext.BulkInsertAsync(newUsers, new BulkConfig { SetOutputIdentity = true });

                // create accounts for these new users
                var newAccounts = newUsers.Select(u => new Account { OwnerId = u.Id, ClusterId = cluster.Id, Status = Account.Statuses.Active, Name = $"{u.FirstName} {u.LastName} ({u.Email})" }).ToArray();
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
                // identify missing permissions
                .Where(x => !_dbContext.Permissions.Any(perm => perm.UserId == x.user.UserId && perm.GroupId == x.grp.Id && perm.RoleId == groupAdminRoleId && perm.ClusterId == cluster.Id))
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