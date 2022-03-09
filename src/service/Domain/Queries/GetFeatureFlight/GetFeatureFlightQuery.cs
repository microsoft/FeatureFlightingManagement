using CQRS.Mediatr.Lite;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Common.Model;

namespace Microsoft.FeatureFlighting.Core.Queries
{
    /// <summary>
    /// Gets <see cref="FeatureFlightDto" /> from Storage
    /// </summary>
    public class GetFeatureFlightQuery : Query<FeatureFlightDto>
    {
        public override string DisplayName => nameof(GetFeatureFlightQuery);

        private readonly string _id;
        public override string Id => _id;

        public string FeatureName { get; set; }
        public string Tenant { get; set; }
        public string Environment { get; set; }
        public LoggerTrackingIds TrackingIds => new(CorrelationId, TransactionId);

        public GetFeatureFlightQuery(string featureName, string tenant, string environment, string correlationId, string transactionId)
        {
            FeatureName = featureName;
            Tenant = tenant;
            Environment = environment;
            CorrelationId = correlationId;
            TransactionId = transactionId;
        }

        public override bool Validate(out string ValidationErrorMessage)
        {
            ValidationErrorMessage = string.Empty;
            if (string.IsNullOrWhiteSpace(FeatureName))
                ValidationErrorMessage = "Feature name cannot be null or empty | ";
            if (string.IsNullOrWhiteSpace(Tenant))
                ValidationErrorMessage = "Tenant cannot be null or empty | ";
            if (string.IsNullOrWhiteSpace(Environment))
                ValidationErrorMessage = "Environment cannot be null or empty";

            return string.IsNullOrWhiteSpace(ValidationErrorMessage);
        }
    }
}
