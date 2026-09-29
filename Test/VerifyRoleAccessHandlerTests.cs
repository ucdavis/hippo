using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Hippo.Core.Data;
using Hippo.Core.Domain;
using Hippo.Core.Models;
using Hippo.Core.Services;
using Hippo.Web.Extensions;
using Hippo.Web.Handlers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Shouldly;
using Test.Helpers;
using Xunit;

namespace Test;

public class VerifyRoleAccessHandlerTests
{
    public static IEnumerable<object[]> PolicyMatrix()
    {
        var policies = new[] { AccessCodes.SystemAccess, AccessCodes.ClusterAdminAccess,
            AccessCodes.FinancialAdminAccess, AccessCodes.ClusterAdminOrFinancialAdminAccess,
            AccessCodes.GroupAdminAccess };
        foreach (var role in new[] { "None", "System", "ClusterAdmin", "FinancialAdmin", "GroupAdmin", "StandaloneGroupAdmin" })
        foreach (var policy in policies)
        foreach (var sameCluster in new[] { true, false })
        {
            var allowed = role == "System" || sameCluster && (role switch
            {
                "ClusterAdmin" => policy is AccessCodes.ClusterAdminAccess or AccessCodes.ClusterAdminOrFinancialAdminAccess or AccessCodes.GroupAdminAccess,
                "FinancialAdmin" => policy is AccessCodes.FinancialAdminAccess or AccessCodes.ClusterAdminOrFinancialAdminAccess,
                "GroupAdmin" => policy == AccessCodes.GroupAdminAccess,
                _ => false
            });
            yield return new object[] { role, policy, sameCluster, allowed };
        }
    }

    [Theory]
    [MemberData(nameof(PolicyMatrix))]
    public async Task OnlyRolesAllowedByPolicySucceed(string role, string policy, bool sameCluster, bool expected)
    {
        await using var db = TestDbContextFactory.Create();
        var (user, cluster) = await AddUser(db, role);
        var allowed = await Authorize(db, user, policy, sameCluster ? cluster.Name : "other");
        allowed.ShouldBe(expected);
        if (role != "GroupAdmin")
        {
            // Administrative permissions must work without any cluster account.
            db.Accounts.Local.Count.ShouldBe(0);
        }
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData("sponsored", true)]
    [InlineData("different", false)]
    public async Task GroupAdminHonorsOptionalGroup(string? group, bool expected)
    {
        await using var db = TestDbContextFactory.Create();
        var (user, cluster) = await AddUser(db, "GroupAdmin");
        (await Authorize(db, user, AccessCodes.GroupAdminAccess, cluster.Name, group)).ShouldBe(expected);
    }

    [Theory]
    [InlineData("account")]
    [InlineData("group")]
    [InlineData("cluster")]
    public async Task InactiveMembershipDoesNotGrantGroupAdmin(string inactive)
    {
        await using var db = TestDbContextFactory.Create();
        var (user, cluster) = await AddUser(db, "GroupAdmin");
        foreach (var account in db.Accounts.Local)
        {
            if (inactive == "account") account.DeactivatedOn = DateTime.UtcNow;
            if (inactive == "group") account.AdminOfGroups[0].IsActive = false;
        }
        if (inactive == "cluster") cluster.IsActive = false;
        await db.SaveChangesAsync();
        (await Authorize(db, user, AccessCodes.GroupAdminAccess, cluster.Name)).ShouldBeFalse();
    }

    [Theory]
    [InlineData("System", true)]
    [InlineData("ClusterAdmin", false)]
    [InlineData("FinancialAdmin", false)]
    [InlineData("GroupAdmin", false)]
    public async Task OnlySystemCanAuthorizeWithoutCluster(string role, bool expected)
    {
        await using var db = TestDbContextFactory.Create();
        var (user, _) = await AddUser(db, role);
        (await Authorize(db, user, AccessCodes.GroupAdminAccess, null)).ShouldBe(expected);
    }

    private static async Task<(User, Cluster)> AddUser(AppDbContext db, string role)
    {
        var cluster = new Cluster { Name = "farm" };
        var user = new User { FirstName = "Test", LastName = "User", Email = "test@example.com", Iam = "100000001", Kerberos = "test" };
        db.AddRange(cluster, user);
        if (role == "GroupAdmin")
        {
            var account = new Account { Name = "Test User", Email = user.Email, Kerberos = user.Kerberos, Owner = user, Cluster = cluster };
            account.AdminOfGroups.Add(new Group { Name = "sponsored", Cluster = cluster });
            db.Accounts.Add(account);
        }
        else if (role != "None")
        {
            db.Permissions.Add(new Permission
            {
                User = user,
                Cluster = role == "System" ? null : cluster,
                Role = new Role { Name = role == "StandaloneGroupAdmin" ? Role.Codes.GroupAdmin : role }
            });
        }
        await db.SaveChangesAsync();
        return (user, cluster);
    }

    private static async Task<bool> Authorize(AppDbContext db, User user, string policy, string? cluster, string? group = null)
    {
        var http = new DefaultHttpContext();
        if (cluster != null) http.Request.RouteValues["cluster"] = cluster;
        if (group != null) http.Request.RouteValues["group"] = group;
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(UserService.IamIdClaimType, user.Iam) }, "test"));
        var requirement = new VerifyRoleAccess(AuthorizationExtensions.GetRoles(policy));
        var context = new AuthorizationHandlerContext(new[] { requirement }, principal, null);
        await new VerifyRoleAccessHandler(db, new HttpContextAccessor { HttpContext = http }).HandleAsync(context);
        return context.HasSucceeded;
    }
}
