namespace Microsoft.PS.FlightingService.Domain.FeatureFilters
{
    public class FilterSettings
    {
        public string Operator { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string IsActive { get; set; } = "false";
        public string StageId { get; set; } = "-1";
        public string StageName { get; set; } = string.Empty;
        public string FlightContextKey { get; set; } = string.Empty;
    }

    public class SecurityGroup
    {
        public string Name { get; set; } = string.Empty;

        public string ObjectId { get; set; } = string.Empty;
    }
}
