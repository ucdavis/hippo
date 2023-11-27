namespace Hippo.Web.Models
{
    public class AccountSshKeyModel
    {
        public int AccountId { get; set; }
        public string SshKey { get; set; } = String.Empty;
    }
}
