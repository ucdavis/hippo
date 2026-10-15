using System.Security.Claims;
using System.Text.Json;
using Hippo.Core.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Serilog;

namespace Hippo.Web.Extensions;

public static class AuthenticationExtensions
{
    public static string GetUserDetails(this HttpContext context)
    {
        var user = context.User.GetUserInfo();
        return JsonSerializer.Serialize(user, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }

    public static IServiceCollection AddHippoAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
        .AddCookie(options =>
        {
            // Preserve .NET 8 redirects for non-XHR requests, including API endpoints.
            options.Events.OnRedirectToLogin = context => RedirectCookie(context, StatusCodes.Status401Unauthorized);
            options.Events.OnRedirectToAccessDenied = context => RedirectCookie(context, StatusCodes.Status403Forbidden);
        })
        .AddOpenIdConnect(oidc =>
        {
            oidc.ClientId = configuration["Authentication:ClientId"];
            oidc.ClientSecret = configuration["Authentication:ClientSecret"];
            oidc.Authority = configuration["Authentication:Authority"];
            oidc.ResponseType = OpenIdConnectResponseType.Code;
            // Preserve the existing CAS authorization-code flow.
            oidc.PushedAuthorizationBehavior = PushedAuthorizationBehavior.Disable;
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

        return services;
    }

    private static Task RedirectCookie(RedirectContext<CookieAuthenticationOptions> context, int statusCode)
    {
        if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
            context.Request.Query["X-Requested-With"] == "XMLHttpRequest")
        {
            context.Response.Headers.Location = context.RedirectUri;
            context.Response.StatusCode = statusCode;
        }
        else
        {
            context.Response.Redirect(context.RedirectUri);
        }
        return Task.CompletedTask;
    }
}
