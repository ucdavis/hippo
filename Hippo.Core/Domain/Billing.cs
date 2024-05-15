using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Hippo.Core.Domain
{
    public class Billing
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string ChartString { get; set; }
        public decimal Percentage { get; set; } = 100;
        [Required]
        public int OrderId { get; set; }
        [JsonIgnore]
        public Order Order { get; set; }
        public DateTime Updated { get; set; } = DateTime.UtcNow;
    }
}
