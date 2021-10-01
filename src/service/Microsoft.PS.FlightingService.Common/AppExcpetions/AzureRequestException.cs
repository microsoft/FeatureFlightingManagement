using System;

namespace Microsoft.PS.FlightingService.Common.AppExcpetions
{
    public class AzureRequestException : BaseAppException
    {
        public override string Type { get => Constants.Exception.Types.AZURERREQUESTEXCEPTION; }

        public AzureRequestException(string message,
            int statusCode,
            string correlationId,
            string transactionId,
            string failedMethod,
            Exception innerException)
            : base(message, exceptionCode: statusCode.ToString(), correlationId: correlationId, transactionId: transactionId, failedMethod: failedMethod, innerException: innerException)
        {
        }

        public AzureRequestException(string message,
            int statusCode,
            string correlationId,
            string transactionId,
            string failedMethod)
            : this(message, statusCode, correlationId, transactionId, failedMethod, innerException: null)
        { }

        protected override string CreateDisplayMessage()
        {
            return string.Format(Constants.Exception.AzureRequestException.DisplayMessage, CorrelationId);
        }
    }
}
