using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hippo.Core.Models.Email
{
    public class NewRequestModel
    {
        public string UcdLogoUrl { get; set; } = String.Empty;
        public string GroupName { get; set; } = String.Empty;
        public string RequesterName { get; set; } = String.Empty;
        public string ClusterName { get; set; } = "FARM/CAES";
        public string RequestDate { get; set; } = String.Empty;
        public string Instructions { get; set; } = "";
        public string RequestUrl { get; set; } = String.Empty;
        public string ButtonText { get; set; } = "View Request";
        public string SupervisingPI { get; set; } = String.Empty;
        public string Action { get; set; } = String.Empty;
        public List<string> AccessTypes { get; set; } = new();
    }
}
