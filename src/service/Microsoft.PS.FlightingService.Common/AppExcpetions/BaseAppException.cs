using System;

namespace Microsoft.PS.FlightingService.Common.AppExcpetions
{
    /// <summary>
    /// Base exception for all Application Specific Exception
    /// </summary>
    [Serializable]
    public abstract class BaseAppException : Exception
    {
        /// <summary>
        /// Exception Type (General/Domain/Infrastructure)
        /// </summary>
        public abstract string Type { get; }
        public string DisplayMessage => CreateDisplayMessage();
        /// <summary>
        /// Name of the Exception
        /// </summary>
        public virtual string Name { get; protected set; }

        public string CorrelationId { get; protected set; }
        public string TransactionId { get; protected set; }
        public string ExceptionCode { get; protected set; }
        public string FailedMethod { get; protected set; }

        /// <summary>
        /// Default constructor for exception
        /// </summary>
        public BaseAppException() : base() { }

        public BaseAppException(string message,
            Exception innerException = null,
            string correlationId = "",
            string transactionId = "",
            string exceptionCode = "",
            string failedMethod = "")
            : base(message, innerException)
        {
            CorrelationId = correlationId;
            TransactionId = transactionId;
            ExceptionCode = exceptionCode;
            FailedMethod = failedMethod;
        }

        protected abstract string CreateDisplayMessage();
    }
}
