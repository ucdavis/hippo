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
using Serilog;
using Hippo.Core.Extensions;

namespace Hippo.Core.Services
{
    public interface IUserService
    {
        Task<User> GetUser(Claim[] userClaims);
        Task<User> GetCurrentUser();
        Task<string> GetCurrentUserJsonAsync();
        Task<IEnumerable<Permission>> GetCurrentPermissionsAsync();
        Task<string> GetCurrentPermissionsJsonAsync();
        Task<string> GetCurrentOpenRequestsAsync();
        string GetCurrentUserId();
        Task<string> GetCurrentAccountsJsonAsync();
        Task<string> GetAvailableClustersJsonAsync();
        Task<string> GetLastPuppetSync();
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
                Log.Warning("No HttpContext found. Unable to retrieve or create User.");
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

        public async Task<IEnumerable<Permission>> GetCurrentPermissionsAsync()
        {
            if (_httpContextAccessor.HttpContext == null)
            {
                Log.Warning("No HttpContext found. Unable to retrieve User permissions.");
                return new Permission[] { };
            }

            var iamId = _httpContextAccessor.HttpContext.User.Claims.Single(c => c.Type == IamIdClaimType).Value;
            var permissions = await _dbContext.Permissions
                .Include(p => p.Cluster)
                .Include(p => p.Role)
                .Where(p => p.User.Iam == iamId)
                .ToArrayAsync();
            return permissions;
        }

        public async Task<string> GetCurrentPermissionsJsonAsync()
        {
            if (_httpContextAccessor.HttpContext == null)
            {
                Log.Warning("No HttpContext found. Unable to retrieve User permissions.");
                return null;
            }

            var iamId = _httpContextAccessor.HttpContext.User.Claims.Single(c => c.Type == IamIdClaimType).Value;
            var permissions = await _dbContext.Permissions
                .Where(p => p.User.Iam == iamId)
                .Select(p => new PermissionModel
                {
                    Role = p.Role.Name,
                    Cluster = p.Cluster.Name,
                })
                .ToArrayAsync();
            return JsonSerializer.Serialize(permissions, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }

        public async Task<string> GetCurrentAccountsJsonAsync()
        {
            string iamId = GetCurrentUserId();

            var accounts = await _dbContext.Accounts
                .AsSplitQuery()
                .Where(a => a.Owner.Iam == iamId).Select(a => new AccountDetail
                {
                    Id = a.Id,
                    Name = a.Name,
                    Owner = a.Owner.Name,
                    Cluster = a.Cluster.Name,
                    MemberOfGroups = a.MemberOfGroups.Select(g => new GroupModel
                    {
                        Id = g.Id,
                        DisplayName = g.DisplayName,
                        Name = g.Name,
                        Admins = g.AdminAccounts
                            .Select(a => new GroupAccountModel
                            {
                                Kerberos = a.Kerberos,
                                Name = a.Name,
                                Email = a.Email
                            }).ToList(),
                    }).ToList(),
                    AdminOfGroups = a.AdminOfGroups.Select(g => new GroupModel
                    {
                        Id = g.Id,
                        DisplayName = g.DisplayName,
                        Name = g.Name,
                        Admins = g.AdminAccounts
                            .Select(a => new GroupAccountModel
                            {
                                Kerberos = a.Kerberos,
                                Name = a.Name,
                                Email = a.Email
                            }).ToList(),
                    }).ToList(),
                }).ToListAsync();
            return JsonSerializer.Serialize(accounts, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }

        public async Task<string> GetCurrentOpenRequestsAsync()
        {
            string iamId = GetCurrentUserId();

            var requests = await _dbContext.Requests
                .Where(r => r.Requester.Iam == iamId && r.Status != Request.Statuses.Completed && r.Status != Request.Statuses.Rejected)
                .SelectRequestModel(_dbContext)
                .ToListAsync();
            return JsonSerializer.Serialize(requests, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
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

                await _dbContext.Users.AddAsync(newUser);

                // check if any existing accounts need to be associated with this user
                var existingAccounts = await _dbContext.Accounts.Where(a => a.Kerberos == newUser.Kerberos).ToArrayAsync();
                if (existingAccounts.Length > 0)
                {
                    foreach (var account in existingAccounts)
                    {
                        account.Owner = newUser;
                    }
                }

                await _dbContext.SaveChangesAsync();

                return newUser;
            }
        }

        public async Task<string> GetAvailableClustersJsonAsync()
        {
            var clusters = await _dbContext.Clusters.ToArrayAsync();
            return JsonSerializer.Serialize(clusters, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }

        public async Task<string> GetLastPuppetSync()
        {
            var lastSync = await _dbContext.Histories
                .Where(h => h.Action == History.Actions.PuppetDataSynced)
                .OrderByDescending(h => h.ActedDate)
                .Select(h => h.ActedDate)
                .FirstOrDefaultAsync();
            return JsonSerializer.Serialize(lastSync.ToPacificTime(), new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }
    }
}
