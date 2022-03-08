using CQRS.Mediatr.Lite;
using System.Collections.Generic;
using Microsoft.FeatureFlighting.Common;

namespace Microsoft.FeatureFlighting.Core.Queries
{
    /// <summary>
    /// Gets a collection of all feature names for the given tenant and environment
    /// </summary>
    public class GetFeatureNamesQuery : Query<IEnumerable<string>>
    {
        public override string DisplayName => nameof(GetFeatureNamesQuery);

        private readonly string _id;
        public override string Id => _id;

        public string Tenant { get; set; }
        public string Environment { get; set; }
        public LoggerTrackingIds TrackingIds => new(CorrelationId, TransactionId);

        public GetFeatureNamesQuery(string tenant, string environment, string correlationId, string transactionId)
        {
            Tenant = tenant;
            Environment = environment;
            CorrelationId = correlationId;
            TransactionId = transactionId;
        }

        public override bool Validate(out string ValidationErrorMessage)
        {
            ValidationErrorMessage = string.Empty;
            if (string.IsNullOrWhiteSpace(Tenant))
                ValidationErrorMessage = "Tenant cannot be null or empty | ";
            if (string.IsNullOrWhiteSpace(Environment))
                ValidationErrorMessage = "Environment cannot be null or empty";

            return string.IsNullOrWhiteSpace(ValidationErrorMessage);
        }
    }
}
