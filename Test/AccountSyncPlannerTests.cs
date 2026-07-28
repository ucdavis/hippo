using System;
using System.Collections.Generic;
using System.Linq;
using Hippo.Core.Domain;
using Hippo.Core.Services;
using Shouldly;
using Xunit;

namespace Test
{
    public class AccountSyncPlannerTests
    {
        [Fact]
        public void GetDesiredGroupMemberAccounts_CreatesMembershipsForUserGroups()
        {
            var puppetDataByClusterId = new Dictionary<int, PuppetData>
            {
                [1] = new()
                {
                    Users =
                    {
                        new PuppetUser
                        {
                            Kerberos = "kerb1",
                            Groups = new[] { "group-a", "group-b" }
                        },
                        new PuppetUser
                        {
                            Kerberos = "not-yet-in-db",
                            Groups = new[] { "group-a" }
                        }
                    }
                }
            };
            var groupIds = new Dictionary<(int ClusterId, string GroupName), int>
            {
                [(1, "group-a")] = 10,
                [(1, "group-b")] = 11
            };
            var accountIds = new Dictionary<(int ClusterId, string Kerberos), int>
            {
                [(1, "kerb1")] = 100
            };

            var desired = AccountSyncPlanner.GetDesiredGroupMemberAccounts(
                puppetDataByClusterId,
                groupIds,
                accountIds);

            desired.Select(gma => (gma.GroupId, gma.AccountId, gma.RevokedOn))
                .ShouldBe(new[]
                {
                    (10, 100, (DateTime?)null),
                    (11, 100, (DateTime?)null)
                }, ignoreOrder: true);
        }

        [Fact]
        public void ApplyRevokedGroupMemberAccounts_RevokesGroupRemovedFromExistingAccount()
        {
            var revokedOn = new DateTime(2026, 5, 7, 12, 0, 0, DateTimeKind.Utc);
            var desired = new[]
            {
                new GroupMemberAccount { GroupId = 10, AccountId = 100, RevokedOn = null }
            };
            var activeMemberships = new[]
            {
                new GroupMemberAccountSyncState(GroupId: 10, AccountId: 100, ClusterId: 1),
                new GroupMemberAccountSyncState(GroupId: 11, AccountId: 100, ClusterId: 1)
            };

            var syncSet = AccountSyncPlanner.ApplyRevokedGroupMemberAccounts(
                desired,
                activeMemberships,
                new[] { 1 },
                revokedOn);

            syncSet.Count.ShouldBe(2);
            syncSet.Single(gma => gma.GroupId == 10 && gma.AccountId == 100).RevokedOn.ShouldBeNull();
            syncSet.Single(gma => gma.GroupId == 11 && gma.AccountId == 100).RevokedOn.ShouldBe(revokedOn);
        }

        [Fact]
        public void ApplyRevokedGroupMemberAccounts_RevokesMembershipsForAccountsMissingFromPuppet()
        {
            var revokedOn = new DateTime(2026, 5, 7, 12, 0, 0, DateTimeKind.Utc);
            var activeMemberships = new[]
            {
                new GroupMemberAccountSyncState(GroupId: 10, AccountId: 100, ClusterId: 1)
            };

            var syncSet = AccountSyncPlanner.ApplyRevokedGroupMemberAccounts(
                Array.Empty<GroupMemberAccount>(),
                activeMemberships,
                new[] { 1 },
                revokedOn);

            var revokedMembership = syncSet.ShouldHaveSingleItem();
            revokedMembership.GroupId.ShouldBe(10);
            revokedMembership.AccountId.ShouldBe(100);
            revokedMembership.RevokedOn.ShouldBe(revokedOn);
        }

        [Fact]
        public void ApplyRevokedGroupMemberAccounts_DoesNotRevokeMembershipsOutsideSyncedClusters()
        {
            var activeMemberships = new[]
            {
                new GroupMemberAccountSyncState(GroupId: 10, AccountId: 100, ClusterId: 2)
            };

            var syncSet = AccountSyncPlanner.ApplyRevokedGroupMemberAccounts(
                Array.Empty<GroupMemberAccount>(),
                activeMemberships,
                new[] { 1 },
                DateTime.UtcNow);

            syncSet.ShouldBeEmpty();
        }

        [Fact]
        public void ApplyRevokedGroupMemberAccounts_IncludesDesiredMembershipsAsActive()
        {
            var desired = new[]
            {
                new GroupMemberAccount { GroupId = 10, AccountId = 100, RevokedOn = null }
            };

            var syncSet = AccountSyncPlanner.ApplyRevokedGroupMemberAccounts(
                desired,
                Array.Empty<GroupMemberAccountSyncState>(),
                new[] { 1 },
                DateTime.UtcNow);

            var desiredMembership = syncSet.ShouldHaveSingleItem();
            desiredMembership.GroupId.ShouldBe(10);
            desiredMembership.AccountId.ShouldBe(100);
            desiredMembership.RevokedOn.ShouldBeNull();
        }

        [Fact]
        public void GetUniqueUserIdsByKerberos_SkipsAmbiguousKerberos()
        {
            var users = new[]
            {
                new UserKerberosSyncState(UserId: 1, Kerberos: "unique"),
                new UserKerberosSyncState(UserId: 2, Kerberos: "duplicate"),
                new UserKerberosSyncState(UserId: 3, Kerberos: "duplicate")
            };

            var userIdsByKerberos = AccountSyncPlanner.GetUniqueUserIdsByKerberos(users);

            userIdsByKerberos.ShouldBe(new Dictionary<string, int>
            {
                ["unique"] = 1
            });
        }

        [Fact]
        public void GetExistingOwnerIdsByAccountKey_PreservesUnambiguousExistingOwner()
        {
            var existingAccounts = new[]
            {
                new ExistingAccountOwnerSyncState(ClusterId: 7, Kerberos: "kerb", OwnerId: 42),
                new ExistingAccountOwnerSyncState(ClusterId: 7, Kerberos: "kerb", OwnerId: 42)
            };
            var userIdsByKerberos = new Dictionary<string, int>
            {
                ["kerb"] = 1
            };

            var ownerIdsByAccountKey = AccountSyncPlanner.GetExistingOwnerIdsByAccountKey(
                existingAccounts,
                userIdsByKerberos);

            ownerIdsByAccountKey.ShouldBe(new Dictionary<(int ClusterId, string Kerberos), int?>
            {
                [(7, "kerb")] = 42
            });
        }

        [Fact]
        public void GetExistingOwnerIdsByAccountKey_UsesUniqueKerberosUserWhenExistingOwnersConflict()
        {
            var existingAccounts = new[]
            {
                new ExistingAccountOwnerSyncState(ClusterId: 7, Kerberos: "kerb", OwnerId: 42),
                new ExistingAccountOwnerSyncState(ClusterId: 7, Kerberos: "kerb", OwnerId: 43)
            };
            var userIdsByKerberos = new Dictionary<string, int>
            {
                ["kerb"] = 1
            };

            var ownerIdsByAccountKey = AccountSyncPlanner.GetExistingOwnerIdsByAccountKey(
                existingAccounts,
                userIdsByKerberos);

            ownerIdsByAccountKey.ShouldBe(new Dictionary<(int ClusterId, string Kerberos), int?>
            {
                [(7, "kerb")] = 1
            });
        }

        [Fact]
        public void GetExistingOwnerIdsByAccountKey_DoesNotSelectOwnerWhenNoExistingOwnersAreSet()
        {
            var existingAccounts = new[]
            {
                new ExistingAccountOwnerSyncState(ClusterId: 7, Kerberos: "kerb", OwnerId: null),
                new ExistingAccountOwnerSyncState(ClusterId: 7, Kerberos: "kerb", OwnerId: null)
            };
            var userIdsByKerberos = new Dictionary<string, int>
            {
                ["kerb"] = 1
            };

            var ownerIdsByAccountKey = AccountSyncPlanner.GetExistingOwnerIdsByAccountKey(
                existingAccounts,
                userIdsByKerberos);

            ownerIdsByAccountKey.ShouldBeEmpty();
        }

        [Fact]
        public void GetExistingOwnerIdsByAccountKey_DoesNotSelectOwnerWhenExistingOwnersConflictAndKerberosIsAmbiguous()
        {
            var existingAccounts = new[]
            {
                new ExistingAccountOwnerSyncState(ClusterId: 7, Kerberos: "kerb", OwnerId: 42),
                new ExistingAccountOwnerSyncState(ClusterId: 7, Kerberos: "kerb", OwnerId: 43)
            };

            var ownerIdsByAccountKey = AccountSyncPlanner.GetExistingOwnerIdsByAccountKey(
                existingAccounts,
                new Dictionary<string, int>());

            ownerIdsByAccountKey.ShouldBeEmpty();
        }

        [Fact]
        public void GetDesiredOwnerId_PreservesExistingOwnerWhenKerberosHasNoMatchingUser()
        {
            var userIdsByKerberos = new Dictionary<string, int>
            {
                ["unique"] = 1
            };
            var existingOwnerIdsByAccountKey = new Dictionary<(int ClusterId, string Kerberos), int?>
            {
                [(7, "missing")] = 42
            };

            var ownerId = AccountSyncPlanner.GetDesiredOwnerId(
                7,
                "missing",
                userIdsByKerberos,
                existingOwnerIdsByAccountKey);

            ownerId.ShouldBe(42);
        }

        [Fact]
        public void GetDesiredOwnerId_PreservesExistingOwnerWhenKerberosHasUniqueMatchingUser()
        {
            var userIdsByKerberos = new Dictionary<string, int>
            {
                ["kerb"] = 1
            };
            var existingOwnerIdsByAccountKey = new Dictionary<(int ClusterId, string Kerberos), int?>
            {
                [(7, "kerb")] = 42
            };

            var ownerId = AccountSyncPlanner.GetDesiredOwnerId(
                7,
                "kerb",
                userIdsByKerberos,
                existingOwnerIdsByAccountKey);

            ownerId.ShouldBe(42);
        }

        [Fact]
        public void GetDesiredOwnerId_ClearsOwnerWhenKerberosHasNoMatchingUserOrExistingOwner()
        {
            var ownerId = AccountSyncPlanner.GetDesiredOwnerId(
                7,
                "missing",
                new Dictionary<string, int>(),
                new Dictionary<(int ClusterId, string Kerberos), int?>());

            ownerId.ShouldBeNull();
        }

        [Fact]
        public void GetDesiredOwnerId_PreservesExistingOwnerWhenKerberosIsAmbiguous()
        {
            var userIdsByKerberos = new Dictionary<string, int>
            {
                ["unique"] = 1
            };
            var existingOwnerIdsByAccountKey = new Dictionary<(int ClusterId, string Kerberos), int?>
            {
                [(7, "duplicate")] = 42
            };

            var ownerId = AccountSyncPlanner.GetDesiredOwnerId(
                7,
                "duplicate",
                userIdsByKerberos,
                existingOwnerIdsByAccountKey);

            ownerId.ShouldBe(42);
        }
    }
}
