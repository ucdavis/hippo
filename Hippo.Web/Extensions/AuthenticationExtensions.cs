using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Hippo.Core.Data;
using Hippo.Core.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Harvest.Web.Extensions
{
    public static class AuthenticationExtensions
    {


        public static string GetUserDetails(this HttpContext context) {
            var user = context.User.GetUserInfo();

            return JsonSerializer.Serialize(user, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }
    }
}