using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hippo.Core.Domain
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }
        public string SlothId { get; set; }
        public string TrackingNumber { get; set; } // KFS tracking number
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public decimal Amount { get; set; }
        public string Status { get; set; }

        public string ClusterChartString { get; set; }
        public string OrderChartStrings { get; set; } //.Serialize() of the billings chart strings and percentages used


        [Required]
        public int OrderId { get; set; }
        public Order Order { get; set; }

        //Optional createdBy. If not set, was crated by a job
        public int? CreatedById { get; set; }
        public User CreatedBy { get; set; }
    }
}
