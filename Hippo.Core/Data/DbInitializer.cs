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

            var JasonUser   = await CheckAndCreateUser(new User
            {
                Email       = "jsylvestre@ucdavis.edu",
                Kerberos    = "jsylvest",
                FirstName   = "Jason",
                LastName    = "Sylvestre",
                Iam         = "1000009309",
                IsAdmin     = true,
                MothraId    = "00600825",
            });
            var ScottUser   = await CheckAndCreateUser(new User
            {
                Email       = "srkirkland@ucdavis.edu",
                Kerberos    = "postit",
                FirstName   = "Scott",
                LastName    = "Kirkland",
                Iam         = "1000029584",
                MothraId    = "00183873",
            });
            var JamesUser   = await CheckAndCreateUser(new User
            {
                Email       = "jscubbage@ucdavis.edu",
                Kerberos    = "jscub",
                FirstName   = "James",
                LastName    = "Cubbage",
                Iam         = "1000025056",
                MothraId    = "00047699",
            });

            var SlupskyUser = await CheckAndCreateUser(new User
            {
                Email       = "cslupsky@ucdavis.edu",
                Kerberos    = "cslupsky",
                FirstName   = "Carolyn",
                LastName    = "Slupsky",
                Iam         = "1000012183",
                MothraId    = "00598045",
            });

            var OmenAdmin   = await CheckAndCreateUser(new User
            {
                Email       = "omen@ucdavis.edu",
                Kerberos    = "omen",
                Iam         = "1000019756",
                FirstName   = "Omen",
                LastName    = "Wild",
                IsAdmin     = true,
                MothraId    = "00457597",
            });

            //for(int i = 1; i <= 5; i++)
            //{
            //    var user = new User { Email = $"fake{i}@ucdavis.edu",
            //        FirstName = $"Fake{i}",
            //        LastName = "Fake",
            //        Kerberos = $"fake{i}",
            //        Iam = $"100000000{i}",
            //        IsAdmin = true,
            //    };
            //    await CheckAndCreateUser(user);
            //}

            var cluster     = new Cluster()
            {
                Name        = "Farm-CAES",
                Description = "Farm CAES",
            };
            await CheckAndCreateCluster(cluster);

            await _dbContext.SaveChangesAsync();

            cluster = await _dbContext.Clusters.FirstAsync();

            if (!(await _dbContext.Accounts.AnyAsync()))
            {
                var sampleSsh = "ABC123";

                var ownerId = (await _dbContext.Users.FirstAsync(a => a.Iam == "1000029584")).Id;
                var scottAccount   = new Account()
                {
                    CanSponsor     = true,
                    Owner          = ScottUser,
                    IsActive       = true,
                    Name           = "Scott's Account",
                    SshKey         = sampleSsh,
                    Status         = Account.Statuses.Active,
                    Cluster        = cluster,
                    IsAdmin        = true,
                };
                await _dbContext.Accounts.AddAsync(scottAccount);

                var owenAccount = new Account()
                {
                    CanSponsor = false,
                    Owner = OmenAdmin,
                    IsActive = true,
                    Name = OmenAdmin.Name,
                    SshKey = null,
                    Status = Account.Statuses.Active,
                    Cluster = cluster,
                    IsAdmin = true,
                };
                await _dbContext.Accounts.AddAsync(owenAccount);

                var slupskyAccount = new Account()
                {
                    CanSponsor     = true,
                    Owner          = SlupskyUser,
                    IsActive       = true,
                    Name           = "Slupsky",
                    SshKey         = sampleSsh,
                    Status         = Account.Statuses.Active,
                    Cluster        = cluster,
                };
                await _dbContext.Accounts.AddAsync(slupskyAccount);

                var otherAccount   = new Account()
                {
                    CanSponsor     = false,
                    Owner          = JasonUser,
                    Sponsor        = scottAccount,
                    Name           = "Jason's Account",
                    IsActive       = true,
                    SshKey         = sampleSsh,
                    Status         = Account.Statuses.PendingApproval,
                    Cluster        = cluster,
                };
                await _dbContext.Accounts.AddAsync(otherAccount);

                var pendingAccount = new Account()
                {
                    CanSponsor     = false,
                    Owner          = JamesUser,
                    Sponsor        = scottAccount,
                    Name           = "James' Account",
                    IsActive       = true,
                    SshKey         = sampleSsh,
                    Status         = Account.Statuses.PendingApproval,
                    Cluster        = cluster,
                };
                await _dbContext.Accounts.AddAsync(pendingAccount);
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

        private async Task CheckAndCreateCluster(Cluster cluster)
        {
            if(await _dbContext.Clusters.AnyAsync(a => a.Name == cluster.Name))
            {
                return;
            }
            await _dbContext.Clusters.AddAsync(cluster);
        }
    }
}
