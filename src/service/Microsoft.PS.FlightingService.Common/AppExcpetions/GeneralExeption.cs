using System;

namespace Microsoft.PS.FlightingService.Common.AppExcpetions
{
    /// <summary>
    /// Represents any general exception in the system
    /// </summary>
    [Serializable]
    public class GeneralException : BaseAppException
    {
        public GeneralException(Exception innerException = null,
            string correlationId = "",
            string transactionId = "",
            string failedMethod = "")
            : base(Constants.Exception.GeneralException.ExceptionMessage,
                 innerException: innerException,
                 correlationId: correlationId, transactionId: transactionId, failedMethod: failedMethod,
                 exceptionCode: Constants.Exception.GeneralException.ExceptionCode)
        { }

        public override string Type => Constants.Exception.Types.GENERAL;

        protected override string CreateDisplayMessage()
        {
            return string.Format(Constants.Exception.GeneralException.DisplayMessage, CorrelationId);
        }
    }
}
