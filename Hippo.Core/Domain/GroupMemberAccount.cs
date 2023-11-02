using System;
using System.ComponentModel.DataAnnotations;

namespace Hippo.Core.Domain
{
    public class GroupMemberAccount
    {
        [Required]
        public int GroupId { get; set; }
        public Group Group { get; set; }
        [Required]
        public int AccountId { get; set; }
        public Account Account { get; set; }
    }
}
