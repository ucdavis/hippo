using Hippo.Core.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hippo.Core.Data
{
    public class DbInitializer
    {
        private readonly AppDbContext _dbContext;

        public DbInitializer(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Initialize(bool recreateDb)
        {
            if (recreateDb)
            {
                //do what needs to be done?
            }

            await CheckAndCreateUser(new User {
                Email = "jsylvestre@ucdavis.edu",
                Kerberos = "jsylvest",
                FirstName = "Jason",
                LastName = "Sylvestre",
                Iam = "1000009309",
            });
            await CheckAndCreateUser(new User
            {
                Email = "srkirkland@ucdavis.edu",
                Kerberos = "postit",
                FirstName = "Scott",
                LastName = "Kirkland",
                Iam = "1000029584",
            });
            await _dbContext.SaveChangesAsync();


            if (!(await _dbContext.Accounts.AnyAsync()))
            {
                var ownerId = (await _dbContext.Users.FirstAsync(a => a.Iam == "1000029584")).Id;
                var account = new Account()
                {
                    CanSponsor = true,
                    OwnerId = ownerId,
                };
                await _dbContext.Accounts.AddAsync(account);
                await _dbContext.SaveChangesAsync();
                var sponsor = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.OwnerId == ownerId);

                var otherOwnerId = (await _dbContext.Users.FirstAsync(a => a.Iam == "1000009309")).Id;
                var otherAccount = new Account()
                {
                    CanSponsor = false,
                    OwnerId = otherOwnerId,
                    SponsorId = sponsor.Id,
                };
                await _dbContext.Accounts.AddAsync(otherAccount);
                await _dbContext.SaveChangesAsync();
            }
        }

        private async Task CheckAndCreateUser(User user)
        {
            var userToCreate = await _dbContext.Users.SingleOrDefaultAsync(a => a.Iam == user.Iam);
            if (userToCreate == null)
            {
                userToCreate = user;
                await _dbContext.Users.AddAsync(userToCreate);
            }
        }
    }
}
