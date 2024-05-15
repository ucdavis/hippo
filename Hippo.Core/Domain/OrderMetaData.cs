using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Hippo.Core.Domain
{
    public class OrderMetaData
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int OrderId { get; set; }
        [JsonIgnore]
        public Order Order { get; set; }
        [Required]
        [MaxLength(128)]
        public string Name { get; set; }

        [Required]
        [MaxLength(450)]
        public string Value { get; set; }

        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrderMetaData>().HasIndex(a => a.OrderId);
            modelBuilder.Entity<OrderMetaData>().HasIndex(a => new {a.OrderId, a.Name, a.Value });
        }
    }
}
