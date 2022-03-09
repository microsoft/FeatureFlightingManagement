using System;
using CQRS.Mediatr.Lite;
using Microsoft.FeatureFlighting.Common;

namespace Microsoft.FeatureFlighting.Core.Commands
{
    /// <summary>
    /// Generates usage report for a tenant
    /// </summary>
    public class GenerateReportCommand : Command<ReportCommandResult>
    {
        public override string DisplayName => nameof(GenerateReportCommand);

        public override string Id { get; }

        public string Tenant { get; set; }
        public string Environment { get; set; }
        public bool TriggerAlert { get; set; }

        public LoggerTrackingIds TrackingIds => new(CorrelationId, TransactionId);

        public GenerateReportCommand(string tenant, string environment, bool triggerAlert, string correlationId, string transactionId)
        {
            Id = Guid.NewGuid().ToString();
            Tenant = tenant;
            Environment = environment;
            TriggerAlert = triggerAlert;
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
