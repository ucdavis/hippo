using System;
using System.Threading.Tasks;
using Hippo.Core.Data;
using Hippo.Core.Models.Settings;
using Hippo.Core.Services;
using Hippo.Core.Utilities;
using Hippo.Jobs.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Hippo.Jobs.PuppetSync
{
    class Program : JobBase
    {
        static void Main(string[] args)
        {
            Configure(jobName: typeof(Program).Assembly.GetName().Name, jobId: Guid.NewGuid());
            var assembyName = typeof(Program).Assembly.GetName();

            Log.Information("Running {job} build {build}", assembyName.Name, assembyName.Version);

            // setup di
            var provider = ConfigureServices();

            var syncService = provider.GetRequiredService<IAccountSyncService>();

            SyncPuppetAccounts(syncService).GetAwaiter().GetResult();
        }


        private static ServiceProvider ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddOptions();

            var efProvider = Configuration.GetValue("Provider", "none");
            if (efProvider == "SqlServer" || (efProvider == "none" && Configuration.GetValue<bool>("Dev:UseSql")))
            {
                services.AddDbContextPool<AppDbContext, AppDbContextSqlServer>((serviceProvider, o) =>
                {
                    o.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"),
                        sqlOptions =>
                        {
                            sqlOptions.MigrationsAssembly("Hippo.Core");
                        });
#if DEBUG
                    o.EnableSensitiveDataLogging();
                    o.LogTo(message => System.Diagnostics.Debug.WriteLine(message));
#endif
                });
            }
            else
            {
                services.AddDbContextPool<AppDbContext, AppDbContextSqlite>((serviceProvider, o) =>
                {
                    var connection = new SqliteConnection("Data Source=hippo.db");
                    o.UseSqlite(connection, sqliteOptions =>
                    {
                        sqliteOptions.MigrationsAssembly("Hippo.Core");
                    });

#if DEBUG
                    o.EnableSensitiveDataLogging();
                    o.LogTo(message => System.Diagnostics.Debug.WriteLine(message));
#endif
                });
            }

            services.Configure<PuppetSettings>(Configuration.GetSection("Puppet"));
            services.Configure<AuthSettings>(Configuration.GetSection("Authentication"));
            services.AddSingleton<IPuppetService, PuppetService>();
            services.AddSingleton<IAccountSyncService, AccountSyncService>();
            services.AddSingleton<IIdentityService, IdentityService>();


            return services.BuildServiceProvider();
        }

        private static async Task SyncPuppetAccounts(IAccountSyncService syncService)
        {
            Log.Information("Syncing Puppet accounts");

            await syncService.Run();
        }
    }
}
