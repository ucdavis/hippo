namespace Hippo.Core.Models
{


    public class PuppetDetails
    {
        public List<string> Groups { get; set; } = new();
        public List<PuppetUser> Users { get; set; } = new();
    }

    public class PuppetUser
    {
        public string Kerberos { get; set; }
        public List<string> Groups { get; set; } = new();
    }
}