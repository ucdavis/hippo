using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hippo.Core.Models.Email
{
    public class OrderNotificationModel
    {

        public string UcdLogoUrl { get; set; } = String.Empty;
       
        public string ButtonText { get; set; } = "View Order";
        public string ButtonUrl { get; set; } = "";

        public string Subject { get; set; } = "";
        public string Header { get; set; } = "";
        public List<string> Paragraphs { get; set; } = new();

    }
}
