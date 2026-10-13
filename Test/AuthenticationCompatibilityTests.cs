using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Hippo.Core.Data;
using Hippo.Core.Domain;
using Hippo.Core.Services;
using Hippo.Web.Extensions;
using Hippo.Web.Handlers;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Shouldly;
using Test.Helpers;
using Xunit;

namespace Test;

public class AuthenticationCompatibilityTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task LoginExpiredSessionLogoutAndForbiddenResponsesRemainCompatible(bool xhr)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddHippoAuthentication(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Authentication:ClientId"] = "test-client", ["Authentication:Authority"] = "https://cas.example"
        }).Build());
        builder.Services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
            options.Configuration = new OpenIdConnectConfiguration
            {
                Issuer = "https://cas.example", AuthorizationEndpoint = "https://cas.example/authorize",
                TokenEndpoint = "https://cas.example/token"
            });
        builder.Services.AddAuthorization(options => options.AddPolicy("admin", policy => policy.RequireRole("admin")));
        builder.Services.AddAntiforgery();
        await using var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapGet("/private", () => "authorized").RequireAuthorization();
        app.MapGet("/admin", () => Results.Ok("admin")).RequireAuthorization("admin");
        app.MapGet("/signin", async (HttpContext context) => await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "researcher") }, "test"))));
        app.MapGet("/expired", async (HttpContext context) => await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "researcher") }, "test")),
            new AuthenticationProperties { IssuedUtc = DateTimeOffset.UtcNow.AddHours(-2), ExpiresUtc = DateTimeOffset.UtcNow.AddHours(-1) }));
        app.MapGet("/signout", async (HttpContext context) => await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme));
        app.MapPost("/antiforgery", async (HttpContext context, IAntiforgery antiforgery) =>
        {
            try { await antiforgery.ValidateRequestAsync(context); }
            catch (AntiforgeryValidationException) { context.Response.StatusCode = 400; }
        });
        await app.StartAsync();
        using var client = app.GetTestClient();
        client.BaseAddress = new Uri("https://localhost");
        if (xhr) client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
        var challenge = await client.GetAsync("/private");
        challenge.StatusCode.ShouldBe(HttpStatusCode.Redirect);
        challenge.Headers.Location!.AbsoluteUri.ShouldStartWith("https://cas.example/authorize?");
        var oidc = app.Services.GetRequiredService<IOptionsMonitor<OpenIdConnectOptions>>().Get(OpenIdConnectDefaults.AuthenticationScheme);
        oidc.PushedAuthorizationBehavior.ShouldBe(PushedAuthorizationBehavior.Disable);
        oidc.MapInboundClaims.ShouldBeTrue();
        var signedIn = await client.GetAsync("/signin");
        var cookie = signedIn.Headers.GetValues("Set-Cookie").Single().Split(';')[0];
        client.DefaultRequestHeaders.Add("Cookie", cookie);
        (await client.GetAsync("/private")).StatusCode.ShouldBe(HttpStatusCode.OK);
        var forbidden = await client.GetAsync("/admin");
        forbidden.StatusCode.ShouldBe(xhr ? HttpStatusCode.Forbidden : HttpStatusCode.Redirect);
        forbidden.Headers.Location!.AbsolutePath.ShouldBe("/Account/AccessDenied");
        (await client.PostAsync("/antiforgery", new StringContent(""))).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var signedOut = await client.GetAsync("/signout");
        signedOut.Headers.GetValues("Set-Cookie").Single().ShouldContain("expires=Thu, 01 Jan 1970");
        client.DefaultRequestHeaders.Remove("Cookie");
        var expired = await client.GetAsync("/expired");
        client.DefaultRequestHeaders.Add("Cookie", expired.Headers.GetValues("Set-Cookie").Single().Split(';')[0]);
        (await client.GetAsync("/private")).StatusCode.ShouldBe(HttpStatusCode.Redirect);
    }

    [Fact]
    public async Task MissingCasClaimsAreRecoveredBeforeCreatingUser()
    {
        using var db = TestDbContextFactory.Create();
        var services = new ServiceCollection().AddLogging().AddDataProtection().Services;
        services.AddHippoAuthentication(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["Authentication:ClientId"] = "test-client", ["Authentication:Authority"] = "https://cas.example" }).Build());
        services.AddSingleton<AppDbContext>(db);
        services.AddSingleton<IIdentityService, FakeIdentity>();
        services.AddHttpContextAccessor();
        services.AddScoped<IUserService, UserService>();
        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptionsMonitor<OpenIdConnectOptions>>().Get(OpenIdConnectDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "pat") }, "oidc"));
        var context = new DefaultHttpContext { RequestServices = provider };
        var ticket = new AuthenticationTicket(principal, "oidc");
        var received = new TicketReceivedContext(context, new AuthenticationScheme("oidc", "oidc", typeof(OpenIdConnectHandler)), options, ticket);
        await options.Events.OnTicketReceived(received);
        principal.FindFirst(Constants.IamIdClaimType)!.Value.ShouldBe("12345");
        principal.FindFirst(ClaimTypes.Email)!.Value.ShouldBe("pat@example.test");
        db.Users.Single().Kerberos.ShouldBe("pat");
    }

    [Theory]
    [InlineData(null, 400)]
    [InlineData("wrong", 401)]
    [InlineData("local-key", 200)]
    public void ApiKeyFilterRetainsResponses(string? key, int status)
    {
        var filter = new ApiKeyAuthFilter(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["ApiKey"] = "local-key" }).Build());
        var http = new DefaultHttpContext();
        if (key != null) http.Request.Headers["X-API-Key"] = key;
        var context = new AuthorizationFilterContext(new ActionContext(http, new RouteData(), new ActionDescriptor()), new List<IFilterMetadata>());
        filter.OnAuthorization(context);
        var actual = context.Result switch { BadRequestObjectResult => 400, UnauthorizedResult => 401, null => 200, _ => 500 };
        actual.ShouldBe(status);
    }

    private sealed class FakeIdentity : IIdentityService
    {
        public Task<User> GetByKerberos(string kerb) => Task.FromResult(new User { Kerberos = kerb, Iam = "12345", FirstName = "Pat", LastName = "Researcher", Email = "pat@example.test" });
        public Task<User> GetByEmail(string email) => throw new NotSupportedException();
        public Task<User> GetByIamId(string iamId) => throw new NotSupportedException();
    }
}
