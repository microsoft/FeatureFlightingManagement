using System;

namespace Microsoft.FeatureFlighting.Common.AppExcpetions
{
    [Serializable]
    public class EvaluationException : BaseAppException
    {
        public override string Type { get => Constants.Exception.Types.DOMAIN; }

        public EvaluationException(string message,
            string correlationId,
            string transactionId,
            string failedMethod,
            Exception innerException)
            : base(message, exceptionCode: Constants.Exception.EvaluationException.ExceptionCode, correlationId: correlationId, transactionId: transactionId, failedMethod: failedMethod, innerException: innerException)
        { }

        protected override string CreateDisplayMessage()
        {
            return string.Format(Constants.Exception.EvaluationException.DisplayMessage, Message, CorrelationId);
        }
    }
}
