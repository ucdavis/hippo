using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hippo.Email.Models
{
    public class EmailOrderPaymentModel
    {

        public string UcdLogoUrl { get; set; } = String.Empty;
       
        public string ButtonText { get; set; } = "View Order";
        public string ButtonUrl { get; set; } = "";

        public string Subject { get; set; } = "";
        public string Header { get; set; } = "";
        public List<EmailTransferResponseModel> Transfers { get; set; } = new();

        public string TotalAmount => Transfers.Where(a => a.Direction == "Debit").Sum(t => t.Amount).ToString("C");

    }

    public class EmailTransferResponseModel
    {
        public Decimal Amount { get; set; }
        public string FinancialSegmentString { get; set; } = "";
        public string Direction { get; set; } = "";
    }
}
