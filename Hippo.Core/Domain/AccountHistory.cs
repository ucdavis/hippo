using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Hippo.Core.Domain
{
    public class AccountHistory
    {
        public AccountHistory()
        {
            CreatedOn = DateTime.UtcNow;
        }

        [Key]
        public int Id { get; set; }

        public DateTime CreatedOn { get; set; }
        [Required]
        [MaxLength(500)]
        public string Action { get; set; }
        public int? ActorId { get;set;}
        public User Actor { get; set; }
        public int AccountId { get; set; }
        public Account Account { get; set; }


    }
}
