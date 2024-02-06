using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Hippo.Core.Domain;

namespace Hippo.Core.Models;

public class QueuedEventUpdateModel
{
    public int Id { get; set; }

    [Required]
    [RegularExpression(QueuedEvent.Statuses.RegexPattern)]
    [Description("The new status of the event. Marking it as 'Complete' will trigger Action-specific processing in Hippo.")]
    public string Status { get; set; } = "";
}
