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
        Task<Account> AddHistory(Account account, string action, string note = null);
        Task<Account> Requested(Account account);
        Task<Account> Approved(Account account);
        Task<Account> Rejected(Account account, string note = null);
    }

    public class HistoryService : IHistoryService
    {
        public IUserService _userService { get; }
        public HistoryService(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<Account> AddHistory(Account account, string action, string note = null)
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

        public Task<Account> Requested(Account account)
        {
            return AddHistory(account, Actions.Requested);
        }

        public Task<Account> Approved(Account account)
        {
            return AddHistory(account, Actions.Approved);
        }

        public Task<Account> Rejected(Account account, string note = null)
        {
            return AddHistory(account, Actions.Rejected, note);
        }
    }
}
