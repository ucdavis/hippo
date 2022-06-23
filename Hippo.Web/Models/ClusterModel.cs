using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Hippo.Core.Domain;

namespace Hippo.Web.Models
{
    public class ClusterModel
    {
        [Required]
        public Cluster Cluster { get; set; }
        public string SshKey { get; set; }

        [JsonConstructor]
        public ClusterModel(Cluster cluster, string sshKey)
        {
            Cluster = cluster;
            SshKey = sshKey;
        }
    }
}
