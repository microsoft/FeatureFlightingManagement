using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using AppInsights.EnterpriseTelemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Common.Model;
using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.FeatureFlighting.Common.Storage;

namespace Microsoft.FeatureFlighting.Infrastructure.Storage
{
    public class FlightsDbRepositoryFactory : IFlightsDbRepositoryFactory
    {
        private readonly ITenantConfigurationProvider _tenantConfigurationProvider;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly IDictionary<string, IDocumentRepository<FeatureFlightDto>?> _documentRepositoryCache;

        public FlightsDbRepositoryFactory(ITenantConfigurationProvider tenantConfigurationProvider, IConfiguration configuration, ILogger logger)
        {
            _tenantConfigurationProvider = tenantConfigurationProvider;
            _configuration = configuration;
            _logger = logger;
            _documentRepositoryCache = new ConcurrentDictionary<string, IDocumentRepository<FeatureFlightDto>?>();
        }

        public async Task<IDocumentRepository<FeatureFlightDto>?> GetFlightsRepository(string tenantName)
        {
            if (_documentRepositoryCache.ContainsKey(tenantName.ToUpperInvariant()))
                return _documentRepositoryCache[tenantName.ToUpperInvariant()];

            TenantConfiguration tenantConfiguration = await _tenantConfigurationProvider.Get(tenantName);
            CosmosDbConfiguration flightsDbConfiguration = tenantConfiguration.FlightsDatabase;
            if (flightsDbConfiguration == null || flightsDbConfiguration.Disabled)
            {
                _documentRepositoryCache.Add(tenantName.ToUpperInvariant(), null);
            }

            IDocumentRepository<FeatureFlightDto> flightsDb = new CosmosDbRepository<FeatureFlightDto>(flightsDbConfiguration, _configuration, _logger);
            _documentRepositoryCache.Add(tenantName.ToUpperInvariant(), flightsDb);
            return flightsDb;
        }
    }
}
