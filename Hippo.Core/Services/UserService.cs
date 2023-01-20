using Hippo.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Hippo.Core.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Hippo.Core.Models;

namespace Hippo.Core.Services
{
    public interface IUserService
    {
        Task<User> GetUser(Claim[] userClaims);
        Task<User> GetCurrentUser();
        Task<string> GetCurrentUserJsonAsync();
        string GetCurrentUserId();
        Task<string> GetCurrentAccountsJsonAsync();
        Task<string> GetAvailableClustersJsonAsync();
    }

    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IIdentityService _identityService;
        private readonly AppDbContext _dbContext;
        public const string IamIdClaimType = "ucdPersonIAMID";

        public UserService(AppDbContext dbContext, IHttpContextAccessor httpContextAccessor, IIdentityService identityService)
        {
            _httpContextAccessor = httpContextAccessor;
            _identityService = identityService;
            _dbContext = dbContext;
        }

        public string GetCurrentUserId()
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(IamIdClaimType);
            return userId;
        }

        public async Task<User> GetCurrentUser()
        {
            if (_httpContextAccessor.HttpContext == null)
            {
                //TODO: Add when we have logging
                //Log.Warning("No HttpContext found. Unable to retrieve or create User.");
                return null;
            }

            var userClaims = _httpContextAccessor.HttpContext.User.Claims.ToArray();

            return await GetUser(userClaims);
        }

        public async Task<string> GetCurrentUserJsonAsync()
        {
            var user = await GetCurrentUser();
            return JsonSerializer.Serialize(user, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }

        public async Task<string> GetCurrentAccountsJsonAsync()
        {
            string iamId = GetCurrentUserId();

            var accounts = await _dbContext.Accounts.Where(a => a.Owner.Iam == iamId).Select(a => new AccountDetail {
                Id = a.Id,
                Name = a.Name,
                CanSponsor = a.CanSponsor,
                Status = a.Status,
                Owner = a.Owner.Name,
                Cluster = a.Cluster.Name,
                Sponsor = a.Sponsor.Name,
                IsAdmin = a.IsAdmin,
            }).ToListAsync();
            return JsonSerializer.Serialize(accounts, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }

        // Get any user based on their claims, creating if necessary
        public async Task<User> GetUser(Claim[] userClaims)
        {
            string iamId = userClaims.Single(c => c.Type == IamIdClaimType).Value;

            var dbUser = await _dbContext.Users.SingleOrDefaultAsync(a => a.Iam == iamId);

            if (dbUser != null)
            {
                if (dbUser.MothraId == null)
                {
                    var foundUser = await _identityService.GetByKerberos(dbUser.Kerberos);
                    dbUser.MothraId = foundUser.MothraId;
                    await _dbContext.SaveChangesAsync();
                }

                return dbUser; // already in the db, just return straight away
            }
            else
            {
                // not in the db yet, create new user and return
                var newUser = new User
                {
                    FirstName = userClaims.Single(c => c.Type == ClaimTypes.GivenName).Value,
                    LastName = userClaims.Single(c => c.Type == ClaimTypes.Surname).Value,
                    Email = userClaims.Single(c => c.Type == ClaimTypes.Email).Value,
                    Iam = iamId,
                    Kerberos = userClaims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value
                };

                var foundUser = await _identityService.GetByKerberos(newUser.Kerberos);
                newUser.MothraId = foundUser.MothraId;

                _dbContext.Users.Add(newUser);

                await _dbContext.SaveChangesAsync();

                return newUser;
            }
        }

        public async Task<string> GetAvailableClustersJsonAsync() {
            var clusters = await _dbContext.Clusters.ToArrayAsync();
            return JsonSerializer.Serialize(clusters, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }
    }
}
