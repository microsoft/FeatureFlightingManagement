using System;
using AppInsights.EnterpriseTelemetry.Exceptions;

namespace Microsoft.FeatureFlighting.Common.AppExceptions
{
    /// <summary>
    /// Exception when validation fails due to client errors
    /// </summary>
    [Serializable]
    public class GraphException : BaseAppException
    {
        public override string Type { get => Constants.Exception.Types.GRAPH; }

        public GraphException(string message,
            string exceptionCode,
            string correlationId,
            string transactionId,
            string source,
            Exception innerException)
            : base(message, exceptionCode: exceptionCode, correlationId: correlationId, transactionId: transactionId, source: source, innerException: innerException)
        {
        }

        public GraphException(string message,
            string exceptionCode,
            string suggestedFix,
            string correlationId,
            string transactionId,
            string source)
            : this(message, exceptionCode, correlationId, transactionId, source, innerException: null)
        { }

        public GraphException(string message, string exceptionCode)
            : this(message, exceptionCode, Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), null, innerException: null)
        { }

        protected override string CreateDisplayMessage()
        {
            return string.Format(Constants.Exception.GraphException.DisplayMessage, Message, CorrelationId);
        }
    }
}
