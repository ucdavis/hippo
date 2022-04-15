﻿using Hippo.Core.Data;
using Hippo.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Serilog;
using static Hippo.Core.Domain.Account;

namespace Hippo.Web.Services
{
    public interface IBulkLoadService
    {
        Task<int> Load(Cluster cluster, string data);
    }

    public class BulkLoadService : IBulkLoadService
    {
        private readonly AppDbContext _dbContext;
        private readonly IIdentityService _identityService;

        public BulkLoadService(AppDbContext dbContext, IIdentityService identityService)
        {
            _dbContext = dbContext;
            _identityService = identityService;
        }



        public async Task<int> Load(Cluster cluster, string data)
        {
            var count = 0;
            var pairs = data.Split(",");

            Log.Information($"Starting bulk load for {pairs.Count()} accounts");

            //Create missing users
            var kerbs = new List<string>();
            var kerbsInDb = new List<string>();
            var sponsorKerbs = new List<string>();
            foreach(var pair in pairs)
            {
                var kerbIs = pair.Split("-");
                kerbs.Add(kerbIs[0].Trim());
                kerbs.Add(kerbIs[1].Trim());
                sponsorKerbs.Add(kerbIs[1].Trim()); //Load sponsors
            }
            kerbs = kerbs.Distinct().ToList();
            sponsorKerbs = sponsorKerbs.Distinct().ToList();
            Log.Information($"Unique Kerbs: {kerbs.Count()}");

            foreach(var kerb in kerbs)
            {
                var user = await _identityService.GetByKerberos(kerb);
                if(user == null)
                {
                    Log.Error($"Kerb not found: {kerb}");
                    continue;
                }
                if(!await _dbContext.Users.AnyAsync(a => a.Iam == user.Iam))
                {
                    Log.Information($"Adding user: {kerb}");
                    await _dbContext.Users.AddAsync(user);
                }
                kerbsInDb.Add(kerb);
            }
            await _dbContext.SaveChangesAsync();

            //Load Sponsors:
            foreach (var spons in sponsorKerbs)
            {

                var sponsorUser = await _dbContext.Users.SingleAsync(a => a.Kerberos == spons.Trim());

                var sponsorAccount = await _dbContext.Accounts.Where(a => a.ClusterId == cluster.Id && a.OwnerId == sponsorUser.Id).SingleOrDefaultAsync();
                if (sponsorAccount == null)
                {
                    sponsorAccount = new Account()
                    {
                        CanSponsor = true,
                        OwnerId = sponsorUser.Id,
                        ClusterId = cluster.Id,
                        Name = $"{sponsorUser.Name} ({sponsorUser.Email})",
                        Status = Statuses.Active,
                    };
                    await _dbContext.Accounts.AddAsync(sponsorAccount);
                }
                else
                {
                    sponsorAccount.CanSponsor = true;
                    _dbContext.Accounts.Update(sponsorAccount);
                }
            }

            await _dbContext.SaveChangesAsync(); //Save sponsors



            foreach (var pair in pairs)
            {
                var accounts = pair.Split("-");
                if (
                    !kerbsInDb.Contains(accounts[0].Trim()) || 
                    !kerbsInDb.Contains(accounts[1].Trim())
                    ){
                    Log.Error($"Skipping {accounts[0]}-{accounts[1]} because of missing kerb");
                    continue;
                }
                if(accounts[0] == accounts[1])
                {
                    Log.Error($"Can't self sponsor: {accounts[0]}");
                    continue;
                }

                var sponsorUser = await _dbContext.Users.SingleAsync(a => a.Kerberos == accounts[1].Trim());
                var accountUser = await _dbContext.Users.SingleAsync(a => a.Kerberos == accounts[0].Trim());

                var sponsorAccount = await _dbContext.Accounts.Where(a => a.ClusterId == cluster.Id && a.OwnerId == sponsorUser.Id).SingleOrDefaultAsync();
                if(sponsorAccount == null)
                {
                    Log.Error($"Skipping {accounts[0]}-{accounts[1]} because of missing sponsor");
                    continue;
                }

                var newAccount = await _dbContext.Accounts.Where(a => a.ClusterId == cluster.Id && a.OwnerId == accountUser.Id).SingleOrDefaultAsync();
                if(newAccount == null)
                {
                    newAccount = new Account()
                    {
                        CanSponsor = false,
                        OwnerId = accountUser.Id,
                        ClusterId = cluster.Id,
                        Name = $"{accountUser.Name} ({accountUser.Email})",
                        SponsorId = sponsorAccount.Id,
                        Status = Statuses.Active,
                    };
                    await _dbContext.Accounts.AddAsync(newAccount);
                }
                else
                {
                    Log.Information($"Skipping. Account exists: {accounts[0]}");
                }
                count++;
            }

            await _dbContext.SaveChangesAsync();

            return count;
        }
    }
}
