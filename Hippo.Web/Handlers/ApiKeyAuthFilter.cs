using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Hippo.Web.Handlers;

public class ApiKeyAttribute : ServiceFilterAttribute
{
    public ApiKeyAttribute() : base(typeof(ApiKeyAuthFilter)) { }
}

public class ApiKeyAuthFilter : IAuthorizationFilter
{
    private readonly IConfiguration _configuration;
    public ApiKeyAuthFilter(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        string userApiKey = context.HttpContext.Request.Headers[ApiKeyConstants.ApiKeyHeaderName].ToString();
        if (string.IsNullOrWhiteSpace(userApiKey))
        {
            context.Result = new BadRequestObjectResult(new { message = "No api key specified" });
            return;
        }
        if (!IsValidApiKey(userApiKey))
        {
            context.Result = new UnauthorizedResult();
        }
    }

    private bool IsValidApiKey(string userApiKey)
    {
        if (string.IsNullOrWhiteSpace(userApiKey))
            return false;
        string? apiKey = _configuration.GetValue<string>(ApiKeyConstants.ApiKeyName);
        if (apiKey == null || apiKey != userApiKey)
            return false;
        return true;
    }

}


public static class ApiKeyConstants
{
    public const string ApiKeyHeaderName = "X-API-Key";
    public const string ApiKeyName = "ApiKey";
}

