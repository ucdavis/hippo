using System.Threading.Tasks;
using Hippo.Core.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace Test;

public class ModelCompatibilityTests
{
    [Fact]
    public void SqlServerModelMatchesExistingSnapshot()
    {
        using var db = new AppDbContextSqlServer(new DbContextOptionsBuilder<AppDbContextSqlServer>()
            .UseSqlServer("Server=localhost;Database=unused;Integrated Security=true").Options);
        db.Database.HasPendingModelChanges().ShouldBeFalse();
    }

    [Fact]
    public void SqliteModelMatchesExistingSnapshot()
    {
        using var db = new AppDbContextSqlite(new DbContextOptionsBuilder<AppDbContextSqlite>()
            .UseSqlite("Data Source=:memory:").Options);
        db.Database.HasPendingModelChanges().ShouldBeFalse();
    }

    [Fact(Skip = "Existing SQLite migration history creates IX_Clusters_Name twice (also fails on EF 8). Migration edits require separate authorization; see README.")]
    public async Task SqliteMigrationHistoryAppliesToEmptyDatabase()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new AppDbContextSqlite(new DbContextOptionsBuilder<AppDbContextSqlite>()
            .UseSqlite(connection).Options);
        db.Database.HasPendingModelChanges().ShouldBeFalse();
        await db.Database.MigrateAsync();
        (await db.Database.GetPendingMigrationsAsync()).ShouldBeEmpty();
    }
}
