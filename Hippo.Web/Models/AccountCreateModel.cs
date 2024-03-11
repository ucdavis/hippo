namespace Hippo.Web.Models
{
    public class AccountCreateModel
    {
        public int GroupId { get; set; }
        public string SshKey { get; set; } = String.Empty;
        public string SupervisingPI { get; set; } = String.Empty;
    }
}
