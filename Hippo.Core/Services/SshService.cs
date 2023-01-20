using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Renci.SshNet;
using Microsoft.Extensions.Options;
using Hippo.Core.Models.Settings;
using Hippo.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace Hippo.Core.Services
{
    public interface ISshService
    {
        Task<IEnumerable<string>> Test(SshConnectionInfo connectionInfo);
        Task PlaceFile(string contents, string path, SshConnectionInfo connectionInfo);
        Task RenameFile(string origPath, string newPath, SshConnectionInfo connectionInfo);
        Task<MemoryStream> DownloadFile(string fileName, SshConnectionInfo connectionInfo);
    }

    public class SshService : ISshService
    {
        private readonly ISecretsService _secretsService;
        private PrivateKeyFile _pkFile = null;

        public SshService(ISecretsService secretsService)
        {
            _secretsService = secretsService;
        }

        public async Task PlaceFile(string contents, string path, SshConnectionInfo connectionInfo)
        {
            using var client = await GetScpClient(connectionInfo);
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(contents));
            client.Upload(ms, path);
        }

        public async Task<IEnumerable<string>> Test(SshConnectionInfo connectionInfo)
        {
            using var client = await GetSshClient(connectionInfo);
            var result = client.RunCommand("ls -l"); // ls -alR
            return result.Result.Split('\n');
        }

        private async Task<PrivateKeyFile> GetPrivateKeyFile(string keyId)
        {
            if (keyId == null)
            {
                throw new ArgumentNullException(nameof(keyId));
            }

            if (_pkFile != null)
            {
                return _pkFile;
            }

            var key = await _secretsService.GetSecret(keyId);
            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(key)))
            {
                _pkFile = new PrivateKeyFile(stream);
            }

            return _pkFile;
        }

        // for running shell commands
        private async Task<SshClient> GetSshClient(SshConnectionInfo connectionInfo)
        {
            var pkFile = await GetPrivateKeyFile(connectionInfo.KeyId);
            var client = new SshClient(connectionInfo.Url, connectionInfo.Name, pkFile);
            client.Connect();
            return client;
        }

        // for file transfer
        private async Task<ScpClient> GetScpClient(SshConnectionInfo connectionInfo)
        {
            var pkFile = await GetPrivateKeyFile(connectionInfo.KeyId);
            var client = new ScpClient(connectionInfo.Url, connectionInfo.Name, pkFile);
            client.Connect();
            return client;
        }

        public async Task<MemoryStream> DownloadFile(string fileName, SshConnectionInfo connectionInfo)
        {
            using var client = await GetScpClient(connectionInfo);
            var stream = new MemoryStream();
            client.Download(fileName, stream );

            return stream;
        }

        public async Task RenameFile(string origPath, string newPath, SshConnectionInfo connectionInfo)
        {
            using var client = await GetSshClient(connectionInfo);
            var result = client.RunCommand($"mv \"{origPath}\" \"{newPath}\"");
        }
    }

    public class SshConnectionInfo
    {
        public string Url { get; set; }
        public string Name { get; set; }
        public string KeyId { get; set; }
    }

    public static class ClusterExtensions
    {
        public static async Task<SshConnectionInfo> GetSshConnectionInfo(this IQueryable<Cluster> clusters, string clusterName)
        {
            if (clusters == null)
            {
                throw new ArgumentNullException(nameof(clusters));
            }

            if (clusterName == null)
            {
                throw new ArgumentNullException(nameof(clusterName));
            }

            var connectionInfo = await clusters.Where(c => c.Name == clusterName).Select(c =>
                new SshConnectionInfo
                {
                    Url = c.SshUrl,
                    Name = c.SshName,
                    KeyId = c.SshKeyId
                }).SingleOrDefaultAsync();

            if (connectionInfo == null)
            {
                throw new ArgumentException($"No cluster found with name {clusterName}");
            }

            return connectionInfo;
        }
    }
}
