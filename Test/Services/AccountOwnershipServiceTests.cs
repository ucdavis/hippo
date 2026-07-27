using System.Linq;
using System.Threading.Tasks;
using Hippo.Core.Domain;
using Hippo.Core.Services;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Test.Helpers;
using Xunit;

namespace Test.Services;

public class AccountOwnershipServiceTests
{
    [Fact]
    public async Task LinkAccountsToUser_LinksOnlyUnownedAccountsWithMatchingKerberos()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var cluster = new Cluster { Name = "farm" };
        var user = new User
        {
            FirstName = "Pat",
            LastName = "Requester",
            Email = "pat@example.com",
            Iam = "100000001",
            Kerberos = "pat",
            MothraId = "mothra1"
        };
        var otherUser = new User
        {
            FirstName = "Other",
            LastName = "Owner",
            Email = "other@example.com",
            Iam = "100000002",
            Kerberos = "other",
            MothraId = "mothra2"
        };

        dbContext.AddRange(
            cluster,
            user,
            otherUser,
            new Account { Cluster = cluster, Kerberos = "pat", Name = "Pat", Email = "pat@example.com" },
            new Account { Cluster = cluster, Kerberos = "other", Name = "Other", Email = "other@example.com" },
            new Account { Cluster = cluster, Kerberos = "pat", Name = "Already Owned", Email = "owned@example.com", Owner = otherUser });
        await dbContext.SaveChangesAsync();

        var linkedCount = await AccountOwnershipService.LinkAccountsToUser(dbContext, user);
        await dbContext.SaveChangesAsync();

        linkedCount.ShouldBe(1);
        var accounts = await dbContext.Accounts
            .IgnoreQueryFilters()
            .OrderBy(a => a.Name)
            .ToListAsync();
        accounts.Single(a => a.Name == "Pat").OwnerId.ShouldBe(user.Id);
        accounts.Single(a => a.Name == "Other").OwnerId.ShouldBeNull();
        accounts.Single(a => a.Name == "Already Owned").OwnerId.ShouldBe(otherUser.Id);
    }

    [Fact]
    public async Task LinkAccountToMatchingUser_DoesNotOverwriteExistingOwner()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var cluster = new Cluster { Name = "farm" };
        var matchingUser = new User
        {
            FirstName = "Pat",
            LastName = "Requester",
            Email = "pat@example.com",
            Iam = "100000001",
            Kerberos = "pat",
            MothraId = "mothra1"
        };
        var currentOwner = new User
        {
            FirstName = "Current",
            LastName = "Owner",
            Email = "current@example.com",
            Iam = "100000002",
            Kerberos = "current",
            MothraId = "mothra2"
        };
        var account = new Account
        {
            Cluster = cluster,
            Kerberos = "pat",
            Name = "Pat",
            Email = "pat@example.com",
            Owner = currentOwner
        };
        dbContext.AddRange(cluster, matchingUser, currentOwner, account);
        await dbContext.SaveChangesAsync();

        var linked = await AccountOwnershipService.LinkAccountToMatchingUser(dbContext, account);

        linked.ShouldBeFalse();
        account.OwnerId.ShouldBe(currentOwner.Id);
    }
}
