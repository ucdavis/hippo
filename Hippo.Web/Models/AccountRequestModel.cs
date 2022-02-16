namespace Hippo.Web.Models
{
    public class AccountRequestModel
    {
        public int SponsorId { get; set; }
        public string SponsorName { get; set; } = String.Empty; //As an extra check? Or just go with the SponsorId?
        public string SshKey { get; set; } = String.Empty;
    }
}
