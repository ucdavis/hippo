using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hippo.Email.Models
{
    public class DecisionModel
    {
        public string UcdLogoUrl { get; set; } = "https://harvest.caes.ucdavis.edu/media/caes-logo-gray.png";
        public string SponsorName { get; set; } = String.Empty;
        public string RequesterName { get; set; } = String.Empty;
        public string ClusterName { get; set; } = "FARM/CAES";
        public string RequestDate { get; set; } = String.Empty;
        public string DecisionDate { get; set; } = String.Empty; 
        public string Instructions { get; set; } = "Your account request has been approved. Once it has been processed you will have access. You can check View Account to see the current status of your account.";
        public string RequestUrl { get; set; } = String.Empty;
        public string ButtonText { get; set; } = "View Account";

        public string Decision { get; set; } = String.Empty ;
        public string DecisionColor { get; set; } = Colors.Approved;

        public class Colors
        {
            public const string Approved = "#266041";
            public const string Rejected = "#cf3c3c";

        }
    }
}
