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
using Hippo.Web.Models.Settings;
using Hippo.Web.Services;
using System.Security.Claims;
using Hippo.Core.Utilities;
using Serilog;
using Hippo.Web.Middleware;

namespace Hippo.Web
{
    public class Startup
    {
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

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });

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
                    if (!identity.HasClaim(c => c.Type == "ucdPersonIAMID"))
                    {
                        var identityService = context.HttpContext.RequestServices.GetRequiredService<IIdentityService>();
                        var kerbId = identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                        if (kerbId != null)
                        {
                            var identityUser = await identityService.GetByKerberos(kerbId.Value);

                            if (identityUser != null)
                            {
                                identity.AddClaim(new Claim("ucdPersonIAMID", identityUser.Iam));
                            }
                        }
                    }

                    // Ensure user exists in the db
                    var userService = context.HttpContext.RequestServices.GetRequiredService<IUserService>();
                    await userService.GetUser(identity.Claims.ToArray());
                };
            });

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
            services.Configure<SshSettings>(Configuration.GetSection("SSH"));

            services.AddSingleton<IFileProvider>(new PhysicalFileProvider(Directory.GetCurrentDirectory()));
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IIdentityService, IdentityService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ISshService, SshService>();
            services.AddScoped<IUserService, UserService>();
            services.AddSingleton<IHttpContextAccessor, NullHttpContextAccessor>();

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, AppDbContext dbContext)
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
            app.UseSpaStaticFiles(new StaticFileOptions()
            {
                OnPrepareResponse = (context) =>
                {
                    if (context.Context.Request.Path.StartsWithSegments("/static"))
                    {
                        var headers = context.Context.Response.GetTypedHeaders();
                        headers.CacheControl = new Microsoft.Net.Http.Headers.CacheControlHeaderValue
                        {
                            Public = true,
                            MaxAge = TimeSpan.FromDays(365)
                        };
                    }
                }
            });

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
                    constraints: new { controller = "(home|test|system)" }
                );

                // API routes map to all other controllers
                endpoints.MapControllerRoute(
                    name: "API",
                    pattern: "/api/{controller=Account}/{action=Index}/{id?}");

                // any other nonfile route should be handled by the spa, except leave the sockjs route alone if we are in dev mode (hot reloading)
                if (env.IsDevelopment())
                {
                    endpoints.MapControllerRoute(
                        name: "react",
                        pattern: "{*path:nonfile}",
                        defaults: new { controller = "Home", action = "Index" },
                        constraints: new { path = new RegexRouteConstraint("^(?!sockjs-node).*$") }
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

            // SPA needs to kick in for all paths during development
            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });
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


            var initializer = new DbInitializer(dbContext);
            initializer.Initialize(recreateDb).GetAwaiter().GetResult();

        }
    }
}