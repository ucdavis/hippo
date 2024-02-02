

using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Regex = System.Text.RegularExpressions.Regex;
using Hippo.Core.Data;
using Hippo.Core.Domain;
using Hippo.Core.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;
using YamlDotNet.Serialization;

namespace Hippo.Core.Services
{
    public interface IAccountUpdateService
    {
        Task<Result> QueueEvent(QueuedEvent queuedEvent);
        Task<Result> UpdateEvent(QueuedEvent queuedEvent, string status);
    }

    public class AccountUpdateService : IAccountUpdateService
    {
        private readonly AppDbContext _dbContext;
        private readonly IHistoryService _historyService;
        public AccountUpdateService(AppDbContext dbContext, IHistoryService historyService)
        {
            _dbContext = dbContext;
            _historyService = historyService;
        }


        public async Task<Result> QueueEvent(QueuedEvent queuedEvent)
        {
            await _dbContext.QueuedEvents.AddAsync(queuedEvent);
            await _historyService.QueuedEventCreated(queuedEvent);
            await _dbContext.SaveChangesAsync();
            return Result.Ok();
        }

        public async Task<Result> UpdateEvent(QueuedEvent queuedEvent, string status)
        {
            var result = Result.Ok();
            if (!Regex.IsMatch(status, QueuedEvent.Statuses.RegexPattern))
            {
                return Result.Error("Invalid status: {Status}", status);
            }

            if (queuedEvent.Status == QueuedEvent.Statuses.Pending && status == QueuedEvent.Statuses.Complete)
            {
                // Update account and/or group data based on the completed action
                var data = JsonSerializer.Deserialize<QueuedEventDataModel>(
                    queuedEvent.Data, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                result = queuedEvent.Action switch
                {
                    QueuedEvent.Actions.UpdateSshKey => await CompleteUpdateSshKey(data),
                    QueuedEvent.Actions.CreateAccount => await CompleteCreateAccount(data),
                    QueuedEvent.Actions.AddAccountToGroup => await CompleteAddAccountToGroup(data),
                    _ => Result.Error("Unknown action: {Action}", queuedEvent.Action)
                };
            }

            if (result.IsError)
            {
                queuedEvent.Status = QueuedEvent.Statuses.Failed;
                queuedEvent.ErrorMessage = result.Message;
                // No need to log error here, as it's already been done by Result.Error()
                // Not touching the request status here in order to allow a retry after the error is resolved
            }
            else
            {
                queuedEvent.Status = status;
                if (queuedEvent.Request != null)
                {
                    var requestStatus = queuedEvent.Status switch
                    {
                        QueuedEvent.Statuses.Complete => Request.Statuses.Completed,
                        QueuedEvent.Statuses.Canceled => Request.Statuses.Canceled,
                        QueuedEvent.Statuses.Pending => Request.Statuses.Processing,
                        _ => queuedEvent.Request.Status
                    };
                    if (requestStatus != queuedEvent.Request.Status)
                    {
                        queuedEvent.Request.Status = requestStatus;
                        queuedEvent.Request.UpdatedOn = DateTime.UtcNow;
                    }
                }
            }

            queuedEvent.UpdatedAt = DateTime.UtcNow;
            _dbContext.QueuedEvents.Update(queuedEvent);
            await _historyService.QueuedEventUpdated(queuedEvent);
            await _dbContext.SaveChangesAsync();
            return result;
        }

        private async Task<Result> CompleteAddAccountToGroup(QueuedEventDataModel data)
        {
            var accountModel = data.Accounts.FirstOrDefault();
            var groupModel = data.Groups.FirstOrDefault();
            var account = await _dbContext.Accounts
                .Include(a => a.MemberOfGroups)
                .Where(a => a.Cluster.Name == data.Cluster && a.Owner.Kerberos == accountModel.Kerberos)
                .FirstOrDefaultAsync();
            var group = await _dbContext.Groups
                .Where(g => g.Cluster.Name == data.Cluster && g.Name == groupModel.Name)
                .FirstOrDefaultAsync();

            if (accountModel == null || groupModel == null)
            {
                return Result.Error("Invalid data: action {Action} requires one account and one group", QueuedEvent.Actions.AddAccountToGroup);
            }

            if (account == null)
            {
                return Result.Error("Account not found: {Kerberos} on cluster {Cluster}", accountModel.Kerberos, data.Cluster);
            }

            if (group == null)
            {
                return Result.Error("Group not found: {Name} on cluster {Cluster}", groupModel.Name, data.Cluster);
            }

            if (!account.MemberOfGroups.Any(g => g.Id == group.Id))
            {
                account.MemberOfGroups.Add(group);
            }

            return Result.Ok();
        }

        private async Task<Result> CompleteCreateAccount(QueuedEventDataModel data)
        {
            var accountModel = data.Accounts.FirstOrDefault();
            var groupModel = data.Groups.FirstOrDefault();
            var account = await _dbContext.Accounts
                .Where(a => a.Cluster.Name == data.Cluster && a.Owner.Kerberos == accountModel.Kerberos)
                .FirstOrDefaultAsync();
            var user = await _dbContext.Users
                .Where(u => u.Kerberos == accountModel.Kerberos)
                .FirstOrDefaultAsync();
            var group = await _dbContext.Groups
                .Where(g => g.Cluster.Name == data.Cluster && g.Name == groupModel.Name)
                .FirstOrDefaultAsync();

            if (accountModel == null || groupModel == null)
            {
                return Result.Error("Invalid data: action {Action} requires one account and one group", QueuedEvent.Actions.CreateAccount);
            }

            if (group == null)
            {
                return Result.Error("Group not found: {Name} on cluster {Cluster}", groupModel.Name, data.Cluster);
            }

            if (user == null)
            {
                return Result.Error("User not found: {Kerberos}", accountModel.Kerberos);
            }

            if (account != null)
            {
                return Result.Error("Account already exists: {Kerberos} on cluster {Cluster}", accountModel.Kerberos, data.Cluster);
            }

            account = new Account
            {
                Owner = user,
                Name = accountModel.Name,
                Email = accountModel.Email,
                Kerberos = accountModel.Kerberos,
                Cluster = await _dbContext.Clusters
                    .Where(c => c.Name == data.Cluster)
                    .SingleAsync(),
                SshKey = accountModel.Key,
                MemberOfGroups = new List<Group> { group }
            };

            await _dbContext.Accounts.AddAsync(account);
            return Result.Ok();
        }

        private async Task<Result> CompleteUpdateSshKey(QueuedEventDataModel data)
        {
            var accountModel = data.Accounts.FirstOrDefault();
            var account = await _dbContext.Accounts
                .Where(a => a.Cluster.Name == data.Cluster && a.Owner.Kerberos == accountModel.Kerberos)
                .FirstOrDefaultAsync();

            if (accountModel == null || string.IsNullOrWhiteSpace(accountModel.Key))
            {
                return Result.Error("Invalid data: action {Action} requires one account and a key", QueuedEvent.Actions.UpdateSshKey);
            }

            if (account == null)
            {
                return Result.Error("Account not found: {Kerberos} on cluster {Cluster}", accountModel.Kerberos, data.Cluster);
            }

            account.SshKey = accountModel.Key;
            return Result.Ok();
        }
    }

    public static class AccountUpdateServiceExtensions
    {
        public static async Task<Result> QueueCreateAccount(this IAccountUpdateService accountUpdateService, Group group, Request request)
        {
            var data = QueuedEventDataModel.FromRequestAndGroup(request, group);
            var queuedEvent = new QueuedEvent
            {
                Action = QueuedEvent.Actions.CreateAccount,
                Status = QueuedEvent.Statuses.Pending,
                Data = JsonSerializer.Serialize(data,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
                Request = request
            };
            var result = await accountUpdateService.QueueEvent(queuedEvent);
            return result;
        }
        public static async Task<Result> QueueAddAccountToGroup(this IAccountUpdateService accountUpdateService, Account account, Group group, Request request)
        {
            var data = QueuedEventDataModel.FromAccountAndGroup(account, group);
            var queuedEvent = new QueuedEvent
            {
                Action = QueuedEvent.Actions.AddAccountToGroup,
                Status = QueuedEvent.Statuses.Pending,
                Data = JsonSerializer.Serialize(data,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
                Request = request
            };
            var result = await accountUpdateService.QueueEvent(queuedEvent);
            return result;
        }

        public static async Task<Result> QueueUpdateSshKey(this IAccountUpdateService accountUpdateService, Account account, string sshKey)
        {
            var data = QueuedEventDataModel.FromAccount(account);
            data.Accounts[0].Key = sshKey;
            var queuedEvent = new QueuedEvent
            {
                Action = QueuedEvent.Actions.UpdateSshKey,
                Status = QueuedEvent.Statuses.Pending,
                Data = JsonSerializer.Serialize(data, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
            };

            var result = await accountUpdateService.QueueEvent(queuedEvent);
            return result;
        }
    }
}