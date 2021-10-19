namespace Microsoft.FeatureFlighting.Common.Caching
{
    /// <summary>
    /// Factory for creating <see cref="ICache"/>
    /// </summary>
    public interface ICacheFactory
    {
        /// <summary>
        /// Creates <see cref="ICache"/> based on the tenant
        /// </summary>
        /// <param name="tenant">Tenant Name</param>
        /// <param name="correlationId">Correlation ID of the operation</param>
        /// <param name="transactionId">Transaction ID of the operation</param>
        /// <returns></returns>
        ICache Create(string tenant, string correlationId, string transactionId);

        /// <summary>
        /// Creates <see cref="ICache"/> based on the tenant and operation that has to be performed
        /// </summary>
        /// <param name="tenant">Tenant Name</param>
        /// <param name="operation">Operation for which cache is required</param>
        /// <param name="correlationId">Correlation ID of the operation</param>
        /// <param name="transactionId">Transaction ID of the operation</param>
        /// <returns></returns>
        ICache Create(string tenant, string operation, string correlationId, string transactionId);
    }
}
