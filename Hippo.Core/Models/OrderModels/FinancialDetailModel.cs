using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hippo.Core.Models.OrderModels
{
    public class FinancialDetailModel
    {
        public string FinancialSystemApiKey { get; set; }
        [Required]
        [MaxLength(50)]
        public string FinancialSystemApiSource { get; set; }
        public string ChartString { get; set; }
        public bool AutoApprove { get; set; }

        public string MaskedApiKey { get; set; }


    }
}
