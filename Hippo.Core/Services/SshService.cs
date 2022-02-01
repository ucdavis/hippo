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
    }

    public class SshService : ISshService
    {
        private readonly SshSettings _sshSettings;
        private readonly PrivateKeyFile? _pkFile;

        public SshService(IOptions<SshSettings> sshSettings)
        {
            _sshSettings = sshSettings.Value;
            var rsa = Convert.FromBase64String(_sshSettings.Key);
            var stream = new MemoryStream(rsa);
            _pkFile = new PrivateKeyFile(stream);
        }

        public IEnumerable<string> Test()
        {
            using (var client = new SshClient(_sshSettings.Url, _sshSettings.Name, _pkFile))
            {
                client.Connect();
                var result = client.RunCommand("ls -l");
                client.Disconnect();
                return result.Result.Split('\n');
            }
        }
    }

}
