namespace Microsoft.FeatureFlighting.Common
{
    /// <summary>
    /// Tracking IDs
    /// </summary>
    public class LoggerTrackingIds
    {
        /// <summary>
        /// Correlation ID of the operation
        /// </summary>
        public string CorrelationId { get; set; }

        /// <summary>
        /// Transaction ID of the operation
        /// </summary>
        public string TransactionId { get; set; }

        public LoggerTrackingIds() { }

        public LoggerTrackingIds(string correlationId, string transactionId)
        {
            CorrelationId = correlationId;
            TransactionId = transactionId;
        }
    }
}
