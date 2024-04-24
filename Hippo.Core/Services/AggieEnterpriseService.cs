using AggieEnterpriseApi;
using AggieEnterpriseApi.Extensions;
using AggieEnterpriseApi.Types;
using AggieEnterpriseApi.Validation;
using Hippo.Core.Models.Settings;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hippo.Core.Services
{
    public interface IAggieEnterpriseService
    {
        //Task<AccountValidationModel> IsAccountValid(string financialSegmentString, bool validateCVRs = true);
    }
    public class AggieEnterpriseService : IAggieEnterpriseService
    {
        private IAggieEnterpriseClient _aggieClient;
        public AggieEnterpriseSettings AeSettings { get; set; }

        public AggieEnterpriseService(IOptions<AggieEnterpriseSettings> aggieEnterpriseSettings)
        {
            AeSettings = aggieEnterpriseSettings.Value;

            try
            {
                //_aggieClient = GraphQlClient.Get(AeSettings.GraphQlUrl, AeSettings.Token);
                _aggieClient = GraphQlClient.Get(AeSettings.GraphQlUrl, AeSettings.TokenEndpoint, AeSettings.ConsumerKey, AeSettings.ConsumerSecret, $"{AeSettings.ScopeApp}-{AeSettings.ScopeEnv}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating Aggie Enterprise Client");
                Log.Information("Aggie Enterprise Scope {scope}", $"{AeSettings.ScopeApp}-{AeSettings.ScopeEnv}");
                _aggieClient = null;
            }
        }
    }

}
