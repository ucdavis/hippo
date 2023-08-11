using System.ComponentModel.DataAnnotations;

namespace Hippo.Web.Models
{
    public class AddGroupAdminModel
    {
        [Required]
        public string Lookup { get; set; } = String.Empty; //Kerb or email
        public string Group { get; set; } = String.Empty;

    }
}
