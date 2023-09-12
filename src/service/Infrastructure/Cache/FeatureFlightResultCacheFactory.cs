using AppInsights.EnterpriseTelemetry;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Common.Cache;
using Microsoft.FeatureFlighting.Common.Caching;
using Microsoft.FeatureFlighting.Common.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.FeatureFlighting.Infrastructure.Cache
{
    public class FeatureFlightResultCacheFactory : IFeatureFlightResultCacheFactory
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger _logger;
        private static readonly Dictionary<string, ICache> _cacheContainer = new();
        private static readonly object _lock = new();

        public FeatureFlightResultCacheFactory(IMemoryCache memoryCache, ILogger logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
        }

        ICache IFeatureFlightResultCacheFactory.Create(string tenant, string operation, string correlationId, string transactionId)
        {
            lock (_lock)
            {
                var cacheContainerKey = $"{tenant.ToUpperInvariant()}:{operation.ToUpperInvariant()}";
                if (_cacheContainer.Keys.Contains(cacheContainerKey))
                    return _cacheContainer[cacheContainerKey];

                ICache cache = new InMemoryCache(_memoryCache, tenant, _logger);
                _cacheContainer.Add(cacheContainerKey, cache);
                return cache;
            }
        }
    }
}
