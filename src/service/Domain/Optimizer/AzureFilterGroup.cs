using Microsoft.FeatureFlighting.Common.Model.AzureAppConfig;

namespace Microsoft.FeatureFlighting.Core.Optimizer
{
    public class AzureFilterGroup
    {
        public string ContextKey { get; set; }
        public AzureFilter Filter { get; set; }
    }
}
