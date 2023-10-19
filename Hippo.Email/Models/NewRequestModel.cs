using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hippo.Email.Models
{
    public class NewRequestModel
    {
        public string UcdLogoUrl { get; set; } = "https://hippo-test.azurewebsites.net/media/caes-logo-gray.png";
        public string GroupName { get; set; } = String.Empty;
        public string RequesterName { get; set; } = String.Empty;
        public string ClusterName { get; set; } = "FARM/CAES";
        public string RequestDate { get; set; } = String.Empty;
        public string Instructions { get; set; } = "As a group admin who can sponsor new accounts and group memberships on this cluster, you have received this request. Please click on the View Request button to approve or deny this request.";
        public string RequestUrl { get; set; } = String.Empty;
        public string ButtonText { get; set; } = "View Request";
    }
}
