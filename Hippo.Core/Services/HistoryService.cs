using Hippo.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hippo.Core.Services
{
    public interface IHistoryService
    {
        Task<Account> AddHistory(Account account, string action);
    }

    public class HistoryService : IHistoryService
    {
        public IUserService _userService { get; }
        public HistoryService(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<Account> AddHistory(Account account, string action)
        {
            var currentUser = await _userService.GetCurrentUser();
            var history = new AccountHistory
            {
                Action = action,
                Status = account.Status,
                Account = account,
            };
            if (currentUser != null)
            {
                history.ActorId = currentUser.Id;
            }
            account.Histories.Add(history);
            return account;
        }
    }
}
