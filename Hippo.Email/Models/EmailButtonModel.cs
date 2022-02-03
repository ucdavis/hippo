using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hippo.Email.Models
{
    public class EmailButtonModel
    {
        public EmailButtonModel(string text, string url)
        {
            Text = text;
            Url = url;
        }

        public string Text { get; set; } = String.Empty;
        public string Url { get; set; } = String.Empty;
    }
}
