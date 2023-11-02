using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Hippo.Core.Domain
{
    public class TempKerberos
    {
        [Key]
        public string Kerberos { get; set; } = "";
    }
}