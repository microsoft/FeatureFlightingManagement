using System;
using CQRS.Mediatr.Lite;
using System.Collections.Generic;
using Microsoft.FeatureFlighting.Common;

namespace Microsoft.FeatureFlighting.Core.Commands
{
    /// <summary>
    /// Rebuilds a feature flag (re-optimizes)
    /// </summary>
    public class RebuildFlightsCommand : Command<RebuildCommandResult>
    {
        public override string DisplayName => nameof(RebuildFlightsCommand);

        private readonly string _id;
        public override string Id => _id;

        public List<string> FeatureNames { get; set; }
        public string Tenant { get; set; }
        public string Environment { get; set; }
        public string Reason { get; set; }
        public bool UpdateFromSource { get; set; }
        public string Source { get; set; }

        public LoggerTrackingIds TrackingIds => new(CorrelationId, TransactionId);

        public RebuildFlightsCommand(List<string> featureNames, string tenant, string environment, string reason, string correlationId, string transactionId, string source)
        {
            _id = Guid.NewGuid().ToString();
            FeatureNames = featureNames;
            Tenant = tenant;
            Environment = environment;
            Reason = reason;
            Source = source;
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
