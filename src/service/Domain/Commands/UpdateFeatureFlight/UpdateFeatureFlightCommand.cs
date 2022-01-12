using CQRS.Mediatr.Lite;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Common.Model.AzureAppConfig;

namespace Microsoft.FeatureFlighting.Core.Commands
{
    /// <summary>
    /// Command to update a feature flight
    /// </summary>
    public class UpdateFeatureFlightCommand : Command<IdCommandResult>
    {
        public override string DisplayName => nameof(UpdateFeatureFlightCommand);

        private readonly string _id;
        public override string Id => _id;

        public AzureFeatureFlag AzureFeatureFlag { get; set; }
        public LoggerTrackingIds TrackingIds => new(CorrelationId, TransactionId);

        public UpdateFeatureFlightCommand(AzureFeatureFlag azureFeatureFlag, string correlationId, string transactionId)
        {
            AzureFeatureFlag = azureFeatureFlag;
            CorrelationId = correlationId;
            TransactionId = transactionId;
        }

        public override bool Validate(out string ValidationErrorMessage)
        {   
            if (AzureFeatureFlag == null)
            {
                ValidationErrorMessage = "Updated Feature flag value cannot be null";
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
