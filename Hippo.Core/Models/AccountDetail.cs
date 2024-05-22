using System.Text.Json;
using Hippo.Core.Domain;

namespace Hippo.Core.Models
{
    public class AccountDetail
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Owner { get; set; }
        public string Cluster { get; set; }
        public JsonElement? Data { get; set; }
        public List<GroupModel> MemberOfGroups { get; set; } = new();
        public List<GroupModel> AdminOfGroups { get; set; } = new();
    }
}