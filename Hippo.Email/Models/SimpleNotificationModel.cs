﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hippo.Email.Models
{
    public class SimpleNotificationModel
    {
        public string UcdLogoUrl { get; set; } = "";
        public string Subject { get; set; } = "";
        public string Header { get; set; } = "";
        public List<string> Paragraphs { get; set; } = new();
    }
}
