using System;
using System.Threading.Tasks;
using AppInsights.EnterpriseTelemetry;
using System.Collections.Generic;
using AppInsights.EnterpriseTelemetry.Context;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.PS.FlightingService.Common.Caching;

namespace Microsoft.PS.FlightingService.Caching
{
    public class InMemoryCache : ICache
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

        public Task<List<string>> GetList(string key, string correlationId, string transactionId)
        {
            key = GetTenantKey(key);
            // NOTE: Get calls are not logged to avoid too much logging
            _memoryCache.TryGetValue(key, out List<string> cachedValues);
            if (cachedValues != null)
                return Task.FromResult(cachedValues);
            return Task.FromResult<List<string>>(null);
        }

        public Task SetList(string key, List<string> values, string correlationId, string transactionId, int relativeExpirationMins = -1)
        {
            key = GetTenantKey(key);
            var dependencyContext = new DependencyContext(CacheLogContext.GetMetadata("InMemory", "SET_LIST", key));
            dependencyContext.AddProperty("Relative Expiration", relativeExpirationMins.ToString());
            
            if (relativeExpirationMins > 0)
                _memoryCache.Set(key, values, TimeSpan.FromMinutes(relativeExpirationMins));
            else
                _memoryCache.Set(key, values);

            dependencyContext.CompleteDependency();
            _logger.Log(dependencyContext);
            return Task.CompletedTask;
        }

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
            $"{_tenant}_{key}";
    }
}
