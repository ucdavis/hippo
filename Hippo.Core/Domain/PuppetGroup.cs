using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Hippo.Core.Domain
{
    public class PuppetGroup
    {
        [Key]
        [MaxLength(32)]
        public string Name { get; set; }
        public List<PuppetUser> Users { get; } = new();
    }
}
