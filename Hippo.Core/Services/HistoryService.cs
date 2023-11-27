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
using System.Text.Json;
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

        private IHttpContextAccessor _httpContextAccessor { get; }
        private IUserService _userService { get; }

        public HistoryService(IUserService userService, AppDbContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _userService = userService;
        }

        public async Task AddHistory(History history)
        {
            if (history.ActedBy == null)
            {
                history.ActedBy = await _userService.GetCurrentUser();
            }

            if (history.ClusterId == 0)
            {
                var cluster = _httpContextAccessor.HttpContext.GetRouteValue("cluster") as string;
                if (!string.IsNullOrWhiteSpace(cluster))
                {
                    history.ClusterId = await _dbContext.Clusters.AsNoTracking().Where(c => c.Name == cluster).Select(c => c.Id).SingleOrDefaultAsync();
                }
            }

            await _dbContext.Histories.AddAsync(history);
        }
    }

    public static class HistoryServiceExtensions
    {
        private static string Serialize(object obj)
        {
            return JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
        }

        private static History CreateHistory(Account account, string action, string note = "")
        {
            return new History
            {
                Action = action,
                Details = Serialize(new
                {
                    Note = note,
                    account.Owner.Kerberos,
                    account.Owner.Iam,
                    account.Owner.Email,
                    account.Owner.Name
                }),
                ClusterId = account.ClusterId
            };
        }

        private static History CreateHistory(Request request, string action, string note = "")
        {
            return new History
            {
                Action = action,
                Status = request.Status,
                Details = Serialize(new
                {
                    Note = note,
                    request.Requester.Kerberos,
                    request.Requester.Iam,
                    request.Requester.Email,
                    request.Requester.Name
                }),
                ClusterId = request.ClusterId
            };
        }

        private static History CreateHistory(User user, Permission perm, string action)
        {
            return new History
            {
                Action = $"{perm.Role.Name} {action}",
                Details = Serialize(new
                {
                    user.Kerberos,
                    user.Iam,
                    user.Email,
                    user.Name,
                }),
                ClusterId = perm.ClusterId ?? 0,
                AdminAction = true
            };
        }

        public static async Task RequestCreated(this IHistoryService historyService, Request request, string note = "")
        {
            var history = CreateHistory(request, Actions.Requested, note);
            await historyService.AddHistory(history);
        }

        public static async Task RequestApproved(this IHistoryService historyService, Request request, bool isAdminOverride, string note = "")
        {
            var history = CreateHistory(request, Actions.Approved, note);
            history.AdminAction = isAdminOverride;
            await historyService.AddHistory(history);
        }

        public static async Task<Account> AccountUpdated(this IHistoryService historyService, Account account, bool isAdminOverride, string note = "")
        {
            var history = CreateHistory(account, Actions.Updated, note);
            history.AdminAction = isAdminOverride;
            await historyService.AddHistory(history);
            return account;
        }

        public static async Task RequestRejected(this IHistoryService historyService, Request request, bool isAdminOverride, string note = "")
        {
            var history = CreateHistory(request, Actions.Rejected, note);
            history.AdminAction = isAdminOverride;
            await historyService.AddHistory(history);
        }

        public static async Task RequestCompleted(this IHistoryService historyService, Request request, bool isAdminOverride, string note = "")
        {
            var history = CreateHistory(request, Actions.Completed, note);
            history.AdminAction = isAdminOverride;
            await historyService.AddHistory(history);
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
