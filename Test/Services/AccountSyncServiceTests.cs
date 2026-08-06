using System.Linq;
using System.Threading.Tasks;
using Hippo.Core.Domain;
using Hippo.Core.Services;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Test.Helpers;
using Xunit;

namespace Test.Services;

public class AccountSyncServiceTests
{
    [Fact]
    public async Task GetProcessingRequestsReadyToComplete_UsesOwnedAccountWhenUserKerberosDiffersFromAccountKerberos()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var cluster = new Cluster { Name = "farm" };
        var requester = new User
        {
            FirstName = "Pat",
            LastName = "Requester",
            Email = "pat@example.com",
            Iam = "100000001",
            Kerberos = "newpat",
            MothraId = "mothra1"
        };
        var account = new Account
        {
            Cluster = cluster,
            Owner = requester,
            Kerberos = "oldpat",
            Name = "Pat",
            Email = "pat@example.com"
        };
        var group = new Group { Cluster = cluster, Name = "research", DisplayName = "Research" };
        var request = new Request
        {
            Requester = requester,
            Group = group.Name,
            Action = Request.Actions.AddAccountToGroup,
            Status = Request.Statuses.Processing,
            Cluster = cluster
        };
        dbContext.AddRange(cluster, requester, account, group, request);
        await dbContext.SaveChangesAsync();
        dbContext.GroupMemberAccount.Add(new GroupMemberAccount { AccountId = account.Id, GroupId = group.Id });
        await dbContext.SaveChangesAsync();

        var readyRequestIds = await AccountSyncService.GetProcessingRequestsReadyToComplete(dbContext)
            .Select(r => r.Id)
            .ToListAsync();

        readyRequestIds.ShouldBe(new[] { request.Id });
    }

    [Fact]
    public async Task GetProcessingRequestsReadyToComplete_UsesNewKerberosAccountAfterPuppetSwitchesAndOwnerIsAssigned()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var cluster = new Cluster { Name = "farm" };
        var requester = new User
        {
            FirstName = "Pat",
            LastName = "Requester",
            Email = "pat@example.com",
            Iam = "100000001",
            Kerberos = "newpat",
            MothraId = "mothra1"
        };
        var account = new Account
        {
            Cluster = cluster,
            Owner = requester,
            Kerberos = "newpat",
            Name = "Pat",
            Email = "pat@example.com"
        };
        var group = new Group { Cluster = cluster, Name = "research", DisplayName = "Research" };
        var request = new Request
        {
            Requester = requester,
            Group = group.Name,
            Action = Request.Actions.AddAccountToGroup,
            Status = Request.Statuses.Processing,
            Cluster = cluster
        };
        dbContext.AddRange(cluster, requester, account, group, request);
        await dbContext.SaveChangesAsync();
        dbContext.GroupMemberAccount.Add(new GroupMemberAccount { AccountId = account.Id, GroupId = group.Id });
        await dbContext.SaveChangesAsync();

        var readyRequestIds = await AccountSyncService.GetProcessingRequestsReadyToComplete(dbContext)
            .Select(r => r.Id)
            .ToListAsync();

        readyRequestIds.ShouldBe(new[] { request.Id });
    }

    [Fact]
    public async Task GetProcessingRequestsReadyToComplete_UsesUnownedMatchingKerberosAccountAsFallback()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var cluster = new Cluster { Name = "farm" };
        var requester = new User
        {
            FirstName = "Pat",
            LastName = "Requester",
            Email = "pat@example.com",
            Iam = "100000001",
            Kerberos = "pat",
            MothraId = "mothra1"
        };
        var account = new Account
        {
            Cluster = cluster,
            Kerberos = "pat",
            Name = "Pat",
            Email = "pat@example.com"
        };
        var group = new Group { Cluster = cluster, Name = "research", DisplayName = "Research" };
        var request = new Request
        {
            Requester = requester,
            Group = group.Name,
            Action = Request.Actions.CreateAccount,
            Status = Request.Statuses.Processing,
            Cluster = cluster
        };
        dbContext.AddRange(cluster, requester, account, group, request);
        await dbContext.SaveChangesAsync();
        dbContext.GroupMemberAccount.Add(new GroupMemberAccount { AccountId = account.Id, GroupId = group.Id });
        await dbContext.SaveChangesAsync();

        var readyRequestIds = await AccountSyncService.GetProcessingRequestsReadyToComplete(dbContext)
            .Select(r => r.Id)
            .ToListAsync();

        readyRequestIds.ShouldBe(new[] { request.Id });
    }

    [Fact]
    public async Task GetProcessingRequestsReadyToComplete_IgnoresDeactivatedAccountsAndRevokedMemberships()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var cluster = new Cluster { Name = "farm" };
        var requester = new User
        {
            FirstName = "Pat",
            LastName = "Requester",
            Email = "pat@example.com",
            Iam = "100000001",
            Kerberos = "pat",
            MothraId = "mothra1"
        };
        var deactivatedAccount = new Account
        {
            Cluster = cluster,
            Owner = requester,
            Kerberos = "oldpat",
            Name = "Pat",
            Email = "pat@example.com",
            DeactivatedOn = System.DateTime.UtcNow
        };
        var activeAccountWithRevokedMembership = new Account
        {
            Cluster = cluster,
            Owner = requester,
            Kerberos = "pat",
            Name = "Pat",
            Email = "pat@example.com"
        };
        var group = new Group { Cluster = cluster, Name = "research", DisplayName = "Research" };
        var request = new Request
        {
            Requester = requester,
            Group = group.Name,
            Action = Request.Actions.AddAccountToGroup,
            Status = Request.Statuses.Processing,
            Cluster = cluster
        };
        dbContext.AddRange(cluster, requester, deactivatedAccount, activeAccountWithRevokedMembership, group, request);
        await dbContext.SaveChangesAsync();
        dbContext.GroupMemberAccount.AddRange(
            new GroupMemberAccount { AccountId = deactivatedAccount.Id, GroupId = group.Id },
            new GroupMemberAccount
            {
                AccountId = activeAccountWithRevokedMembership.Id,
                GroupId = group.Id,
                RevokedOn = System.DateTime.UtcNow
            });
        await dbContext.SaveChangesAsync();

        var readyRequestIds = await AccountSyncService.GetProcessingRequestsReadyToComplete(dbContext)
            .Select(r => r.Id)
            .ToListAsync();

        readyRequestIds.ShouldBeEmpty();
    }
}
