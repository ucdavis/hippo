using System;

namespace Hippo.Core.Models.Settings
{
    public class AzureSettings
    {       
        public string TenantName { get; set; }
        public string TentantId { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string KeyVaultUrl { get; set; }
    }
}
