using System;
using CQRS.Mediatr.Lite;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Core.Operators;

namespace Microsoft.FeatureFlighting.Core.Queries
{
    /// <summary>
    /// Query to verify the veracity of a rule engine
    /// </summary>
    public class VerifyRulesEngineQuery : Query<EvaluationResult>
    {
        public override string DisplayName => nameof(VerifyRulesEngineQuery);

        private readonly string _id;
        public override string Id => _id;

        public string Tenant { get; set; }
        public string WorkflowName { get; set; }
        public string WorkflowPayload { get; set; }
        public string FlightContext { get; set; }
        public bool Debug { get; set; }

        public LoggerTrackingIds TrackingIds => new(CorrelationId, TransactionId);

        public VerifyRulesEngineQuery(string tenant, string workflowName, string workflowPayload, string flightContext, bool debug, string correlationId, string transactionId)
        {
            _id = Guid.NewGuid().ToString();
            Tenant = tenant;
            WorkflowName = workflowName;
            WorkflowPayload = workflowPayload;
            FlightContext = flightContext;
            Debug = debug;
            CorrelationId = correlationId;
            TransactionId = transactionId;
        }

        public override bool Validate(out string ValidationErrorMessage)
        {
            ValidationErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Tenant))
                ValidationErrorMessage += "Tenant cannot be null or empty";

            if (string.IsNullOrWhiteSpace(WorkflowName))
                ValidationErrorMessage += "Workflow Name cannot be null or empty";

            if (string.IsNullOrWhiteSpace(WorkflowPayload))
                ValidationErrorMessage += "Workflow Payload cannot be null or empty";

            if (string.IsNullOrWhiteSpace(FlightContext))
                ValidationErrorMessage += "Flight Context cannot be null or empty";

            return string.IsNullOrWhiteSpace(ValidationErrorMessage);
        }
    }
}
