namespace Hippo.Core.Models.Settings
{
    public class AggieEnterpriseSettings
    {
        public string GraphQlUrl { get; set; }
        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }
        public string TokenEndpoint { get; set; }
        public string ScopeApp { get; set; }
        public string ScopeEnv { get; set; }

        public string DebitNaturalAccount { get; set; } 
        public string CreditNaturalAccount { get; set; }
    }
}
