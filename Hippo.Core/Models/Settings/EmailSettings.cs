using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hippo.Core.Models.Settings
{
    public class EmailSettings
    {
        public string ApiKey { get; set; }
        public string DisableSend { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string BccEmail { get; set; }
        public string FromEmail { get; set; }
        public string FromName { get; set; }
    }
}
