﻿using Hippo.Core.Domain;
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

            var JasonUser = await CheckAndCreateUser(new User
            {
                Email = "jsylvestre@ucdavis.edu",
                Kerberos = "jsylvest",
                FirstName = "Jason",
                LastName = "Sylvestre",
                Iam = "1000009309",
                MothraId = "00600825",
            });
            var ScottUser = await CheckAndCreateUser(new User
            {
                Email = "srkirkland@ucdavis.edu",
                Kerberos = "postit",
                FirstName = "Scott",
                LastName = "Kirkland",
                Iam = "1000029584",
                MothraId = "00183873",
            });
            var JamesUser = await CheckAndCreateUser(new User
            {
                Email = "jscubbage@ucdavis.edu",
                Kerberos = "jscub",
                FirstName = "James",
                LastName = "Cubbage",
                Iam = "1000025056",
                MothraId = "00047699",
            });

            var SlupskyUser = await CheckAndCreateUser(new User
            {
                Email = "cslupsky@ucdavis.edu",
                Kerberos = "cslupsky",
                FirstName = "Carolyn",
                LastName = "Slupsky",
                Iam = "1000012183",
                MothraId = "00598045",
            });

            var OmenAdmin = await CheckAndCreateUser(new User
            {
                Email = "omen@ucdavis.edu",
                Kerberos = "omen",
                Iam = "1000019756",
                FirstName = "Omen",
                LastName = "Wild",
                MothraId = "00457597",
            });

            //for(int i = 1; i <= 5; i++)
            //{
            //    var user = new User { Email = $"fake{i}@ucdavis.edu",
            //        FirstName = $"Fake{i}",
            //        LastName = "Fake",
            //        Kerberos = $"fake{i}",
            //        Iam = $"100000000{i}",
            //    };
            //    await CheckAndCreateUser(user);
            //}

            var cluster = new Cluster()
            {
                Name = "caesfarm",
                Description = "CAES Farm Cluster",
            };
            var fakeCluster = new Cluster()
            {
                Name = "fakefarm",
                Description = "Fake Farm Cluster",
            };
            await CheckAndCreateCluster(cluster);
            await CheckAndCreateCluster(fakeCluster);

            await _dbContext.SaveChangesAsync();

            cluster = await _dbContext.Clusters.FirstAsync();

            await _dbContext.SaveChangesAsync();
        }

        public async Task CheckAndCreateRoles()
        {
            var update = false;
            if (await CheckAndCreateRole(Role.Codes.System))
            { update = true; }

            if (await CheckAndCreateRole(Role.Codes.ClusterAdmin))
            {
                update = true;
            }
            if (await CheckAndCreateRole(Role.Codes.FinancialAdmin))
            {
                update = true;
            }
            if (update)
            {
                await _dbContext.SaveChangesAsync();
            }

        }

        private async Task<bool> CheckAndCreateRole(string roleName)
        {
            if (await _dbContext.Roles.AnyAsync(a => a.Name == roleName))
            {
                return false;
            }
            await _dbContext.Roles.AddAsync(new Role
            {
                Name = roleName,
            });
            return true;
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
            if (await _dbContext.Clusters.AnyAsync(a => a.Name == cluster.Name))
            {
                return;
            }
            await _dbContext.Clusters.AddAsync(cluster);
        }
    }
}
