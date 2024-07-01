using Hippo.Core.Data;
using Hippo.Core.Domain;
using Hippo.Core.Migrations.Sqlite;
using Hippo.Core.Models;
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
        Task AddHistory(History history, string clusterName = null);
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

        // This constructor for use by jobs, which don't register the IHttpContextAccessor or IUserService in DI
        public HistoryService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddHistory(History history, string clusterName = null)
        {
            if (history.ActedBy == null && _userService != null)
            {
                history.ActedBy = await _userService.GetCurrentUser();
            }

            if (history.ClusterId == 0)
            {
                if (string.IsNullOrWhiteSpace(clusterName) && _httpContextAccessor != null)
                {
                    clusterName = _httpContextAccessor.HttpContext.GetRouteValue("cluster") as string;
                }

                if (!string.IsNullOrWhiteSpace(clusterName))
                {
                    history.ClusterId = await _dbContext.Clusters.Where(c => c.Name == clusterName).Select(c => c.Id).SingleOrDefaultAsync();
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

        public static async Task QueuedEventCreated(this IHistoryService historyService, QueuedEvent queuedEvent)
        {
            // convert the QueuedEvent to a QueuedEventModel for cleaner serialization
            var queuedEventModel = QueuedEventModel.FromQueuedEvent(queuedEvent);
            var history = new History
            {
                Action = Actions.QueuedEventCreated,
                Details = Serialize(queuedEventModel),
                ActedDate = DateTime.UtcNow
            };
            await historyService.AddHistory(history, queuedEventModel.Data.Cluster);
        }

        public static async Task QueuedEventUpdated(this IHistoryService historyService, QueuedEvent queuedEvent)
        {
            // convert the QueuedEvent to a QueuedEventModel for cleaner serialization
            var queuedEventModel = QueuedEventModel.FromQueuedEvent(queuedEvent);
            var history = new History
            {
                Action = Actions.QueuedEventUpdated,
                Details = Serialize(queuedEventModel),
                ActedDate = DateTime.UtcNow
            };
            await historyService.AddHistory(history, queuedEventModel.Data.Cluster);
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
            var history = CreateHistory(user, perm, Actions.RoleAdded);
            await historyService.AddHistory(history);
        }

        public static async Task RoleRemoved(this IHistoryService historyService, User user, Permission perm)
        {
            var history = CreateHistory(user, perm, Actions.RoleRemoved);
            await historyService.AddHistory(history);
        }

        public static async Task PuppetDataSynced(this IHistoryService historyService, int clusterId)
        {
            var history = new History
            {
                Action = Actions.PuppetDataSynced,
                ClusterId = clusterId,
                ActedDate = DateTime.UtcNow,
            };
            await historyService.AddHistory(history);
        }

        public static async Task SoftwareInstallRequested(this IHistoryService historyService, SoftwareRequestModel softwareRequestModel)
        {
            var history = new History
            {
                Action = Actions.SoftwareInstallRequested,
                Details = Serialize(softwareRequestModel),
                ActedDate = DateTime.UtcNow
            };
            await historyService.AddHistory(history);
        }
    }
}
