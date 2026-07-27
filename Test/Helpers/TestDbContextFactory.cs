using System;
using Hippo.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace Test.Helpers;

public static class TestDbContextFactory
{
    public static AppDbContextSqlServer Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContextSqlServer>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContextSqlServer(options);
    }
}
