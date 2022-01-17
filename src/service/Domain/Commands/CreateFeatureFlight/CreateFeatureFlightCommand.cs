using System;
using CQRS.Mediatr.Lite;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Common.Model.AzureAppConfig;

namespace Microsoft.FeatureFlighting.Core.Commands
{
    /// <summary>
    /// Command to create a new feature flag
    /// </summary>
    public class CreateFeatureFlightCommand : Command<IdCommandResult>
    {
        public override string DisplayName => nameof(CreateFeatureFlightCommand);

        private readonly string _id;
        public override string Id => _id;

        public AzureFeatureFlag AzureFeatureFlag { get; set; }

        public LoggerTrackingIds TrackingIds => new(CorrelationId, TransactionId);

        public CreateFeatureFlightCommand(AzureFeatureFlag flag, string correlationId, string transactionId)
        {
            _id = Guid.NewGuid().ToString();
            AzureFeatureFlag = flag;
            CorrelationId = correlationId ?? Guid.NewGuid().ToString();
            TransactionId = transactionId ?? Guid.NewGuid().ToString();
        }

        public override bool Validate(out string ValidationErrorMessage)
        {
            if (AzureFeatureFlag == null)
            {
                ValidationErrorMessage = "New Feature flag value cannot be null";
                return false;
            }

            if (!AzureFeatureFlag.IsValid(out string flagValidationError))
            {
                ValidationErrorMessage = flagValidationError;
                return false;
            }
            ValidationErrorMessage = null;
            return true;
        }
    }
}
