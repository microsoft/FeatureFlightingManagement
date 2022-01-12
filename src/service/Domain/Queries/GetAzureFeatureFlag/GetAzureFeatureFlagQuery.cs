using CQRS.Mediatr.Lite;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Common.Model.AzureAppConfig;

namespace Microsoft.FeatureFlighting.Core.Queries
{
    /// <summary>
    /// Queries for an azure feature flag
    /// </summary>
    public class GetAzureFeatureFlagQuery : Query<AzureFeatureFlag?>
    {
        public override string DisplayName => nameof(GetAzureFeatureFlagQuery);

        private readonly string _id;
        public override string Id => _id;

        public string FeatureName { get; set; }
        public string TenantName { get; set; }
        public string EnvironmentName { get; set; }
        public LoggerTrackingIds TrackingIds => new(CorrelationId, TransactionId);

        public GetAzureFeatureFlagQuery(string featureName, string tenantName, string environmentName, string correlationId, string transactionId)
        {
            FeatureName = featureName;
            TenantName = tenantName;
            EnvironmentName = environmentName;
            CorrelationId = correlationId;
            TransactionId = transactionId;
        }

        public override bool Validate(out string ValidationErrorMessage)
        {
            ValidationErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(FeatureName))
                ValidationErrorMessage = "Feature name cannot be null or empty | ";
            if (string.IsNullOrWhiteSpace(TenantName))
                ValidationErrorMessage = "Tenant name cannot be null or empty | ";
            if (string.IsNullOrWhiteSpace(EnvironmentName))
                ValidationErrorMessage = "Environment cannot be null or empty | ";

            return string.IsNullOrWhiteSpace(ValidationErrorMessage);
        }
    }
}
