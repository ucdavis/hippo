using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Hippo.Core.Domain
{
    public class GroupAdminAccount
    {
        [Required]
        public int GroupId { get; set; }
        [JsonIgnore]
        public Group Group { get; set; }
        [Required]
        public int AccountId { get; set; }
        [JsonIgnore]
        public Account Account { get; set; }
    }
}
