

using Hippo.Core.Data;
using Hippo.Core.Domain;
using Serilog;
using YamlDotNet.Serialization;

namespace Hippo.Core.Services
{
    public interface IAccountUpdateService
    {
        Task<bool> UpdateAccount(Account account, Group group = null);
    }

    public class AccountUpdateService : IAccountUpdateService
    {
        private readonly ISshService _sshService;
        private readonly AppDbContext _dbContext;
        public AccountUpdateService(ISshService sshService, AppDbContext dbContext)
        {
            _sshService = sshService;
            _dbContext = dbContext;
        }

        public async Task<bool> UpdateAccount(Account account, Group group = null)
        {
            try
            {
                var connectionInfo = await _dbContext.Clusters.GetSshConnectionInfo(account.Cluster.Name);
                var tempFileName = $"/var/lib/remote-api/.{account.Owner.Kerberos}.yaml"; //Leading .
                var fileName = $"/var/lib/remote-api/{account.Owner.Kerberos}.yaml";
                var yaml = GetYaml(account, group);
                await _sshService.PlaceFile(yaml, tempFileName, connectionInfo);
                await _sshService.RenameFile(tempFileName, fileName, connectionInfo);
                return true;
            }
            catch (Exception e)
            {
                Log.Error(e, "Error updating account");
                return false;
            }
        }

        private string GetYaml(Account account, Group group = null)
        {
            if (account == null)
            {
                throw new InvalidOperationException($"Account is required");
            }

            if (account.Owner == null)
            {
                throw new InvalidOperationException($"Account Owner is required");
            }

            if (account.Cluster == null)
            {
                throw new InvalidOperationException($"Cluster is required");
            }

            var yaml = new Serializer();

            return yaml.Serialize(
                new
                {
                    groups = group != null ? new[] { group.Name } : new string[] { },
                    account = new
                    {
                        name = account.Owner.Name,
                        email = account.Owner.Email,
                        kerb = account.Owner.Kerberos,
                        iam = account.Owner.Iam,
                        mothra = account.Owner.MothraId,
                        key = account.SshKey
                    },
                    meta = new
                    {
                        cluster = account.Cluster.Name,
                    }
                }
            );

        }
    }
}