using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hippo.Core.Models.Email
{
    public class PendingDecisionsModel
    {

        public string UcdLogoUrl { get; set; } = String.Empty;
       
        public string ButtonText { get; set; } = "View Requests";
        public string ButtonUrl { get; set; } = "";

        public string ClusterName { get; set; } = "";
        public string GroupName { get; set; } = ""; //? don't know if I need it...

        public string Subject { get; set; } = "";
        public string Header { get; set; } = "";

    }
}
