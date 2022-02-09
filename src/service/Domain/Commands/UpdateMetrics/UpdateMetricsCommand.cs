using CQRS.Mediatr.Lite;
using Microsoft.FeatureFlighting.Common;

namespace Microsoft.FeatureFlighting.Core.Commands
{
    public class UpdateMetricsCommand : Command<MetricsCommandResult>
    {
        public override string DisplayName => nameof(UpdateMetricsCommand);

        private readonly string _id;
        public override string Id => _id;

        public string FeatureName { get; set; }
        public string Tenant { get; set; }
        public string Environment { get; set; }
        public string Source { get; set; }
        public int TimespanInDays { get; set; }

        public LoggerTrackingIds TrackingIds => new(CorrelationId, TransactionId);

        public UpdateMetricsCommand(string featureName, string tenant, string environment, int timespanInDays, string correlationId, string transactionId, string source)
        {
            FeatureName = featureName;
            Tenant = tenant;
            Environment = environment;
            TimespanInDays = timespanInDays;
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
            if (TimespanInDays <= 0)
                ValidationErrorMessage = "Timespan cannot be zero or negative";

            return string.IsNullOrWhiteSpace(ValidationErrorMessage);
        }
    }
}
