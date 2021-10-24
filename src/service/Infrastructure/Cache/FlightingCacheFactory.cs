using System;
using System.Linq;
using System.Collections.Generic;
using AppInsights.EnterpriseTelemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.FeatureFlighting.Common.Caching;
using static Microsoft.FeatureFlighting.Common.Cache.CacheConstants;

namespace Microsoft.FeatureFlighting.Infrastructure.Cache
{
    /// <inheritdoc/>
    public class FlightingCacheFactory : ICacheFactory
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IConfiguration _configuration;
        private readonly ITenantConfigurationProvider _tenantConfigurationProvider;
        private readonly ILogger _logger;
        private static readonly Dictionary<string, ICache> _cacheContainer = new();
        private static readonly object _lock = new();

        public FlightingCacheFactory(IMemoryCache memoryCache, ITenantConfigurationProvider tenantConfigurationProvider, IConfiguration configuration, ILogger logger)
        {
            _memoryCache = memoryCache;
            _tenantConfigurationProvider = tenantConfigurationProvider;
            _configuration = configuration;
            _logger = logger;
        }

        /// <inheritdoc/>
        public ICache Create(string tenant, string correlationId, string transactionId) =>
            Create(tenant, string.Empty, correlationId, transactionId);

        /// <inheritdoc/>
        public ICache Create(string tenant, string operation, string correlationId, string transactionId)
        {
            lock (_lock)
            {
                var cacheContainerKey = $"{tenant.ToUpperInvariant()}:{operation.ToUpperInvariant()}";
                if (_cacheContainer.Keys.Contains(cacheContainerKey))
                    return _cacheContainer[cacheContainerKey];

                string cacheType = GetCacheType(tenant, operation);
                ICache cache = Create(cacheType, tenant);
                _cacheContainer.Add(cacheContainerKey, cache);
                return cache;
            }
        }

        /// <inheritdoc/>
        private ICache Create(string cacheType, string tenant)
        {   
            if (string.IsNullOrEmpty(cacheType))
                return null;

            return Enum.Parse<CacheType>(cacheType) switch
            {
                CacheType.NoCache => null,
                CacheType.InMemory => CreateInMemoryCache(tenant),
                CacheType.Redis => CreateRedisCache(tenant),
                _ => CreateUnifiedRedisCache(tenant),
            };
        }

        private string GetCacheType(string tenant, string operation)
        {
            TenantConfiguration tenantConfiguration = _tenantConfigurationProvider.Get(tenant).ConfigureAwait(false).GetAwaiter().GetResult();
            if (tenantConfiguration.Cache == null || string.IsNullOrWhiteSpace(tenantConfiguration.Cache.Type))
                return null;

            string cacheType = tenantConfiguration.Cache.GetCacheType(operation);
            return cacheType;
        }

        private ICache CreateInMemoryCache(string tenant) =>
            new InMemoryCache(_memoryCache, tenant, _logger);

        private ICache CreateRedisCache(string tenant)
        {
            TenantConfiguration tenantConfiguration = _tenantConfigurationProvider.Get(tenant).ConfigureAwait(false).GetAwaiter().GetResult();
            if (tenantConfiguration.Cache == null || tenantConfiguration.Cache.Redis == null)
                return null;

            string connectionString = _configuration.GetValue<string>(tenantConfiguration.Cache.Redis.ConnectionStringLocation);
            int timeout = tenantConfiguration.Cache.Redis.Timeout;
            return new RedisCache(connectionString, timeout, _logger);
        }

        private ICache CreateUnifiedRedisCache(string tenant)
        {
            TenantConfiguration tenantConfiguration = _tenantConfigurationProvider.Get(tenant).ConfigureAwait(false).GetAwaiter().GetResult();
            if (tenantConfiguration.Cache == null || tenantConfiguration.Cache.URP == null)
                return null;

            UrpConfiguration urp = tenantConfiguration.Cache.URP;
            string secret = _configuration.GetValue<string>(urp.SecretKey);
            return new UnifiedRedisCache(urp.Cluster, urp.App, secret, urp.Location, _logger);
        }
    }
}
