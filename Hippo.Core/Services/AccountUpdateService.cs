

using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Regex = System.Text.RegularExpressions.Regex;
using Hippo.Core.Data;
using Hippo.Core.Domain;
using Hippo.Core.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;
using YamlDotNet.Serialization;
using Hippo.Core.Extensions;

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
                result = queuedEvent.Action switch
                {
                    QueuedEvent.Actions.UpdateSshKey => await CompleteUpdateSshKey(queuedEvent),
                    QueuedEvent.Actions.CreateAccount => await CompleteCreateAccount(queuedEvent),
                    QueuedEvent.Actions.AddAccountToGroup => await CompleteAddAccountToGroup(queuedEvent),
                    QueuedEvent.Actions.CreateGroup => await CompleteCreateGroup(queuedEvent),
                    QueuedEvent.Actions.RemoveAccountFromGroup => await CompleteRemoveAccountFromGroup(queuedEvent),
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

        private async Task<Result> CompleteAddAccountToGroup(QueuedEvent queuedEvent)
        {
            var data = queuedEvent.Data;
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

            // _dbContext.SaveAsync() is handled elsewhere for this change
            return Result.Ok();
        }

        private async Task<Result> CompleteCreateAccount(QueuedEvent queuedEvent)
        {
            var data = queuedEvent.Data;
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

            var accountRequestData = queuedEvent.Request.GetAccountRequestData();
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
                MemberOfGroups = new List<Group> { group },
                AcceptableUsePolicyAgreedOn = accountRequestData.AcceptableUsePolicyAgreedOn
            };

            await _dbContext.Accounts.AddAsync(account);
            // _dbContext.SaveAsync() is handled elsewhere for this change
            return Result.Ok();
        }

        private async Task<Result> CompleteCreateGroup(QueuedEvent queuedEvent)
        {
            var data = queuedEvent.Data;
            var accountModel = data.Accounts.FirstOrDefault();
            var groupModel = data.Groups.FirstOrDefault();
            var account = await _dbContext.Accounts
                .Where(a => a.Cluster.Name == data.Cluster && a.Owner.Kerberos == accountModel.Kerberos)
                .FirstOrDefaultAsync();
            var groupExists = await _dbContext.Groups
                .AnyAsync(g => g.Cluster.Name == data.Cluster && g.Name == groupModel.Name);

            if (accountModel == null || groupModel == null)
            {
                return Result.Error("Invalid data: action {Action} requires one account and one group", QueuedEvent.Actions.CreateAccount);
            }

            if (groupExists)
            {
                return Result.Error("Group already exists: {Name} on cluster {Cluster}", groupModel.Name, data.Cluster);
            }

            if (account == null)
            {
                return Result.Error("Account does not exist: {Kerberos} on cluster {Cluster}", accountModel.Kerberos, data.Cluster);
            }

            var newGroup = new Group
            {
                Name = groupModel.Name,
                DisplayName = data.Metadata?["DisplayName"] ?? groupModel.Name,
                AdminAccounts = new () { account },
                ClusterId = account.ClusterId
            };
            

            await _dbContext.Groups.AddAsync(newGroup);
            // _dbContext.SaveAsync() is handled elsewhere for this change
            return Result.Ok();
        }

        private async Task<Result> CompleteUpdateSshKey(QueuedEvent queuedEvent)
        {
            var data = queuedEvent.Data;
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
            // _dbContext.SaveAsync() is handled elsewhere for this change
            return Result.Ok();
        }

        private async Task<Result> CompleteRemoveAccountFromGroup(QueuedEvent queuedEvent)
        {
            var data = queuedEvent.Data;
            var accountModel = data.Accounts.FirstOrDefault();
            var groupModel = data.Groups.FirstOrDefault();
            var account = await _dbContext.Accounts
                .Include(a => a.MemberOfGroups.Where(g => g.Name == groupModel.Name))
                .Where(a => a.Cluster.Name == data.Cluster && a.Owner.Kerberos == accountModel.Kerberos)
                .FirstOrDefaultAsync();

            if (accountModel == null || groupModel == null)
            {
                return Result.Error("Invalid data: action {Action} requires one account and one group", QueuedEvent.Actions.AddAccountToGroup);
            }

            if (account == null)
            {
                return Result.Error("Account not found: {Kerberos} on cluster {Cluster}", accountModel.Kerberos, data.Cluster);
            }

            if (!account.MemberOfGroups.Any())
            {
                return Result.Error("Account {Kerberos} not a member of group {Name} on cluster {Cluster}", account.Kerberos, groupModel.Name, data.Cluster);
            }

            // We're only removing the one group that was included...
            account.MemberOfGroups.Clear();

            // _dbContext.SaveAsync() is handled elsewhere for this change
            return Result.Ok();
        }
    }

    public static class AccountUpdateServiceExtensions
    {
        public static async Task<Result> QueueCreateAccount(this IAccountUpdateService accountUpdateService, Group group, Request request)
        {
            var queuedEvent = new QueuedEvent
            {
                Action = QueuedEvent.Actions.CreateAccount,
                Status = QueuedEvent.Statuses.Pending,
                Data = QueuedEventDataModel.FromRequestAndGroup(request, group),
                Request = request
            };
            var result = await accountUpdateService.QueueEvent(queuedEvent);
            return result;
        }
        public static async Task<Result> QueueAddAccountToGroup(this IAccountUpdateService accountUpdateService, Account account, Group group, Request request)
        {
            var queuedEvent = new QueuedEvent
            {
                Action = QueuedEvent.Actions.AddAccountToGroup,
                Status = QueuedEvent.Statuses.Pending,
                Data = QueuedEventDataModel.FromAccountAndGroup(account, group),
                Request = request
            };
            var result = await accountUpdateService.QueueEvent(queuedEvent);
            return result;
        }

        public static async Task<Result> QueueUpdateSshKey(this IAccountUpdateService accountUpdateService, Account account, string sshKey)
        {
            var queuedEvent = new QueuedEvent
            {
                Action = QueuedEvent.Actions.UpdateSshKey,
                Status = QueuedEvent.Statuses.Pending,
                Data = QueuedEventDataModel.FromAccount(account)
            };
            queuedEvent.Data.Accounts[0].Key = sshKey;

            var result = await accountUpdateService.QueueEvent(queuedEvent);
            return result;
        }

        public static async Task<Result> QueueCreateGroup(this IAccountUpdateService accountUpdateService, Account account, Request request)
        {
            if (request.Action != Request.Actions.CreateGroup)
            {
                throw new InvalidOperationException("Invalid action: request action must be CreateGroup");
            }

            var requestData = request.GetCreateGroupRequestData();
            var queuedEvent = new QueuedEvent
            {
                Action = QueuedEvent.Actions.CreateGroup,
                Status = QueuedEvent.Statuses.Pending,
                Data = QueuedEventDataModel.FromAccountAndGroupName(account, requestData.Name),
                Request = request
            };
            queuedEvent.Data.Metadata["DisplayName"] = requestData.DisplayName;

            var result = await accountUpdateService.QueueEvent(queuedEvent);
            return result;
        }

        public static async Task<Result> QueueRemoveGroupMember(this IAccountUpdateService accountUpdateService, Account account, Group group)
        {
            var queuedEvent = new QueuedEvent
            {
                Action = QueuedEvent.Actions.RemoveAccountFromGroup,
                Status = QueuedEvent.Statuses.Pending,
                Data = QueuedEventDataModel.FromAccountAndGroup(account, group)
            };
            var result = await accountUpdateService.QueueEvent(queuedEvent);
            return result;
        }

    }
}