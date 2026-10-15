using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using Hippo.Core.Data;
using Hippo.Core.Domain;
using Hippo.Core.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace Test;

public sealed class SqlServerFactAttribute : FactAttribute
{
    public SqlServerFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("HIPPO_TEST_SQLSERVER")))
            Skip = "Set HIPPO_TEST_SQLSERVER to a local disposable SQL Server; each test creates and deletes its own database.";
    }
}

public class SqlServerCompatibilityTests
{
    [SqlServerFact]
    public async Task MigrationHistoryBulkUpsertDeleteJsonOwnershipAndFilters()
    {
        var connection = new SqlConnectionStringBuilder(Environment.GetEnvironmentVariable("HIPPO_TEST_SQLSERVER"))
        { InitialCatalog = "HippoUpgradeTest_" + Guid.NewGuid().ToString("N") };
        await using var db = new AppDbContextSqlServer(new DbContextOptionsBuilder<AppDbContextSqlServer>()
            .UseSqlServer(connection.ConnectionString).Options);
        try
        {
            db.Database.HasPendingModelChanges().ShouldBeFalse();
            await db.Database.MigrateAsync();
            (await db.Database.GetPendingMigrationsAsync()).ShouldBeEmpty();
            var cluster = new Cluster { Name = "test" };
            var user = new User { FirstName = "Pat", LastName = "Researcher", Email = "pat@example.test", Iam = "123", Kerberos = "newpat" };
            db.AddRange(cluster, user);
            await db.SaveChangesAsync();
            var account = new Account { Name = "Pat", Email = user.Email, Kerberos = "oldpat", OwnerId = user.Id, ClusterId = cluster.Id, Data = JsonSerializer.SerializeToElement(new { qos = "normal" }) };
            var bulk = new BulkConfig { UpdateByProperties = new() { nameof(Account.ClusterId), nameof(Account.Kerberos) } };
            await db.BulkInsertOrUpdateAsync(new List<Account> { account }, bulk);
            account.Name = "Updated";
            account.Data = JsonSerializer.SerializeToElement(new { qos = "priority" });
            await db.BulkInsertOrUpdateAsync(new List<Account> { account }, bulk);
            (await db.Accounts.CountAsync()).ShouldBe(1);
            var stored = await db.Accounts.AsNoTracking().SingleAsync();
            stored.Name.ShouldBe("Updated");
            stored.OwnerId.ShouldBe(user.Id);
            stored.Data!.Value.GetProperty("qos").GetString().ShouldBe("priority");
            (await db.Database.SqlQueryRaw<string>("SELECT JSON_VALUE([Data], '$.qos') AS [Value] FROM [Accounts]").SingleAsync()).ShouldBe("priority");
            db.Model.FindEntityType(typeof(Account))!.FindProperty(nameof(Account.Data))!.GetColumnType().ShouldBe("nvarchar(max)");
            var names = new[] { "oldpat", "other" };
            (await db.Accounts.Where(a => names.Contains(a.Kerberos)).CountAsync()).ShouldBe(1);
            var group = new Group { Name = "research", DisplayName = "Research", ClusterId = cluster.Id };
            var request = new Request { ClusterId = cluster.Id, RequesterId = user.Id, Group = group.Name, Action = Request.Actions.AddAccountToGroup, Status = Request.Statuses.Processing };
            db.AddRange(group, request);
            await db.SaveChangesAsync();
            db.GroupMemberAccount.Add(new GroupMemberAccount { AccountId = stored.Id, GroupId = group.Id });
            await db.SaveChangesAsync();
            (await AccountSyncService.GetProcessingRequestsReadyToComplete(db).Select(r => r.Id).ToListAsync()).ShouldContain(request.Id);
            stored.DeactivatedOn = DateTime.UtcNow;
            await db.BulkInsertOrUpdateAsync(new List<Account> { stored }, bulk);
            (await db.Accounts.CountAsync()).ShouldBe(0);
            (await db.Accounts.IgnoreQueryFilters().CountAsync()).ShouldBe(1);
            var temp = new List<TempGroup> { new() { ClusterId = cluster.Id, Group = "research" } };
            await db.BulkInsertAsync(temp);
            await db.BulkDeleteAsync(temp);
            (await db.TempGroups.CountAsync()).ShouldBe(0);
            // Exercise the full synchronization path, including ownership retention and membership deletion.
            cluster.Domain = "test.example";
            await db.SaveChangesAsync();
            db.ChangeTracker.Clear();
            var puppet = new FakePuppet();
            var history = new FakeHistory();
            var sync = new AccountSyncService(puppet, db, history);
            (await sync.Run()).ShouldBeTrue();
            var synced = await db.Accounts.AsNoTracking().SingleAsync();
            synced.OwnerId.ShouldBe(user.Id);
            synced.Kerberos.ShouldBe("oldpat");
            (await db.GroupMemberAccount.CountAsync()).ShouldBe(1);
            puppet.Data.Users.Clear();
            db.ChangeTracker.Clear();
            (await sync.Run()).ShouldBeTrue();
            (await db.Accounts.CountAsync()).ShouldBe(0);
            (await db.GroupMemberAccount.CountAsync()).ShouldBe(0);
            history.Count.ShouldBeGreaterThan(0);
        }
        finally { await db.Database.EnsureDeletedAsync(); }
    }
    private sealed class FakeHistory : IHistoryService
    {
        public int Count { get; private set; }
        public Task AddHistory(History history, string clusterName = null!) { Count++; return Task.CompletedTask; }
    }

    private sealed class FakePuppet : IPuppetService
    {
        public PuppetData Data { get; } = new()
        {
            Groups = new() { new() { Name = "research", Data = JsonNode.Parse("{}") } },
            GroupsWithSponsors = new() { new() { Name = "research", Data = JsonNode.Parse("{}") } },
            Users = new() { new() { Kerberos = "oldpat", Name = "Pat", Email = "pat@example.test", Groups = new[] { "research" }, SponsorForGroups = new[] { "research" }, Data = JsonNode.Parse("{}") } }
        };
        public Task<PuppetData> GetPuppetData(string clusterName, string domain) => Task.FromResult(Data);
    }

}
