using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hippo.Email.Views.Shared
{
    public class EmailButtonModel
    {
        public EmailButtonModel(string text, string url)
        {
            Text = text;
            Url = url;
        }

        public string Text { get; set; }
        public string Url { get; set; }
    }
}
