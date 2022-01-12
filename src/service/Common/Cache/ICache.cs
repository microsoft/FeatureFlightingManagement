using System.Threading.Tasks;
using System.Collections.Generic;

namespace Microsoft.FeatureFlighting.Common.Caching
{
    /// <summary>
    /// Cache
    /// </summary>
    public interface ICache
    {
        /// <summary>
        /// Gets an item from cache
        /// </summary>
        /// <typeparam name="T">Type of object being retrieved</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="correlationId">Correlation ID of the operation</param>
        /// <param name="transactionId">Transaction ID of the operation</param>
        /// <returns>Cached object</returns>
        Task<T> Get<T>(string key, string correlationId, string transactionId);

        /// <summary>
        /// Gets a list of cache string (with a single key)
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <param name="correlationId">Correlation ID of the operation</param>
        /// <param name="transactionId">Transaction ID of the operation</param>
        /// <returns cref="IList{string}">List of cache string</returns>
        Task<IList<string>> GetList(string key, string correlationId, string transactionId);

        /// <summary>
        /// Caches an item
        /// </summary>
        /// <typeparam name="T">Type of object to be cached</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="value">Object to be cached</param>
        /// <param name="correlationId">Correlation ID of the operation</param>
        /// <param name="transactionId">Transaction ID of the operation</param>
        /// <param name="relativeExpirationMins">Mins after which the cache will be purged. -1 indicates forever caching.</param>        
        /// <returns></returns>
        Task Set<T>(string key, T value, string correlationId, string transactionId, int relativeExpirationMins = -1);

        /// <summary>
        /// Sets a list of string in the cache
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <param name="values">List of strings to be caches</param>
        /// <param name="correlationId">Correlation ID of the operation</param>
        /// <param name="transactionId">Transaction ID of the operation</param>
        /// <param name="relativeExpirationMins">Mins after which the cache will be purged.  -1 indicates forever caching.</param>        
        Task SetList(string key, IList<string> values, string correlationId, string transactionId, int relativeExpirationMins = -1);

        /// <summary>
        /// Deletes an item in the cache
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <param name="correlationId">Correlation ID of the operation</param>
        /// <param name="transactionId">Transaction ID of the operation</param>
        Task Delete(string key, string correlationId, string transactionId);
    }
}
