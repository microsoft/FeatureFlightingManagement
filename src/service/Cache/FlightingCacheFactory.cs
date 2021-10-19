using System;
using System.Linq;
using AppInsights.EnterpriseTelemetry;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Common.Caching;

namespace Microsoft.FeatureFlighting.Caching
{
    public class FlightingCacheFactory : ICacheFactory
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private static readonly Dictionary<string, ICache> _cacheContainer = new Dictionary<string, ICache>();
        private static readonly object _lock = new object();

        public FlightingCacheFactory(IMemoryCache memoryCache, IConfiguration configuration, ILogger logger)
        {
            _memoryCache = memoryCache;
            _configuration = configuration;
            _logger = logger;
        }

        public ICache Create(string tenant, string correlationId, string transactionId) =>
            Create(tenant, null, correlationId, transactionId);

        public ICache Create(string tenant, string operation, string correlationId, string transactionId)
        {
            var cacheContainerKey = $"{tenant.ToUpperInvariant()}:{operation.ToUpperInvariant()}";
            if (_cacheContainer.Keys.Contains(cacheContainerKey))
                return _cacheContainer[cacheContainerKey];

            lock (_lock)
            {
                var cacheType = GetCacheType(tenant, operation);
                var cache = cacheType switch
                {
                    CacheConstants.CacheType.NoCache => null,
                    CacheConstants.CacheType.InMemory => CreateInMemoryCache(tenant),
                    CacheConstants.CacheType.Redis => CreateRedisCache(tenant),
                    _ => CreateUnifiedRedisCache(tenant),
                };
                _cacheContainer.AddOrUpdate(cacheContainerKey, cache);
                return cache;
            }
        }

        private CacheConstants.CacheType GetCacheType(string tenant, string operation)
        {
            var formattedTenantName = Utility.GetFormattedTenantName(tenant);
            var defaultCacheType = _configuration.GetValue<string>("Tenants:Default:Caching:Type");
            var defaultOperationCacheType = string.IsNullOrWhiteSpace(operation) ? null : _configuration.GetValue<string>($"Tenants:Default:Caching:{operation.ToLowerInvariant()}");
            defaultCacheType = string.IsNullOrWhiteSpace(defaultOperationCacheType) ? defaultCacheType : defaultOperationCacheType;

            var tenantCacheType = _configuration.GetValue<string>($"Tenants:{formattedTenantName}:Caching:Type");
            var tenantOperationCacheType = string.IsNullOrWhiteSpace(operation) ? null : _configuration.GetValue<string>($"Tenants:{formattedTenantName}:Caching:{operation.ToLowerInvariant()}");
            tenantCacheType = string.IsNullOrWhiteSpace(tenantOperationCacheType) ? tenantCacheType : tenantOperationCacheType;

            var cacheTypeStr = string.IsNullOrWhiteSpace(tenantCacheType) ? defaultCacheType : tenantCacheType;
            return Enum.Parse<CacheConstants.CacheType>(cacheTypeStr);
        }

        private ICache CreateInMemoryCache(string tenant) =>
            new InMemoryCache(_memoryCache, tenant, _logger);

        private ICache CreateRedisCache(string tenant) 
        {
            var formattedTenantName = Utility.GetFormattedTenantName(tenant);
            var connectionString = _configuration.GetValue<string>(_configuration.GetValue<string>($"Tenants:{formattedTenantName}:Caching:ConnectionString"));
            var timeout = _configuration.GetValue<int>(_configuration.GetValue<string>($"Tenants:{formattedTenantName}:Caching:Timeout"));
            return new RedisCache(connectionString, timeout, _logger);
        }

        private ICache CreateUnifiedRedisCache(string tenant)
        {
            var unifiedRedisConfiguration = GetUnifiedRedisConfiguration(tenant);
            return new UnifiedRedisCache(unifiedRedisConfiguration["Cluster"], unifiedRedisConfiguration["App"], unifiedRedisConfiguration["Secret"], unifiedRedisConfiguration["Location"], _logger);
        }

        private Dictionary<string, string> GetUnifiedRedisConfiguration(string tenant)
        {
            var formattedTenantName = Utility.GetFormattedTenantName(tenant);

            var defaultUnifiedCluster = _configuration.GetValue<string>("Tenants:Default:Caching:Cluster");
            var defaultUnifiedApp = _configuration.GetValue<string>("Tenants:Default:Caching:App");
            var defaultUnifiedLocation = _configuration.GetValue<string>("Tenants:Default:Caching:Location");
            var defaultUnifiedAppKey = _configuration.GetValue<string>(_configuration.GetValue<string>("Tenants:Default:Caching:Secret"));

            var tenantUnifiedCluster = _configuration.GetValue<string>($"Tenants:{formattedTenantName}:Caching:Cluster");
            var tenantUnifiedApp = _configuration.GetValue<string>($"Tenants:{formattedTenantName}:Caching:App");
            var tenantUnifiedLocation = _configuration.GetValue<string>($"Tenants:{formattedTenantName}:Caching:Location");
            var tenantUnifiedAppKey = string.IsNullOrWhiteSpace(_configuration.GetValue<string>($"Tenants:{formattedTenantName}:Caching:Secret")) 
                ? defaultUnifiedAppKey 
                : _configuration.GetValue<string>(_configuration.GetValue<string>($"Tenants:{formattedTenantName}:Caching:Secret"));

            return new Dictionary<string, string>
            {
                { "Cluster", tenantUnifiedCluster ?? defaultUnifiedCluster },
                { "App", tenantUnifiedApp ?? defaultUnifiedApp },
                { "Location", tenantUnifiedLocation ?? defaultUnifiedLocation },
                { "Secret", tenantUnifiedAppKey ?? defaultUnifiedAppKey }
            };
        }
    }
}
