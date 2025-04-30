namespace Hippo.Web.Models
{
    public class AddToGroupModel
    {
        public int GroupId { get; set; }
        public string SupervisingPI { get; set; } = String.Empty;
        public string SupervisingPIIamId { get; set; } = String.Empty;
    }
}
