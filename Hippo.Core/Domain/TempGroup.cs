using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Hippo.Core.Domain
{
    public class TempGroup
    {
        [Key]
        public string Group { get; set; } = "";
    }
}