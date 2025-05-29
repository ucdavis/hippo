using Hippo.Core.Data;
using Hippo.Core.Domain;
using Hippo.Core.Models;
using Serilog;
using YamlDotNet.Serialization;

namespace Hippo.Core.Services;

/// <summary>
/// Temporary implementation of IAccountUpdateService that sends yaml files for backwards compatibility
/// </summary>
[Obsolete("AccountUpdateYamlService is deprecated and will be removed in a future release.")]
public class AccountUpdateYamlService : IAccountUpdateService
{
    private readonly ISshService _sshService;
    private readonly AppDbContext _dbContext;

    public AccountUpdateYamlService(ISshService sshService, AppDbContext dbContext)
    {
        _sshService = sshService;
        _dbContext = dbContext;
    }

    public async Task<Result> QueueEvent(QueuedEvent queuedEvent)
    {
        var queuedEventModel = QueuedEventModel.FromQueuedEvent(queuedEvent);
        var kerberos = queuedEventModel.Data.Accounts.Select(a => a.Kerberos).Single();
        try
        {
            var connectionInfo = await _dbContext.Clusters.GetSshConnectionInfo(queuedEventModel.Data.Cluster);
            var tempFileName = $"/var/lib/remote-api/.{kerberos}.yaml";
            var fileName = $"/var/lib/remote-api/{kerberos}.yaml";
            var yaml = GetYaml(queuedEventModel);
            await _sshService.PlaceFile(yaml, tempFileName, connectionInfo);
            await _sshService.RenameFile(tempFileName, fileName, connectionInfo);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error sending account update yaml to remote api");
            return Result.Error("Error sending account update yaml to remote api");
        }
    }

    private string GetYaml(QueuedEventModel queuedEventModel)
    {
        var account = queuedEventModel.Data.Accounts.Single();
        var group = queuedEventModel.Data.Groups.SingleOrDefault();
        var serializer = new Serializer();
        return serializer.Serialize(
            string.IsNullOrWhiteSpace(account.Key)
                ? new
                {
                    groups = group != null ? new[] { group.Name } : new string[] { },
                    account = new
                    {
                        name = account.Name,
                        email = account.Email,
                        kerb = account.Kerberos,
                        iam = account.Iam,
                        mothra = account.Mothra,
                        // no key in this request, so excluding it from yaml
                    },
                    meta = new
                    {
                        cluster = queuedEventModel.Data.Cluster
                    }
                }
                : new
                {
                    groups = group != null ? new[] { group.Name } : new string[] { },
                    account = new
                    {
                        name = account.Name,
                        email = account.Email,
                        kerb = account.Kerberos,
                        iam = account.Iam,
                        mothra = account.Mothra,
                        key = account.Key
                    },
                    meta = new
                    {
                        cluster = queuedEventModel.Data.Cluster
                    }
                }
        );
    }

    public Task<Result> UpdateEvent(QueuedEvent queuedEvent, string status)
    {
        // no QueuedEvents to be updated in this implementation
        throw new NotImplementedException();
    }
}