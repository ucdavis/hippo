using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hippo.Core.Models.SlothModels
{
    public class SlothResponseModel
    {
        public string Id { get; set; }
        public string KfsTrackingNumber { get; set; }
        public string Status { get; set; }
    }

    public static class SlothStatuses
    {
        public const string PendingApproval = "PendingApproval";
        public const string Scheduled = "Scheduled";
        public const string Completed = "Completed";
        public const string Cancelled = "Cancelled";

        //Probably not going to do anything with these, just here for completeness 
        public const string Processing = "Processing";
        public const string Rejected = "Rejected";
    }
}
