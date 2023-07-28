using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Hippo.Core.Domain
{
    public class PuppetUser
    {
        [Key]
        [MaxLength(20)]
        public string Kerberos { get; set; }

        public List<PuppetGroup> Groups { get; set; } = new();
    }
}
