using Hippo.Core.Data;
using Hippo.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace Hippo.Core.Services;

public static class AccountOwnershipService
{
    public static async Task<int> LinkAccountsToUser(AppDbContext dbContext, User user)
    {
        if (string.IsNullOrWhiteSpace(user.Kerberos))
        {
            return 0;
        }

        var accounts = await dbContext.Accounts
            .Where(a => a.OwnerId == null && a.Kerberos == user.Kerberos)
            .ToListAsync();

        foreach (var account in accounts)
        {
            account.Owner = user;
        }

        return accounts.Count;
    }

    public static async Task<bool> LinkAccountToMatchingUser(AppDbContext dbContext, Account account)
    {
        if (account.OwnerId != null || string.IsNullOrWhiteSpace(account.Kerberos))
        {
            return false;
        }

        var user = await dbContext.Users
            .Where(u => u.Kerberos == account.Kerberos)
            .FirstOrDefaultAsync();

        if (user == null)
        {
            return false;
        }

        account.Owner = user;
        return true;
    }
}
