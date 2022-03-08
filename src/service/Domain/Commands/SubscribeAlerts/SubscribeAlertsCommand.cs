using CQRS.Mediatr.Lite;
using Microsoft.FeatureFlighting.Common;

namespace Microsoft.FeatureFlighting.Core.Commands
{
    /// <summary>
    /// Command to subscribe alers for the feature flight
    /// </summary>
    public class SubscribeAlertsCommand : Command<IdCommandResult>
    {
        public override string DisplayName => nameof(SubscribeAlertsCommand);

        private readonly string _id;
        public override string Id => _id;

        public string FeatureName { get; set; }
        public string Tenant { get; set; }
        public string Environment { get; set; }
        public string Source { get; set; }

        public LoggerTrackingIds TrackingIds => new(CorrelationId, TransactionId);

        public SubscribeAlertsCommand(string featureName, string tenant, string environment, string correlationId, string transactionId, string source)
        {
            FeatureName = featureName;
            Tenant = tenant;
            Environment = environment;
            Source = source;
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
