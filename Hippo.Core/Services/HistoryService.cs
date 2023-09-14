using Hippo.Core.Data;
using Hippo.Core.Domain;
using Hippo.Core.Migrations.Sqlite;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Hippo.Core.Domain.History;

namespace Hippo.Core.Services
{
    public interface IHistoryService
    {
        Task AddHistory(History history);
    }

    public class HistoryService : IHistoryService
    {
        private AppDbContext _dbContext { get; }
        private Lazy<Task<int>> _currentClusterId { get; }
        private Lazy<Task<User>> _currentUser { get; }

        public HistoryService(IUserService userService, AppDbContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _currentClusterId = new Lazy<Task<int>>(async () =>
            {
                var cluster = httpContextAccessor.HttpContext.GetRouteValue("cluster") as string;
                if (string.IsNullOrWhiteSpace(cluster))
                {
                    return 0;
                }

                return await _dbContext.Clusters.Where(c => c.Name == cluster).Select(c => c.Id).SingleOrDefaultAsync();
            });
            _currentUser = new Lazy<Task<User>>(async () => await userService.GetCurrentUser());
        }

        public async Task AddHistory(History history)
        {
            if (history.ActedBy == null)
            {
                history.ActedBy = await _currentUser.Value;
            }

            if (history.ClusterId == 0)
            {
                history.ClusterId = await _currentClusterId.Value;
            }

            await _dbContext.Histories.AddAsync(history);
        }
    }

    public static class HistoryServiceExtensions
    {
        private static History CreateHistory(Account account, string action, string details = "")
        {
            return new History
            {
                Action = action,
                AccountStatus = account.Status,
                Account = account,
                Details = $"Kerb: {account.Owner.Kerberos} IAM: {account.Owner.Iam} Email: {account.Owner.Email} Name: {account.Owner.Name}{Environment.NewLine}{details}",
                ClusterId = account.ClusterId
            };
        }

        private static History CreateHistory(User user, Permission perm, string action)
        {
            return new History
            {
                Action = $"{perm.Role.Name} {action}",
                Details = $"Kerb: {user.Kerberos} IAM: {user.Iam} Email: {user.Email} Name: {user.Name}{(perm.Group != null ? $" Group: " : perm.Group.Name)}",
                ClusterId = perm.ClusterId ?? 0,
                AdminAction = true
            };
        }

        public static async Task<Account> AccountRequested(this IHistoryService historyService, Account account)
        {
            var history = CreateHistory(account, Actions.Requested);
            await historyService.AddHistory(history);
            return account;
        }

        public static async Task<Account> AccountApproved(this IHistoryService historyService, Account account, bool isAdminOverride)
        {
            var history = CreateHistory(account, isAdminOverride ? Actions.AdminApproved : Actions.Approved);
            history.AdminAction = isAdminOverride;
            await historyService.AddHistory(history);
            return account;
        }

        public static async Task<Account> AccountUpdated(this IHistoryService historyService, Account account, bool isAdminOverride)
        {
            var history = CreateHistory(account, Actions.Updated);
            await historyService.AddHistory(history);
            return account;
        }

        public static async Task<Account> AccountRejected(this IHistoryService historyService, Account account, bool isAdminOverride, string note = "")
        {
            var history = CreateHistory(account, isAdminOverride ? Actions.AdminRejected :  Actions.Rejected, note);
            history.AdminAction = isAdminOverride;
            await historyService.AddHistory(history);
            return account;
        }

        public static async Task RoleAdded(this IHistoryService historyService, User user, Permission perm)
        {
            var history = CreateHistory(user, perm, "Role Added");
            await historyService.AddHistory(history);
        }

        public static async Task RoleRemoved(this IHistoryService historyService, User user, Permission perm)
        {
            var history = CreateHistory(user, perm, "Role Removed");
            await historyService.AddHistory(history);
        }

    }
}
