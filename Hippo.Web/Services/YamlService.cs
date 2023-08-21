using Hippo.Core.Data;
using Hippo.Core.Domain;
using Hippo.Web.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;
using YamlDotNet.Serialization;

namespace Hippo.Web.Services
{
    public interface IYamlService
    {
        Task<string> Get(User currentUser, AccountCreateModel accountCreateModel, Cluster cluster);
    }

    public class YamlService : IYamlService
    {
        public AppDbContext _dbContext { get; }

        public YamlService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        

        public async Task<string> Get(User currentUser, AccountCreateModel accountCreateModel, Cluster cluster)
        {
            var sponsorAccount = await _dbContext.Accounts.Include(a => a.Owner).SingleAsync(a => a.Id == accountCreateModel.SponsorId);

            var yaml =  new Serializer();
            
            return yaml.Serialize(
                new
                {
                    sponsor = new
                    {
                        accountname = sponsorAccount.Name,
                        name = sponsorAccount.Owner.Name,
                        email = sponsorAccount.Owner.Email,
                        kerb = sponsorAccount.Owner.Kerberos,
                        iam = sponsorAccount.Owner.Iam,
                        mothra = sponsorAccount.Owner.MothraId,
                    },
                    account = new
                    {
                        name = currentUser.Name,
                        email = currentUser.Email,
                        kerb = currentUser.Kerberos,
                        iam = currentUser.Iam,
                        mothra = currentUser.MothraId,
                        key = accountCreateModel.SshKey
                    },
                    meta = new
                    {
                        cluster = cluster.Name,
                    }
                }
            );
        }
    }
}
