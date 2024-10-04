using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hippo.Core.Models.Email
{
    public class DecisionModel
    {
        public string RequestedAction { get; set; } = String.Empty;
        public string UcdLogoUrl { get; set; } = String.Empty;
        public string GroupName { get; set; } = String.Empty;
        public string RequesterName { get; set; } = String.Empty;
        public string ClusterName { get; set; } = "FARM/CAES";
        public string RequestDate { get; set; } = String.Empty;
        public string DecisionDate { get; set; } = String.Empty;
        public string RequestUrl { get; set; } = String.Empty;
        public string ButtonText { get; set; } = "Visit Hippo";

        public string Decision { get; set; } = String.Empty;
        public string DecisionColor { get; set; } = Colors.Approved;
        public string DecisionDetails { get; set; } = String.Empty;
        public string AdminName { get; set; } = String.Empty; //Used for admin override email
        public List<string> AccessTypes { get; set; } = new();
        public string SupervisingPI { get; set; } = String.Empty;

        public class Colors
        {
            public const string Approved = "#481268";
            public const string Rejected = "#cf3c3c";

        }
    }
}
