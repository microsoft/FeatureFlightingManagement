using CQRS.Mediatr.Lite;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Common.Model;

namespace Microsoft.FeatureFlighting.Core.Queries
{
    /// <summary>
    /// Gets the <see cref="EvaluationMetricsDto"/> for a feature flight
    /// </summary>
    public class GetEvaluationMetricsQuery: Query<EvaluationMetricsDto>
    {
        public override string DisplayName => nameof(GetEvaluationMetricsQuery);

        private readonly string _id;
        public override string Id => _id;

        public string FeatureName { get; set; }
        public string Tenant { get; set; }
        public string Environment { get; set; }
        public int TimespanInDays { get; set; }

        public LoggerTrackingIds TrackingIds => new(CorrelationId, TransactionId);

        public GetEvaluationMetricsQuery(string featureName, string tenant, string environment, int timespanInDays, string correlationId, string transactionId)
        {
            FeatureName = featureName;
            Tenant = tenant;
            Environment = environment;
            TimespanInDays = timespanInDays;
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
