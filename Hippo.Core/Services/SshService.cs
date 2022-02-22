using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Renci.SshNet;
using Microsoft.Extensions.Options;
using Hippo.Core.Models.Settings;

namespace Hippo.Core.Services
{
    public interface ISshService
    {
        IEnumerable<string> Test();
        void PlaceFile(string contents, string path);
        void RenameFile(string origPath, string newPath);

        MemoryStream DownloadFile(string fileName);
    }

    public class SshService : ISshService
    {
        private readonly SshSettings _sshSettings;
        private readonly PrivateKeyFile _pkFile;

        public SshService(IOptions<SshSettings> sshSettings)
        {
            _sshSettings = sshSettings.Value;
            using (var stream = new MemoryStream(Convert.FromBase64String(_sshSettings.Key)))
            {
                _pkFile = new PrivateKeyFile(stream);
            }
        }

        public void PlaceFile(string contents, string path)
        {
            using var client = GetScpClient();
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(contents));
            client.Upload(ms, path);
        }

        public IEnumerable<string> Test()
        {
            using var client = GetSshClient();
            var result = client.RunCommand("ls -l"); // ls -alR
            return result.Result.Split('\n');
        }

        // for running shell commands
        private SshClient GetSshClient()
        {
            var client = new SshClient(_sshSettings.Url, _sshSettings.Name, _pkFile);
            client.Connect();
            return client;
        }

        // for file transfer
        private ScpClient GetScpClient()
        {
            var client = new ScpClient(_sshSettings.Url, _sshSettings.Name, _pkFile);
            client.Connect();
            return client;
        }

        public MemoryStream DownloadFile(string fileName)
        {
            using var client = GetScpClient();
            var stream = new MemoryStream();
            client.Download(fileName, stream );

            return stream;
        }

        public void RenameFile(string origPath, string newPath)
        {
            using var client = GetSshClient();
            var result = client.RunCommand($"mv \"{origPath}\" \"{newPath}\"");
        }
    }
}
