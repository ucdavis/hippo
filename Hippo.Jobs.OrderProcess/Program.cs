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
using Serilog;


namespace Hippo.Jobs.OrderProcess
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

                var slothService = provider.GetRequiredService<ISlothService>();
                var paymentsService = provider.GetRequiredService<IPaymentsService>();

                var successCreatePayments = paymentsService.CreatePayments().GetAwaiter().GetResult();
                if(!successCreatePayments)
                {
                    Log.Error("There was one or more problems running the payments service 1.");
                }

                var successPayments = slothService.ProcessPayments().GetAwaiter().GetResult();

                var successUpdates = slothService.UpdatePayments().GetAwaiter().GetResult();

                if (!successPayments)
                {
                    Log.Error("There was one or more problems running the sloth service 1.");                    
                }
                if (!successUpdates)
                {
                    Log.Error("There was one or more problems running the sloth service 2.");
                }
                if (!successPayments || !successUpdates)
                {
                    return 1;
                }


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
            services.Configure<SlothSettings>(Configuration.GetSection("Sloth"));

            services.AddScoped<IPaymentsService, PaymentsService>();
            services.AddSingleton<IHistoryService, HistoryService>();
            services.AddHttpClient();
            services.AddSingleton<ISecretsService, SecretsService>();
            services.AddScoped<ISlothService, SlothService>();


            return services.BuildServiceProvider();
        }

    }
}