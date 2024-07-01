using System.ComponentModel.DataAnnotations;

namespace Hippo.Core.Models;

public class SoftwareRequestModel
{
    [Required]
    public string ClusterName { get; set; }
    [Required]
    public string Email { get; set; }
    [Required]
    public string AccountName { get; set; }
    [Required]
    public string SoftwareTitle { get; set; }
    [Required]
    public string SoftwareLicense { get; set; }
    [Required]
    public string SoftwareHomePage { get; set; }
    [Required]
    public string BenefitDescription { get; set; }
    public string AdditionalInformation { get; set; }
}
