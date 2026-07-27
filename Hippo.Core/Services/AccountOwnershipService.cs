using Hippo.Core.Data;
using Hippo.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Hippo.Core.Services;

public static class AccountOwnershipService
{
    public static async Task<int> LinkAccountsToUser(AppDbContext dbContext, User user)
    {
        if (string.IsNullOrWhiteSpace(user.Kerberos))
        {
            return 0;
        }

        if (await HasMultipleUsersWithKerberos(dbContext, user.Kerberos))
        {
            Log.Warning(
                "Multiple users found with Kerberos {Kerberos}. Skipping account ownership linking for user {UserId}.",
                user.Kerberos,
                user.Id);
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

        var users = await dbContext.Users
            .Where(u => u.Kerberos == account.Kerberos)
            .Take(2)
            .ToListAsync();

        if (users.Count == 0)
        {
            return false;
        }

        if (users.Count > 1)
        {
            Log.Warning(
                "Multiple users found with Kerberos {Kerberos}. Skipping owner assignment for account {AccountId}.",
                account.Kerberos,
                account.Id);
            return false;
        }

        account.Owner = users.Single();
        return true;
    }

    private static async Task<bool> HasMultipleUsersWithKerberos(AppDbContext dbContext, string kerberos)
    {
        var userCount = await dbContext.Users
            .Where(u => u.Kerberos == kerberos)
            .Take(2)
            .CountAsync();

        return userCount > 1;
    }
}
