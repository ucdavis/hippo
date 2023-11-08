using System.Linq.Expressions;
using Hippo.Core.Data;
using Hippo.Core.Domain;

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
        public string SupervisingPI { get; set; } = "";

        public GroupModel GroupModel { get; set; } = new();
    }
}