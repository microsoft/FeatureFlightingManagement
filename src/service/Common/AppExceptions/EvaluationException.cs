using System;
using AppInsights.EnterpriseTelemetry.Exceptions;

namespace Microsoft.FeatureFlighting.Common.AppExceptions
{
    /// <summary>
    /// Exception when there is an error in evaluating a feature flag
    /// </summary>
    [Serializable]
    public class EvaluationException : BaseAppException
    {
        public override string Type { get => Constants.Exception.Types.DOMAIN; }

        public EvaluationException(string message,
            string correlationId,
            string transactionId,
            string source,
            Exception innerException)
            : base(message, exceptionCode: Constants.Exception.EvaluationException.ExceptionCode, correlationId: correlationId, transactionId: transactionId, source: source, innerException: innerException)
        { }

        protected override string CreateDisplayMessage()
        {
            return string.Format(Constants.Exception.EvaluationException.DisplayMessage, Message, CorrelationId);
        }
    }
}
