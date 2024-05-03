using AggieEnterpriseApi;
using AggieEnterpriseApi.Extensions;
using AggieEnterpriseApi.Validation;
using Hippo.Core.Models;
using Hippo.Core.Models.Settings;
using Microsoft.Extensions.Options;
using Serilog;

namespace Hippo.Core.Services
{
    public interface IAggieEnterpriseService
    {
        Task<ChartStringValidationModel> IsChartStringValid(string chartString, bool validateCVRs = true);
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

        public async Task<ChartStringValidationModel> IsChartStringValid(string chartString, bool validateCVRs = true)
        {
            var rtValue = new ChartStringValidationModel();
            rtValue.IsValid = false;
            rtValue.ChartString = chartString;

            var segmentStringType = FinancialChartValidation.GetFinancialChartStringType(chartString);
            rtValue.ChartType = segmentStringType;


            if (segmentStringType == FinancialChartStringType.Gl)
            {
                chartString = ReplaceNaturalAccount(chartString, AeSettings.NaturalAccount);
                rtValue.ChartString = chartString;

                var result = await _aggieClient.GlValidateChartstring.ExecuteAsync(chartString, validateCVRs);

                var data = result.ReadData();

                rtValue.IsValid = data.GlValidateChartstring.ValidationResponse.Valid;

                if (!rtValue.IsValid)
                {
                    foreach (var err in data.GlValidateChartstring.ValidationResponse.ErrorMessages)
                    {
                        rtValue.Messages.Add(err);
                    }
                }
                rtValue.Details.Add(new KeyValuePair<string, string>("Entity", $"{data.GlValidateChartstring.SegmentNames.EntityName} ({data.GlValidateChartstring.Segments.Entity})"));
                rtValue.Details.Add(new KeyValuePair<string, string>("Fund", $"{data.GlValidateChartstring.SegmentNames.FundName} ({data.GlValidateChartstring.Segments.Fund})"));
                rtValue.Details.Add(new KeyValuePair<string, string>("Department", $"{data.GlValidateChartstring.SegmentNames.DepartmentName} ({data.GlValidateChartstring.Segments.Department})"));
                rtValue.Details.Add(new KeyValuePair<string, string>("Account", $"{data.GlValidateChartstring.SegmentNames.AccountName} ({data.GlValidateChartstring.Segments.Account})"));
                rtValue.Details.Add(new KeyValuePair<string, string>("Purpose", $"{data.GlValidateChartstring.SegmentNames.PurposeName} ({data.GlValidateChartstring.Segments.Purpose})"));
                rtValue.Details.Add(new KeyValuePair<string, string>("Project", $"{data.GlValidateChartstring.SegmentNames.ProjectName} ({data.GlValidateChartstring.Segments.Project})"));
                rtValue.Details.Add(new KeyValuePair<string, string>("Program", $"{data.GlValidateChartstring.SegmentNames.ProgramName} ({data.GlValidateChartstring.Segments.Program})"));
                rtValue.Details.Add(new KeyValuePair<string, string>("Activity", $"{data.GlValidateChartstring.SegmentNames.ActivityName} ({data.GlValidateChartstring.Segments.Activity})"));

                if (data.GlValidateChartstring.Warnings != null)
                {
                    foreach (var warn in data.GlValidateChartstring.Warnings)
                    {
                        rtValue.Warnings.Add(new KeyValuePair<string, string>(warn.SegmentName, warn.Warning));
                    }
                }

                rtValue.GlSegments = FinancialChartValidation.GetGlSegments(chartString);

                rtValue.Description = $"{data.GlValidateChartstring.SegmentNames.DepartmentName} - {data.GlValidateChartstring.SegmentNames.FundName}";

                if (!string.IsNullOrWhiteSpace(AeSettings.NaturalAccount))
                {
                    if (rtValue.GlSegments.Account != AeSettings.NaturalAccount)
                    {
                        rtValue.Messages.Add($"Natural Account must be {AeSettings.NaturalAccount}");
                        rtValue.IsValid = false;
                    }
                }

                return rtValue;
            }

            if (segmentStringType == FinancialChartStringType.Ppm)
            {
                chartString = ReplaceNaturalAccount(chartString, AeSettings.NaturalAccount);
                rtValue.ChartString = chartString;
                var result = await _aggieClient.PpmSegmentStringValidate.ExecuteAsync(chartString);

                var data = result.ReadData();

                rtValue.IsValid = data.PpmSegmentStringValidate.ValidationResponse.Valid;
                if (!rtValue.IsValid)
                {
                    foreach (var err in data.PpmSegmentStringValidate.ValidationResponse.ErrorMessages)
                    {
                        rtValue.Messages.Add(err);
                    }
                }

                rtValue.Details.Add(new KeyValuePair<string, string>("Project", data.PpmSegmentStringValidate.Segments.Project));
                rtValue.Details.Add(new KeyValuePair<string, string>("Task", data.PpmSegmentStringValidate.Segments.Task));
                rtValue.Details.Add(new KeyValuePair<string, string>("Organization", data.PpmSegmentStringValidate.Segments.Organization));
                rtValue.Details.Add(new KeyValuePair<string, string>("Expenditure Type", data.PpmSegmentStringValidate.Segments.ExpenditureType));
                rtValue.Details.Add(new KeyValuePair<string, string>("Award", data.PpmSegmentStringValidate.Segments.Award));
                rtValue.Details.Add(new KeyValuePair<string, string>("Funding Source", data.PpmSegmentStringValidate.Segments.FundingSource));

                if (data.PpmSegmentStringValidate.Warnings != null)
                {
                    foreach (var warn in data.PpmSegmentStringValidate.Warnings)
                    {
                        rtValue.Warnings.Add(new KeyValuePair<string, string>(warn.SegmentName, warn.Warning));
                    }
                }

                rtValue.PpmSegments = FinancialChartValidation.GetPpmSegments(chartString);


                await GetPpmAccountManager(rtValue);
                if (!string.IsNullOrWhiteSpace(AeSettings.NaturalAccount))
                {

                    if (rtValue.PpmSegments.ExpenditureType != AeSettings.NaturalAccount)
                    {
                        rtValue.Messages.Add($"Expenditure Type must be {AeSettings.NaturalAccount}");
                        rtValue.IsValid = false;
                    }
                }

                return rtValue;
            }

            rtValue.IsValid = false; //Just in case.
            rtValue.Messages.Add("Invalid Aggie Enterprise COA format");

            return rtValue;
        }

        public string ReplaceNaturalAccount(string chartString, string naturalAccount)
        {
            if(string.IsNullOrWhiteSpace(naturalAccount))
            {
                return chartString;
            }

            var segmentStringType = FinancialChartValidation.GetFinancialChartStringType(chartString);
            if (segmentStringType == FinancialChartStringType.Gl)
            {
                var segments = FinancialChartValidation.GetGlSegments(chartString);
                segments.Account = naturalAccount;
                return segments.ToSegmentString();
            }
            if (segmentStringType == FinancialChartStringType.Ppm)
            {
                var segments = FinancialChartValidation.GetPpmSegments(chartString);
                segments.ExpenditureType = naturalAccount;
                return segments.ToSegmentString();
            }
            Log.Error($"Invalid financial segment string: {chartString}");
            return chartString;
        }
        private async Task GetPpmAccountManager(ChartStringValidationModel rtValue)
        {
            var result = await _aggieClient.PpmProjectManager.ExecuteAsync(rtValue.PpmSegments.Project);

            var data = result.ReadData();

            if (data.PpmProjectByNumber?.ProjectNumber == rtValue.PpmSegments.Project)
            {
                rtValue.AccountManager = data.PpmProjectByNumber.PrimaryProjectManagerName;
                rtValue.AccountManagerEmail = data.PpmProjectByNumber.PrimaryProjectManagerEmail;
                rtValue.Description = data.PpmProjectByNumber.Name;
            }
            return;
        }
    }

}
