using Microsoft.FeatureFlighting.Common.Caching;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.FeatureFlighting.Common.Cache
{
    public interface IFeatureFlightResultCacheFactory
    {
        /// <summary>
        /// Creates <see cref="ICache"/> based on the tenant and operation that has to be performed
        /// </summary>
        /// <param name="tenant">Tenant Name</param>
        /// <param name="operation">Operation for which cache is required</param>
        /// <param name="correlationId">Correlation ID of the operation</param>
        /// <param name="transactionId">Transaction ID of the operation</param>
        /// <returns cref="ICache">Cache</returns>
        ICache Create(string tenant, string operation, string correlationId, string transactionId);
    }
}
