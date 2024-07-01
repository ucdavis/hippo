using AggieEnterpriseApi.Types;
using AggieEnterpriseApi.Validation;

namespace Hippo.Core.Models
{
    public class ChartStringValidationModel
    {
        public bool IsValid { get; set; } = true;

        public string ChartString { get; set; }
        public FinancialChartStringType ChartType { get; set; }
        public GlSegments GlSegments { get; set; }
        public PpmSegments PpmSegments { get; set; }

        public string AccountManager { get; set; }
        public string AccountManagerEmail { get; set; }
        public string Description { get; set; } //Description of COA



        /// <summary>
        /// Return Segment info.
        /// </summary>
        public List<KeyValuePair<string, string>> Details { get; set; } = new List<KeyValuePair<string, string>>();
        public List<KeyValuePair<string, string>> Warnings { get; set; } = new List<KeyValuePair<string, string>>();
        public string Message
        {
            get
            {
                if (Messages.Count <= 0)
                {
                    return string.Empty;
                }

                return string.Join(" ", Messages);
            }
        }
        public List<string> Messages { get; set; } = new List<string>();

        public string Warning
        {
            get
            {
                if (Warnings.Count <= 0)
                {
                    return string.Empty;
                }

                //select the warnings into a string of key - Value and return it
                return string.Join(" ", Warnings.Select(w => $"{w.Key} - {w.Value}"));

            }
        }
    }
}
