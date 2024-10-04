using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hippo.Core.Models.Email
{
    public class EmailInnerDetailsModel
    {
        public EmailInnerDetailsModel(string heading, string text)
        {
            Heading = heading;
            Text = text;
        }

        public string Heading { get; set; } = String.Empty;
        public string Text { get; set; } = String.Empty;
    }
}
