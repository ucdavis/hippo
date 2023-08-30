using Hippo.Core.Domain;

namespace Hippo.Core.Models
{
    public class AccountDetail
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public string Owner { get; set; }
        public string Cluster { get; set; }
        public List<string> Groups { get; set; } = new();
    }
}