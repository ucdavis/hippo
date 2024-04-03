using System.ComponentModel.DataAnnotations;
using Hippo.Core.Domain;
using Hippo.Core.Validation;

namespace Hippo.Web.Models
{
    public class AccountCreateModel
    {
        public int GroupId { get; set; }
        public string SshKey { get; set; } = String.Empty;
        [MaxLength(100)]
        public string SupervisingPI { get; set; } = String.Empty;
        [RegularExpressionList(AccessType.Codes.RegexPattern)]
        public List<string> AccessTypes { get; set; } = new();
    }
}
