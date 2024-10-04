using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Hippo.Core.Validation;

namespace Hippo.Core.Models.Email
{
    public class SimpleNotificationModel
    {
        [Required]
        [ListOfEmailAddress]
        public string[] Emails { get; set; } = Array.Empty<string>();
        [Required]
        [ListOfEmailAddress]
        public string[] CcEmails { get; set; } = Array.Empty<string>();
        [Required]
        public string Subject { get; set; } = "";
        public string Header { get; set; } = "";
        [Required]
        public List<string> Paragraphs { get; set; } = new();
        [JsonIgnore]
        public string UcdLogoUrl { get; set; } = "";
    }
}
