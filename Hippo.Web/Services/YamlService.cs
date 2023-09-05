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
            var groupName = await _dbContext.Groups.Where(g => g.Id == accountCreateModel.GroupId).Select(g => g.Name).SingleOrDefaultAsync();
            if (string.IsNullOrWhiteSpace(groupName))
            {
                throw new KeyNotFoundException($"Group with id {accountCreateModel.GroupId} not found");
            }

            var yaml = new Serializer();

            return yaml.Serialize(
                new
                {
                    groups = new[] { groupName },
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
