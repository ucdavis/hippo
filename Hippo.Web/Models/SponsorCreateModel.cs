using System.ComponentModel.DataAnnotations;

namespace Hippo.Web.Models
{
    public class SponsorCreateModel
    {
        [Required]
        public string Lookup { get;set;} = String.Empty; //Kerb or email
        public string Name { get;set;} = String.Empty; //Leave blank to use User's name

    }
}
