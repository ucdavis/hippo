using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Text.Json;
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

        public JsonElement? Data { get; set; }
    }

    public class AccountRequestDataModel
    {
        public DateTime? AcceptableUsePolicyAgreedOn { get; set; }
        [MaxLength(100)]
        public string SupervisingPI { get; set; } = "";
        public int? SupervisingPIUserId { get; set; }
        public string SshKey { get; set; } = "";
        [ListOfStringsOptions(AccessType.Codes.RegexPattern, nonEmpty: true)]
        public List<string> AccessTypes { get; set; } = new();

        public static List<string> ValidActions = new List<string>
        {
            Request.Actions.CreateAccount,
            Request.Actions.AddAccountToGroup 
        };
    }

    public class GroupRequestDataModel {
        [MaxLength(32)]
        public string Name { get; set; } = "";
        [MaxLength(100)]
        public string DisplayName { get; set; } = "";

        public static List<string> ValidActions = new List<string>
        {
            Request.Actions.CreateGroup,
        };
    }    
}