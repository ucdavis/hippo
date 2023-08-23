
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
        private readonly Lazy<Task<int>> _groupMemberRoleId;

        public AccountSyncService(IPuppetService puppetService, AppDbContext dbContext, IIdentityService identityService)
        {
            _puppetService = puppetService;
            _dbContext = dbContext;
            _identityService = identityService;
            // GroupMember RoleId will be needed in a couple places, so start a task for it now
            _groupMemberRoleId = new Lazy<Task<int>>(async () => (await _dbContext.Roles.SingleAsync(r => r.Name == Role.Codes.GroupMember)).Id);
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
            await AddNewGroupMemberships(cluster);
            await RemoveOldGroupMemberships(cluster);
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

        private async Task RemoveOldGroupMemberships(Cluster cluster)
        {
            var groupMemberRoleId = await _groupMemberRoleId.Value;

            // check for group memberships that no longer exist
            var removePerms = await _dbContext.Permissions
                .AsNoTracking()
                .Where(p => p.ClusterId == cluster.Id && p.RoleId == groupMemberRoleId)
                // identify GroupMember perms that are no longer represented in puppet data (Left Join is via GroupJoin/SelectMany)
                .GroupJoin(_dbContext.PuppetGroupsPuppetUsers.Where(pgpu => pgpu.ClusterName == cluster.Name),
                    permission => new { kerb = permission.User.Kerberos, GroupName = permission.Group.Name },
                    pgpu => new { kerb = pgpu.UserKerberos, pgpu.GroupName },
                    (permission, pgpus) => new { permission, pgpus })
                .SelectMany(x => x.pgpus.DefaultIfEmpty(), (x, pgpu) => new { x.permission, pgpu })
                .Where(x => x.pgpu == null)
                .Select(x => x.permission)
                .ToArrayAsync();

            if (removePerms.Any())
            {
                // disable accounts before removing permissions
                var disableAccounts = await _dbContext.Accounts
                    .AsNoTracking()
                    .Where(a => a.ClusterId == cluster.Id
                        && removePerms.Select(p => p.UserId).Contains(a.OwnerId)
                        && removePerms.Select(p => p.GroupId).Contains(a.GroupId))
                    .ToArrayAsync();
                
                if (disableAccounts.Any())
                {
                    foreach (var account in disableAccounts)
                    {
                        account.IsActive = false;
                    }
                    Log.Information("Disabling {Count} accounts for cluster {Cluster}", disableAccounts.Length, cluster.Name);
                    await _dbContext.BulkUpdateAsync(disableAccounts);
                }
                
                Log.Information("Removing {Count} GroupMember permissions for cluster {Cluster}", removePerms.Length, cluster.Name);
                await _dbContext.BulkDeleteAsync(removePerms);
            }
        }

        private async Task AddNewGroupMemberships(Cluster cluster)
        {
            var groupMemberRoleId = await _groupMemberRoleId.Value;

            // check for new group memberships
            var addPerms = await _dbContext.PuppetGroupsPuppetUsers
                // only sync users that are members of an existing group
                .Where(pgpu => pgpu.ClusterName == cluster.Name && _dbContext.Groups.Any(group => group.IsActive && group.Name == pgpu.GroupName && group.ClusterId == cluster.Id))
                .Join(_dbContext.Users, pgpu => pgpu.UserKerberos, user => user.Kerberos, (pgpu, user) => new { pgpu, user })
                .Join(_dbContext.Groups, x => x.pgpu.GroupName, group => group.Name, (x, group) => new { x.pgpu, x.user, group })
                // identify perms that are not yet in the db (Left Join is via GroupJoin/SelectMany)
                .GroupJoin(_dbContext.Permissions,
                    x => new { UserId = x.user.Id, GroupId = (int?)x.group.Id, RoleId = groupMemberRoleId, ClusterId = (int?)cluster.Id },
                    p => new { p.UserId, p.GroupId, p.RoleId, p.ClusterId },
                    (x, perms) => new { x.pgpu, x.user, g = x.group, perms })
                .SelectMany(x => x.perms.DefaultIfEmpty(), (x, permission) => new { x.pgpu, x.user, x.g, permission })
                .Where(x => x.permission == null)
                .Select(x => new Permission { ClusterId = cluster.Id, GroupId = x.g.Id, RoleId = groupMemberRoleId, UserId = x.user.Id })
                .ToArrayAsync();

            if (addPerms.Any())
            {
                Log.Information("Adding {Count} new GroupMembership permissions for cluster {Cluster}", addPerms.Length, cluster.Name);
                await _dbContext.BulkInsertAsync(addPerms);
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
            }

        }

        // TODO: delete after Sponsor to Group migration is complete
        private async Task MakeSponsorsGroupAdmins(Cluster cluster)
        {
            var groupMemberRoleId = await _groupMemberRoleId.Value;

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
            var missingPerms = await usersThatShouldBeGroupAdmins
                .Join(_dbContext.Groups.Where(g => g.IsActive), 
                    u => new { u.GroupName, cluster.Id }, 
                    g => new { GroupName = g.Name, Id = g.ClusterId }, 
                    (user, grp) => new { user, grp })
                .GroupJoin(_dbContext.Permissions, 
                    x => new { x.user.UserId, GroupId = (int?)x.grp.Id, RoleId = groupMemberRoleId, ClusterId = (int?)cluster.Id }, 
                    perm => new { perm.UserId, perm.GroupId, perm.RoleId, perm.ClusterId }, 
                    (x, perms) => new { x.user, x.grp, perms })
                .SelectMany(x => x.perms.DefaultIfEmpty(), (x, perm) => new { x.user, x.grp, perm })
                .Where(x => x.perm == null)
                .Select(x => new Permission { ClusterId = cluster.Id, GroupId = x.grp.Id, RoleId = groupMemberRoleId, UserId = x.user.UserId })
                .ToArrayAsync();

            if (missingPerms.Any())
            {
                Log.Information("Adding {Count} missing GroupAdmin permissions for cluster {Cluster}", missingPerms.Count(), cluster.Name);
                await _dbContext.BulkInsertAsync(missingPerms);
            }
        }
    }
}