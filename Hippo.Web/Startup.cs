using Hippo.Core.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Hippo.Core.Models.Settings;
using Hippo.Core.Services;
using Hippo.Web.Services;
using System.Security.Claims;
using Hippo.Core.Utilities;
using Serilog;
using Hippo.Web.Middleware;
using Hippo.Core.Models;
using Hippo.Web.Handlers;
using Microsoft.AspNetCore.Authorization;
using MvcReact;
using Microsoft.Extensions.Options;

namespace Hippo.Web
{
    public class Startup
    {
        public const string IamIdClaimType = "ucdPersonIAMID";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews(options =>
            {
                options.Filters.Add<SerilogControllerActionFilter>();
            });

            // Init services for hybrid mvc/react app
            services.AddMvcReact();

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie()
            .AddOpenIdConnect(oidc =>
            {
                oidc.ClientId = Configuration["Authentication:ClientId"];
                oidc.ClientSecret = Configuration["Authentication:ClientSecret"];
                oidc.Authority = Configuration["Authentication:Authority"];
                oidc.ResponseType = OpenIdConnectResponseType.Code;
                oidc.Scope.Add("openid");
                oidc.Scope.Add("profile");
                oidc.Scope.Add("email");
                oidc.Scope.Add("eduPerson");
                oidc.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
                };
                oidc.Events.OnTicketReceived = async context =>
                {
                    if (context.Principal == null || context.Principal.Identity == null)
                    {
                        return;
                    }
                    var identity = (ClaimsIdentity)context.Principal.Identity;


                    // Sometimes CAS doesn't return the required IAM ID
                    // If this happens, we take the reliable Kerberos (NameIdentifier claim) and use it to lookup IAM ID
                    if (!identity.HasClaim(c => c.Type == IamIdClaimType) || 
                        !identity.HasClaim(c => c.Type == ClaimTypes.Surname) || 
                        !identity.HasClaim(c => c.Type == ClaimTypes.GivenName) || 
                        !identity.HasClaim(c => c.Type == ClaimTypes.Email))
                    {
                        var identityService = context.HttpContext.RequestServices.GetRequiredService<IIdentityService>();
                        var kerbId = identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                        if (kerbId != null)
                        {
                            Log.Error($"CAS IAM Id Missing. For Kerb: {kerbId}");
                            var identityUser = await identityService.GetByKerberos(kerbId.Value);

                            if (identityUser != null)
                            {
                                if (!identity.HasClaim(c => c.Type == IamIdClaimType))
                                {
                                    identity.AddClaim(new Claim(IamIdClaimType, identityUser.Iam));
                                }
                                //Check for other missing claims
                                if (!identity.HasClaim(c => c.Type == ClaimTypes.Surname))
                                {
                                    identity.AddClaim(new Claim(ClaimTypes.Surname, identityUser.LastName));
                                }
                                if (!identity.HasClaim(c => c.Type == ClaimTypes.GivenName))
                                {
                                    identity.AddClaim(new Claim(ClaimTypes.GivenName, identityUser.FirstName));
                                }
                                if (!identity.HasClaim(c => c.Type == ClaimTypes.Email))
                                {
                                    identity.AddClaim(new Claim(ClaimTypes.Email, identityUser.Email));
                                }
                            }
                            else
                            {
                                Log.Error($"IAM Id Not Found with identity service. For Kerb: {kerbId}");
                            }
                        }
                        else
                        {
                            Log.Error($"CAS IAM Id Missing. Kerb Not Found");
                        }
                    }

                    // Ensure user exists in the db
                    var userService = context.HttpContext.RequestServices.GetRequiredService<IUserService>();
                    await userService.GetUser(identity.Claims.ToArray());
                };
            });

            services.AddAuthorization(options =>
            { 
                options.AddPolicy(AccessCodes.SystemAccess, policy => policy.Requirements.Add(
                    new VerifyRoleAccess(AccessConfig.GetRoles(AccessCodes.SystemAccess))));

                options.AddPolicy(AccessCodes.AdminAccess, policy => policy.Requirements.Add(
                    new VerifyRoleAccess(AccessConfig.GetRoles(AccessCodes.AdminAccess))));
            });
            services.AddScoped<IAuthorizationHandler, VerifyRoleAccessHandler>();

            // Done? (Copied from Harvest): database/EF
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
#endif
                });
            }

            // TODO: authorization

            // TODO: config

            // TODO: DI
            //Settings:
            services.Configure<EmailSettings>(Configuration.GetSection("Email"));
            services.Configure<AuthSettings>(Configuration.GetSection("Authentication"));
            services.Configure<AzureSettings>(Configuration.GetSection("Azure"));

            services.AddSingleton<IFileProvider>(new PhysicalFileProvider(Directory.GetCurrentDirectory()));
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IIdentityService, IdentityService>();
            services.AddScoped<IHistoryService, HistoryService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<ISshService, SshService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IYamlService, YamlService>();
            services.AddSingleton<ISecretsService, SecretsService>();
            services.AddHttpContextAccessor();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, AppDbContext dbContext, IOptions<MvcReactOptions> mvcReactOptions)
        {
            // TODO: DB config/init
            ConfigureDb(dbContext);

            if (env.IsDevelopment())
            {
                System.Console.WriteLine("Development environment detected");
                app.UseDeveloperExceptionPage();
            }
            else
            {
                System.Console.WriteLine("Production environment detected");
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseMvcReactStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware<LogUserNameMiddleware>();
            app.UseSerilogRequestLogging();

            app.UseEndpoints(endpoints =>
            {
                // default for MVC server-side endpoints
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action}/{id?}",
                    defaults: new { controller = "Home", action = "Index" },
                    constraints: new { controller = "(home|system)" }
                );

                // clusteradmin API routes don't include a {cluster} segment
                endpoints.MapControllerRoute(
                    name: "clusteradminAPI",
                    pattern: "/api/{controller}/{action=Index}/{id?}",
                    constraints: new { controller = "(clusteradmin)" });

                // remaining API routes map to all other controllers and require cluster
                endpoints.MapControllerRoute(
                    name: "API",
                    pattern: "/api/{cluster}/{controller=Account}/{action=Index}/{id?}",
                    constraints: new { controller = mvcReactOptions.Value.ExcludeHmrPathsRegex });

                // any other nonfile route should be handled by the spa, except leave the sockjs route alone if we are in dev mode (hot reloading)
                if (env.IsDevelopment())
                {
                    endpoints.MapControllerRoute(
                        name: "react",
                        pattern: "{*path:nonfile}",
                        defaults: new { controller = "Home", action = "Index" },
                        constraints: new { path = new RegexRouteConstraint(mvcReactOptions.Value.ExcludeHmrPathsRegex) }
                    );
                }
                else
                {
                    endpoints.MapControllerRoute(
                        name: "react",
                        pattern: "{*path:nonfile}",
                        defaults: new { controller = "Home", action = "Index" }
                    );
                }
            });

            // During development, SPA will kick in for all remaining paths
            app.UseMvcReact();
        }

        private void ConfigureDb(AppDbContext dbContext)
        {
            var recreateDb = Configuration.GetValue<bool>("Dev:RecreateDb");

            if (recreateDb)
            {
                dbContext.Database.EnsureDeleted();
                dbContext.Dispose();
            }

            dbContext.Database.Migrate();

            var initializeDb = Configuration.GetValue<bool>("Dev:InitializeDb");
            
            if (initializeDb)
            {
                var initializer = new DbInitializer(dbContext);
                initializer.Initialize(recreateDb).GetAwaiter().GetResult();
            }
        }
    }
}