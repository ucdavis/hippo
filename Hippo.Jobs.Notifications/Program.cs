using System;
using System.Threading.Tasks;
using Hippo.Core.Data;
using Hippo.Core.Models.Settings;
using Hippo.Core.Services;
using Hippo.Jobs.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mjml.Net;
using Serilog;


namespace Hippo.Jobs.Notifications
{
    class Program : JobBase
    {
        static int Main(string[] args)
        {
            try
            {
                Configure(jobName: typeof(Program).Assembly.GetName().Name, jobId: Guid.NewGuid());
                var assembyName = typeof(Program).Assembly.GetName();

                Log.Information("Running {job} build {build}", assembyName.Name, assembyName.Version);

                // setup di
                var provider = ConfigureServices();

                var expiringOrdersService = provider.GetRequiredService<IExpiringOrdersService>();
                var notificationService = provider.GetRequiredService<INotificationService>(); //For nagging

                var result = expiringOrdersService.ProcessExpiringOrderNotifications().GetAwaiter().GetResult();
                Log.Information("Expiring Orders Service ran successfully. {result}", result);

                result = notificationService.ProcessOrdersInCreatedStatus([DayOfWeek.Monday]).GetAwaiter().GetResult();
                Log.Information("ProcessOrdersInCreatedStatus Service ran successfully. {result}", result);

                result = notificationService.NagSponsorsAboutPendingAccounts([DayOfWeek.Monday]).GetAwaiter().GetResult();
                Log.Information("NagSponsorsAboutPendingAccounts Service ran successfully. {result}", result);

            }
            catch (Exception ex) //Maybe have a try catch for each service call?
            {
                Log.Error(ex, "Unhandled exception");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }

            return 0;
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
            services.AddMemoryCache();

            services.Configure<AuthSettings>(Configuration.GetSection("Authentication")); //Don't know if I need this. Copy Pasta 
            services.Configure<AzureSettings>(Configuration.GetSection("Azure"));
            services.Configure<EmailSettings>(Configuration.GetSection("Email"));

            services.AddScoped<IExpiringOrdersService, ExpiringOrdersService>();
            services.AddSingleton<IHistoryService, HistoryService>(); //Might need to be scoped
            services.AddHttpClient();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IMjmlRenderer, MjmlRenderer>();



            return services.BuildServiceProvider();
        }

    }
}