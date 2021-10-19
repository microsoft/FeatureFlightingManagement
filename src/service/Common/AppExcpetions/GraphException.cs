using System;

namespace Microsoft.FeatureFlighting.Common.AppExcpetions
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
            string failedMethod,
            Exception innerException)
            : base(message, exceptionCode: exceptionCode, correlationId: correlationId, transactionId: transactionId, failedMethod: failedMethod, innerException: innerException)
        {
        }

        public GraphException(string message,
            string exceptionCode,
            string suggestedFix,
            string correlationId,
            string transactionId,
            string failedMethod)
            : this(message, exceptionCode, correlationId, transactionId, failedMethod, innerException: null)
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
