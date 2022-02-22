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
        [Required]
        [MaxLength(50)]
        public string Status { get; set; }
        
        public int? ActorId { get;set;}
        public User Actor { get; set; }
        public int AccountId { get; set; }
        public Account Account { get; set; }


        public class Actions
        {
            public const string Requested = "Requested";
            public const string Approved = "Approved";
            public const string Rejected = "Rejected";
            public const string AdminApproved = "Admin Approved";
            public const string AdminRejected = "Admin Rejected";
            public const string Other = "Other";

            public static List<string> TypeList = new List<string>
            {
                Requested,
                Approved,
                Rejected,
                AdminApproved,
                AdminRejected,
                Other,
            }.ToList();
        }

    }
}
