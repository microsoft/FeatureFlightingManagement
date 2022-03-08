namespace Microsoft.FeatureFlighting.Common.Model.AzureAppConfig
{
    public class AzureFilterParameters
    {
        public string Operator { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string IsActive { get; set; } = "false";
        public string StageId { get; set; } = "-1";
        public string StageName { get; set; } = string.Empty;
        public string FlightContextKey { get; set; } = string.Empty;
    }
}
