using System.Diagnostics;
using System.Security.Claims;
using Hippo.Core.Data;
using Hippo.Core.Models;
using Hippo.Core.Models.Settings;
using Hippo.Core.Services;
using Hippo.Web.Extensions;
using Hippo.Web.Handlers;
using Hippo.Web.Middleware;
using Hippo.Web.Models.Settings;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Mjml.Net;
using MvcReact;
using NSwag.Generation.Processors.Security;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using Serilog.Exceptions.EntityFrameworkCore.Destructurers;
using Serilog.Sinks.Elasticsearch;

#if DEBUG
Serilog.Debugging.SelfLog.Enable(msg => Debug.WriteLine(msg));
#endif

var builder = WebApplication.CreateBuilder(args);

var loggingSection = builder.Configuration.GetSection("Serilog");

var loggerConfig = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    // .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning) // uncomment this to hide EF core general info logs
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithClientIp()
    .Enrich.WithCorrelationId()
    .Enrich.WithRequestHeader("User-Agent")
    .Enrich.WithExceptionDetails(new DestructuringOptionsBuilder()
        .WithDefaultDestructurers()
        .WithDestructurers(new[] { new DbUpdateExceptionDestructurer() }))
    .Enrich.WithProperty("Application", loggingSection.GetValue<string>("AppName"))
    .Enrich.WithProperty("AppEnvironment", loggingSection.GetValue<string>("Environment"))
    .WriteTo.Console();

// add in elastic search sink if the uri is valid
if (Uri.TryCreate(loggingSection.GetValue<string>("ElasticUrl"), UriKind.Absolute, out var elasticUri))
{
    loggerConfig.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(elasticUri)
    {
        IndexFormat = "aspnet-hippo-{0:yyyy.MM}",
        TypeName = null,
    });
}

Log.Logger = loggerConfig.CreateLogger();

try
{
    Log.Information("Configuring web host");

    // Add services to the container.
    builder.Services.AddControllersWithViews(options =>
    {
        options.Filters.Add<SerilogControllerActionFilter>();
    });

    // Init services for hybrid mvc/react app
    builder.Services.AddViteServices(options =>
    {
        options.DevServerPort = 3000;
    });

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie()
    .AddOpenIdConnect(oidc =>
    {
        oidc.ClientId = builder.Configuration["Authentication:ClientId"];
        oidc.ClientSecret = builder.Configuration["Authentication:ClientSecret"];
        oidc.Authority = builder.Configuration["Authentication:Authority"];
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
            if (!identity.HasClaim(c => c.Type == Constants.IamIdClaimType) ||
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
                        if (!identity.HasClaim(c => c.Type == Constants.IamIdClaimType))
                        {
                            identity.AddClaim(new Claim(Constants.IamIdClaimType, identityUser.Iam));
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

    builder.Services.AddAuthorization(options =>
    {
        options.AddAccessPolicy(AccessCodes.SystemAccess);
        options.AddAccessPolicy(AccessCodes.ClusterAdminAccess);
        options.AddAccessPolicy(AccessCodes.ClusterAdminOrFinancialAdminAccess);
        options.AddAccessPolicy(AccessCodes.GroupAdminAccess);
        options.AddAccessPolicy(AccessCodes.FinancialAdminAccess);
    });
    builder.Services.AddScoped<IAuthorizationHandler, VerifyRoleAccessHandler>();

    // Done? (Copied from Harvest): database/EF
    var efProvider = builder.Configuration.GetValue("Provider", "none");
    if (efProvider == "SqlServer" || (efProvider == "none" && builder.Configuration.GetValue<bool>("Dev:UseSql")))
    {
        builder.Services.AddDbContextPool<AppDbContext, AppDbContextSqlServer>((serviceProvider, o) =>
        {
            o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
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
        builder.Services.AddDbContextPool<AppDbContext, AppDbContextSqlite>((serviceProvider, o) =>
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

    // Add NSwag (OpenAPI/Swagger) services
    builder.Services.AddOpenApiDocument(document =>
    {
        // add security definition for api key
        document.DocumentProcessors.Add(
            new SecurityDefinitionAppender("apikey", new NSwag.OpenApiSecurityScheme
            {
                Type = NSwag.OpenApiSecuritySchemeType.ApiKey,
                Name = ApiKeyConstants.ApiKeyHeaderName,
                In = NSwag.OpenApiSecurityApiKeyLocation.Header,
                Description = "API Key Authentication"
            })
        );
        // include api key in request headers
        document.OperationProcessors.Add(new OperationSecurityScopeProcessor("apikey"));
    });

    //Settings:
    builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));
    builder.Services.Configure<AuthSettings>(builder.Configuration.GetSection("Authentication"));
    builder.Services.Configure<AzureSettings>(builder.Configuration.GetSection("Azure"));
    builder.Services.Configure<AggieEnterpriseSettings>(builder.Configuration.GetSection("AggieEnterprise"));
    builder.Services.Configure<SlothSettings>(builder.Configuration.GetSection("Sloth"));
    builder.Services.Configure<PuppetSettings>(builder.Configuration.GetSection("Puppet"));
    builder.Services.Configure<FeatureFlagSettings>(builder.Configuration.GetSection("FeatureFlags"));

    builder.Services.AddSingleton<IFileProvider>(new PhysicalFileProvider(Directory.GetCurrentDirectory()));
    builder.Services.AddScoped<IEmailService, EmailService>();
    builder.Services.AddScoped<IIdentityService, IdentityService>();
    builder.Services.AddScoped<IHistoryService, HistoryService>();
    builder.Services.AddScoped<INotificationService, NotificationService>();
    builder.Services.AddScoped<ISshService, SshService>();
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddTransient<IAggieEnterpriseService, AggieEnterpriseService>();
    builder.Services.AddScoped<IPaymentsService, PaymentsService>();
    builder.Services.AddScoped<IExpiringOrdersService, ExpiringOrdersService>();

    // AccountUpdateYamlService is deprecated and will be removed in a future release.
    // if (builder.Configuration.GetValue<bool>("EventQueueEnabled"))
    // {
        builder.Services.AddScoped<IAccountUpdateService, AccountUpdateService>();
    // }
    // else
    // {
    //     builder.Services.AddScoped<IAccountUpdateService, AccountUpdateYamlService>();
    // }
    builder.Services.AddSingleton<ISecretsService, SecretsService>();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<IMjmlRenderer, MjmlRenderer>();
    builder.Services.AddScoped<ApiKeyAuthFilter>();

    builder.Services.AddHttpClient();
    builder.Services.AddScoped<ISlothService, SlothService>();

    builder.Services.AddMemoryCache();
    builder.Services.AddScoped<IPuppetService, PuppetService>();
    builder.Services.AddScoped<IAccountSyncService, AccountSyncService>();

    builder.Host.UseSerilog();

    var app = builder.Build();

    // Configure middleware
    var mvcReactOptions = app.Services.GetRequiredService<IOptions<MvcReactOptions>>();
    ConfigureDb(app);

    if (app.Environment.IsDevelopment())
    {
        Console.WriteLine("Development environment detected");
        app.UseDeveloperExceptionPage();
    }
    else
    {
        Console.WriteLine("Production environment detected");
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

    app.UseOpenApi();
    app.UseSwaggerUi();

    app.UseMiddleware<LogUserNameMiddleware>();
    app.UseSerilogRequestLogging();

#if DEBUG
    // default for MVC server-side endpoints
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller}/{action}/{id?}",
        defaults: new { controller = "Home", action = "Index" },
        constraints: new { controller = "(home|system|test)" }
    );
#else
    // default for MVC server-side endpoints
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller}/{action}/{id?}",
        defaults: new { controller = "Home", action = "Index" },
        constraints: new { controller = "(home|system)" }
    );
#endif
    // API routes that don't include a {cluster} segment
    app.MapControllerRoute(
        name: "clusteradminAPI",
        pattern: "/api/{controller}/{action=Index}/{id?}",
        constraints: new { controller = "(clusteradmin|eventqueue|software|people|notify)" });

    // remaining API routes map to all other controllers and require cluster
    app.MapControllerRoute(
        name: "API",
        pattern: "/api/{cluster}/{controller=Account}/{action=Index}/{id?}",
        constraints: new { controller = mvcReactOptions.Value.ExcludeHmrPathsRegex });

    // any other nonfile route should be handled by the spa, except leave the sockjs route alone if we are in dev mode (hot reloading)
    if (app.Environment.IsDevelopment())
    {
        app.MapControllerRoute(
            name: "react",
            pattern: "{*path:nonfile}",
            defaults: new { controller = "Home", action = "Index" },
            constraints: new { path = new RegexRouteConstraint(mvcReactOptions.Value.ExcludeHmrPathsRegex) }
        );
    }
    else
    {
        app.MapControllerRoute(
            name: "react",
            pattern: "{*path:nonfile}",
            defaults: new { controller = "Home", action = "Index" }
        );
    }

    // During development, SPA will kick in for all remaining paths
    app.UseMvcReact();

    Log.Information("Starting web host");
    app.Run();

    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
    return 1; // indicate abnormal termination
}
finally
{
    Log.CloseAndFlush();
}

static void ConfigureDb(WebApplication app)
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var dbContext = services.GetRequiredService<AppDbContext>();
            var recreateDb = app.Configuration.GetValue<bool>("Dev:RecreateDb");

            if (recreateDb)
            {
                dbContext.Database.EnsureDeleted();
                dbContext.Dispose();
            }

            dbContext.Database.Migrate();

            var initializeDb = app.Configuration.GetValue<bool>("Dev:InitializeDb");
            var initializer = new DbInitializer(dbContext);

            if (initializeDb)
            {
                initializer.Initialize(recreateDb).GetAwaiter().GetResult();
            }
            initializer.CheckAndCreateRoles().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "An error occurred while migrating or initializing the database.");
            throw;
        }
    }
}

public static class Constants
{
    public const string IamIdClaimType = "ucdPersonIAMID";
}

