using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using AppInsights.EnterpriseTelemetry;
using Microsoft.Extensions.Caching.Memory;
using AppInsights.EnterpriseTelemetry.Context;
using Microsoft.FeatureFlighting.Common.Caching;

namespace Microsoft.FeatureFlighting.Infrastructure.Cache
{
    /// <summary>
    /// In Memory cache (wrapper over <see cref="IMemoryCache"/>)
    /// </summary>
    internal class InMemoryCache : ICache
    {
        private readonly IMemoryCache _memoryCache;
        private readonly string _tenant;
        private readonly ILogger _logger;

        public InMemoryCache(IMemoryCache memoryCache, string tenant, ILogger logger)
        {
            _memoryCache = memoryCache;
            _tenant = tenant;
            _logger = logger;
        }

        
        /// <inheritdoc/>
        public Task<T> Get<T>(string key, string correlationId, string transactionId)
        {
            key = GetTenantKey(key);
            _memoryCache.TryGetValue(key, out T result);
            return Task.FromResult(result);
        }

        /// <inheritdoc/>
        public Task<IList<string>> GetList(string key, string correlationId, string transactionId)
        {
            key = GetTenantKey(key);
            // NOTE: Get calls are not logged to avoid too much logging
            _memoryCache.TryGetValue(key, out IList<string> cachedValues);
            if (cachedValues != null)
                return Task.FromResult(cachedValues);
            return Task.FromResult<IList<string>>(null);
        }

        /// <inheritdoc/>
        public Task Set<T>(string key, T value, string correlationId, string transactionId, int relativeExpirationMins = -1)
        {
            key = GetTenantKey(key);
            DependencyContext dependencyContext = new(CacheLogContext.GetMetadata("InMemory", "SET_ITEM", key));
            dependencyContext.AddProperty("Relative Expiration", relativeExpirationMins.ToString());
            dependencyContext.CorrelationId = correlationId;
            dependencyContext.TransactionId = correlationId;

            if (relativeExpirationMins > 0)
                _memoryCache.Set<T>(key, value, TimeSpan.FromMinutes(relativeExpirationMins));
            else
                _memoryCache.Set<T>(key, value);

            dependencyContext.CompleteDependency();
            _logger.Log(dependencyContext);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task SetList(string key, IList<string> values, string correlationId, string transactionId, int relativeExpirationMins = -1)
        {
            key = GetTenantKey(key);
            DependencyContext dependencyContext = new(CacheLogContext.GetMetadata("InMemory", "SET_LIST", key));
            dependencyContext.AddProperty("Relative Expiration", relativeExpirationMins.ToString());
            dependencyContext.CorrelationId = correlationId;
            dependencyContext.TransactionId = correlationId;

            if (relativeExpirationMins > 0)
                _memoryCache.Set(key, values, TimeSpan.FromMinutes(relativeExpirationMins));
            else
                _memoryCache.Set(key, values);

            dependencyContext.CompleteDependency();
            _logger.Log(dependencyContext);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task Delete(string key, string correlationId, string transactionId)
        {
            key = GetTenantKey(key);
            var dependencyContext = new DependencyContext(CacheLogContext.GetMetadata("InMemory", "DELETE", key));
            _memoryCache.Remove(key);
            dependencyContext.CompleteDependency();
            _logger.Log(dependencyContext);
            return Task.CompletedTask;
        }

        private string GetTenantKey(string key) =>
            $"{_tenant}_{key}".ToUpperInvariant();

        public Task<IList<T>> GetListObject<T>(string key, string correlationId, string transactionId)
        {
            key = GetTenantKey(key);
            // NOTE: Get calls are not logged to avoid too much logging
            _memoryCache.TryGetValue(key, out IList<T> cachedValues);
            if (cachedValues != null)
                return Task.FromResult(cachedValues);
            return Task.FromResult<IList<T>>(null);
        }

        public Task SetListObjects<T>(string key, IList<T> values, string correlationId, string transactionId, int relativeExpirationMins = -1)
        {
            key = GetTenantKey(key);
            DependencyContext dependencyContext = new(CacheLogContext.GetMetadata("InMemory", "SET_LIST", key));
            dependencyContext.AddProperty("Relative Expiration", relativeExpirationMins.ToString());
            dependencyContext.CorrelationId = correlationId;
            dependencyContext.TransactionId = correlationId;

            if (relativeExpirationMins > 0)
                _memoryCache.Set(key, values, TimeSpan.FromMinutes(relativeExpirationMins));
            else
                _memoryCache.Set(key, values);

            dependencyContext.CompleteDependency();
            _logger.Log(dependencyContext);
            return Task.CompletedTask;
        }
    }
}
