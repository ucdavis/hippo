using System.Threading.Tasks;
using Hippo.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Context;

namespace Hippo.Web.Middleware
{
    public class LogUserNameMiddleware
    {
        private readonly RequestDelegate _next;

        public LogUserNameMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            using (LogContext.PushProperty("User", context?.User?.Identity?.Name ?? "anonymous"))
            {
                if(context != null)
                {
                    await _next(context);
                }
            } 
        }
    }
}