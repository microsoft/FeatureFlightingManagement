using System;
using AppInsights.EnterpriseTelemetry.Exceptions;

namespace Microsoft.FeatureFlighting.Common.AppExceptions
{
    public class AzureRequestException : BaseAppException
    {
        public override string Type { get => Constants.Exception.Types.AZURERREQUESTEXCEPTION; }

        public AzureRequestException(string message,
            int statusCode,
            string correlationId,
            string transactionId,
            string source,
            Exception innerException)
            : base(message, exceptionCode: statusCode.ToString(), correlationId: correlationId, transactionId: transactionId, source: source, innerException: innerException)
        {
        }

        public AzureRequestException(string message,
            int statusCode,
            string correlationId,
            string transactionId,
            string source)
            : this(message, statusCode, correlationId, transactionId, source, innerException: null)
        { }

        protected override string CreateDisplayMessage()
        {
            return string.Format(Constants.Exception.AzureRequestException.DisplayMessage, CorrelationId);
        }
    }
}
