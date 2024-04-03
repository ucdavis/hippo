using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using Hippo.Core.Data;
using Hippo.Core.Domain;
using Hippo.Core.Validation;

namespace Hippo.Core.Models
{
    public class RequestModel
    {
        public int Id { get; set; }
        public string Action { get; set; } = "";
        public string RequesterEmail { get; set; } = "";
        public string RequesterName { get; set; } = "";
        public string Status { get; set; } = "";
        public string Cluster { get; set; } = "";

        public GroupModel GroupModel { get; set; } = new();

        public string Data { get; set; }
    }

    public class AccountRequestDataModel
    {
        [MaxLength(100)]
        public string SupervisingPI { get; set; } = "";
        public string SshKey { get; set; } = "";
        [RegularExpressionList(AccessType.Codes.RegexPattern)]
        public List<string> AccessTypes { get; set; } = new();

        public static List<string> ValidActions = new List<string>
        {
            Request.Actions.CreateAccount,
            Request.Actions.AddAccountToGroup 
        };
    }
}