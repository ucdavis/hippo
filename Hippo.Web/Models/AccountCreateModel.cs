using System.ComponentModel.DataAnnotations;
using Hippo.Core.Domain;
using Hippo.Core.Validation;

namespace Hippo.Web.Models
{
    public class AccountCreateModel
    {
        public DateTime? AcceptableUsePolicyAgreedOn { get; set; }
        public int GroupId { get; set; }
        public string SshKey { get; set; } = String.Empty;
        [MaxLength(100)]
        public string SupervisingPI { get; set; } = String.Empty;
        public string SupervisingPIIamId { get; set; } = String.Empty;
        [ListOfStringsOptions(AccessType.Codes.RegexPattern, nonEmpty: true)]
        public List<string> AccessTypes { get; set; } = new();
    }
}
