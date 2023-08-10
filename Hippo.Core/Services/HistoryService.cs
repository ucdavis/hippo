﻿using Hippo.Core.Data;
using Hippo.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Hippo.Core.Domain.AccountHistory;

namespace Hippo.Core.Services
{
    public interface IHistoryService
    {
        Task<Account> AddAccountHistory(Account account, string action, string note = null);
        Task<Account> AccountRequested(Account account);
        Task<Account> AccountApproved(Account account);
        Task<Account> AccountRejected(Account account, string note = null);

        Task AddHistory(string action, string details, int clusterId, int? accountId = null, bool adminAction = true);
    }

    public class HistoryService : IHistoryService
    {
        public IUserService _userService { get; }
        public AppDbContext _dbContext { get; }

        public HistoryService(IUserService userService, AppDbContext dbContext)
        {
            _userService = userService;
            _dbContext = dbContext;
        }

        public async Task<Account> AddAccountHistory(Account account, string action, string note = null)
        {
            var currentUser = await _userService.GetCurrentUser();
            var history = new AccountHistory
            {
                Action = action,
                Status = account.Status,
                Account = account,
                Note = note,
            };
            if (currentUser != null)
            {
                history.ActorId = currentUser.Id;
            }
            account.Histories.Add(history);
            return account;
        }

        public Task<Account> AccountRequested(Account account)
        {
            return AddAccountHistory(account, Actions.Requested);
        }

        public Task<Account> AccountApproved(Account account)
        {
            return AddAccountHistory(account, Actions.Approved);
        }

        public Task<Account> AccountRejected(Account account, string note = null)
        {
            return AddAccountHistory(account, Actions.Rejected, note);
        }

        public async Task AddHistory(string action, string details, int clusterId, int? accountId = null, bool adminAction = true)
        {
            var currentUser = await _userService.GetCurrentUser();

            var history = new History { Action = action,
                Details = details,
                AdminAction = adminAction,
                AccountId = accountId,
                ActedBy = currentUser,
                ClusterId = clusterId,
            };

            await _dbContext.Histories.AddAsync(history);
        }
    }
}
