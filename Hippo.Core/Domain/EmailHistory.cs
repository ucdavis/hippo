using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hippo.Core.Domain
{
    public class EmailHistory
    {
        [Key]
        public int Id { get; set; }
        public bool IsSuccess { get; set; }
        public DateTime CreatedOn { get; set; }
        [Required]
        [MaxLength(250)]
        public string EmailTo { get; set; }
        [MaxLength(250)]
        public string EmailCC { get; set; }
        [Required]
        [MaxLength(200)]        
        public string Subject { get; set; }
        [Required]
        public string Body { get; set; }
        public string AltText { get; set; }
        public string ErrorText { get; set; }
        
    }
}
