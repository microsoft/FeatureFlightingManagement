namespace Microsoft.FeatureFlighting.Common.Model.AzureAppConfig
{
    public class AzureCustomFeatureFlag: AzureFeatureFlag
    {   
        public AzureFlagInsights Insights { get; set; }
    }
}
