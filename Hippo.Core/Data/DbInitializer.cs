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

            var JasonUser = await CheckAndCreateUser(new User {
                Email = "jsylvestre@ucdavis.edu",
                Kerberos = "jsylvest",
                FirstName = "Jason",
                LastName = "Sylvestre",
                Iam = "1000009309",
            });
            var ScottUser = await CheckAndCreateUser(new User
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
                    Owner = ScottUser,
                };
                await _dbContext.Accounts.AddAsync(account);

                var otherAccount = new Account()
                {
                    CanSponsor = false,
                    Owner = JasonUser,
                    Sponsor = account,

                };
                await _dbContext.Accounts.AddAsync(otherAccount);                
            }

            await _dbContext.SaveChangesAsync();
        }

        private async Task<User> CheckAndCreateUser(User user)
        {
            var userToCreate = await _dbContext.Users.SingleOrDefaultAsync(a => a.Iam == user.Iam);
            if (userToCreate == null)
            {
                userToCreate = user;
                await _dbContext.Users.AddAsync(userToCreate);
            }
            return userToCreate;
        }
    }
}
