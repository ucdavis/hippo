using Hippo.Core.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Test.Helpers;

public static class TestDbContextFactory
{
    public static AppDbContextSqlite Create()
    {
        SQLitePCL.Batteries_V2.Init();

        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContextSqlite>()
            .UseSqlite(connection, contextOwnsConnection: true)
            .Options;

        var dbContext = new AppDbContextSqlite(options);
        dbContext.Database.EnsureCreated();

        return dbContext;
    }
}
