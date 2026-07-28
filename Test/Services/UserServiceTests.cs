using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Hippo.Core.Domain;
using Hippo.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Test.Helpers;
using Xunit;

namespace Test.Services;

public class UserServiceTests
{
    [Fact]
    public async Task GetUser_WhenUserAlreadyExists_LinksMatchingUnownedAccounts()
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
        var account = new Account
        {
            Cluster = cluster,
            Kerberos = "pat",
            Name = "Pat",
            Email = "pat@example.com"
        };
        dbContext.AddRange(cluster, user, account);
        await dbContext.SaveChangesAsync();

        var userService = new UserService(dbContext, new HttpContextAccessor(), new ThrowingIdentityService());

        var result = await userService.GetUser(new[]
        {
            new Claim(UserService.IamIdClaimType, user.Iam),
            new Claim(ClaimTypes.GivenName, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Kerberos)
        });

        result.Id.ShouldBe(user.Id);
        var linkedAccount = await dbContext.Accounts.IgnoreQueryFilters().SingleAsync(a => a.Id == account.Id);
        linkedAccount.OwnerId.ShouldBe(user.Id);
    }

    [Fact]
    public async Task GetUser_WhenNewUserKerberosMatchesExistingUser_DoesNotLinkAccounts()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var cluster = new Cluster { Name = "farm" };
        var existingUser = new User
        {
            FirstName = "Existing",
            LastName = "User",
            Email = "existing@example.com",
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
        dbContext.AddRange(cluster, existingUser, account);
        await dbContext.SaveChangesAsync();

        var userService = new UserService(dbContext, new HttpContextAccessor(), new NullIdentityService());

        var result = await userService.GetUser(new[]
        {
            new Claim(UserService.IamIdClaimType, "100000002"),
            new Claim(ClaimTypes.GivenName, "Pat"),
            new Claim(ClaimTypes.Surname, "Requester"),
            new Claim(ClaimTypes.Email, "pat@example.com"),
            new Claim(ClaimTypes.NameIdentifier, "pat")
        });

        result.Id.ShouldNotBe(existingUser.Id);
        var linkedAccount = await dbContext.Accounts.IgnoreQueryFilters().SingleAsync(a => a.Id == account.Id);
        linkedAccount.OwnerId.ShouldBeNull();
        var matchingUsers = await dbContext.Users.CountAsync(u => u.Kerberos == "pat");
        matchingUsers.ShouldBe(2);
    }

    private class ThrowingIdentityService : IIdentityService
    {
        public Task<User> GetByEmail(string email)
        {
            throw new InvalidOperationException("This test should not call the identity service.");
        }

        public Task<User> GetByKerberos(string kerb)
        {
            throw new InvalidOperationException("This test should not call the identity service.");
        }

        public Task<User> GetByIamId(string iamId)
        {
            throw new InvalidOperationException("This test should not call the identity service.");
        }
    }

    private class NullIdentityService : IIdentityService
    {
        public Task<User> GetByEmail(string email)
        {
            return Task.FromResult<User>(null!);
        }

        public Task<User> GetByKerberos(string kerb)
        {
            return Task.FromResult<User>(null!);
        }

        public Task<User> GetByIamId(string iamId)
        {
            return Task.FromResult<User>(null!);
        }
    }
}
