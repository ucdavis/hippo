using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hippo.Core.Models.Settings
{
    public class SshSettings
    {
        public string Name { get; set; }
        public string Key { get; set; }
        public string Url { get; set; }
    }
}
