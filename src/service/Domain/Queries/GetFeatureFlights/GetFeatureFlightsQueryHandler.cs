using System.Text;
using System.Linq;
using CQRS.Mediatr.Lite;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.FeatureFlighting.Core.Spec;
using Microsoft.FeatureFlighting.Common.Model;
using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.FeatureFlighting.Common.Storage;
using Microsoft.FeatureFlighting.Common.AppConfig;
using Microsoft.FeatureFlighting.Core.Domain.Assembler;
using Microsoft.FeatureFlighting.Common.Model.AzureAppConfig;

namespace Microsoft.FeatureFlighting.Core.Queries
{
    /// <summary>
    /// Handles <see cref="GetFeatureFlightQuery"/>. Gets feature flags for the given tenant from database or azure app config
    /// </summary>
    internal class GetFeatureFlightsQueryHandler : QueryHandler<GetFeatureFlightsQuery, IEnumerable<FeatureFlightDto>>
    {
        private readonly ITenantConfigurationProvider _tenantConfigurationProvider;
        private readonly IAzureFeatureManager _azureFeatureManager;
        private readonly IFlightsDbRepositoryFactory _flightDbRepositoryFactory;
        private readonly IFeatureFlightCache _cache;

        public GetFeatureFlightsQueryHandler(ITenantConfigurationProvider tenantConfigurationProvider,
            IAzureFeatureManager azureFeatureFlightManager,
            IFlightsDbRepositoryFactory flightsDbRepositoryFactory,
            IFeatureFlightCache cache)
        {
            _tenantConfigurationProvider = tenantConfigurationProvider;
            _azureFeatureManager = azureFeatureFlightManager;
            _flightDbRepositoryFactory = flightsDbRepositoryFactory;
            _cache = cache;
        }

        protected override async Task<IEnumerable<FeatureFlightDto>> ProcessRequest(GetFeatureFlightsQuery query)
        {
            TenantConfiguration tenantConfiguration = await _tenantConfigurationProvider.Get(query.Tenant);
            
            IEnumerable<FeatureFlightDto> featureFlights = await GetCachedFlights(query, tenantConfiguration);
            if (featureFlights != null)
                return featureFlights;
            
            featureFlights = await GetFlightsFromDb(query, tenantConfiguration);
            if (featureFlights != null)
                return featureFlights;
            
            return await GetFlightsFromAzure(query, tenantConfiguration);
        }

        private Task<IEnumerable<FeatureFlightDto>> GetCachedFlights(GetFeatureFlightsQuery query, TenantConfiguration tenantConfiguration)
        {
            return _cache.GetFeatureFlights(tenantConfiguration.Name, query.Environment, query.TrackingIds);
        }

        private async Task<IEnumerable<FeatureFlightDto>> GetFlightsFromDb(GetFeatureFlightsQuery query, TenantConfiguration tenantConfiguration)
        {
            IDocumentRepository<FeatureFlightDto> repository = await _flightDbRepositoryFactory.GetFlightsRepository(tenantConfiguration.Name);
            if (repository == null)
                return null;

            string getFlightsDbQuery = new StringBuilder()
                .Append("SELECT * FROM c WHERE c.Tenant = '")
                .Append(tenantConfiguration.Name)
                .Append("'")
                .Append(" AND c.Environment = '")
                .Append(query.Environment.ToLowerInvariant())
                .Append("'")
                .ToString();

            IEnumerable<FeatureFlightDto> featureFlights = await repository.QueryAll(getFlightsDbQuery, tenantConfiguration.Name, query.TrackingIds);
            if (featureFlights != null)
                await _cache.SetFeatureFlights(tenantConfiguration.Name, query.Environment, featureFlights, query.TrackingIds);
            return featureFlights;
        }

        private async Task<IEnumerable<FeatureFlightDto>> GetFlightsFromAzure(GetFeatureFlightsQuery query, TenantConfiguration tenantConfiguration)
        {
            IEnumerable<AzureFeatureFlag> azureFeatureFlags = (await _azureFeatureManager.Get(tenantConfiguration.Name, query.Environment)).ToList();
            if (azureFeatureFlags == null || !azureFeatureFlags.Any())
                return null;

            IEnumerable<FeatureFlightDto> featureFlights = azureFeatureFlags.Select(azureFlag => FeatureFlightDtoAssembler.Assemble(azureFlag)).ToList();
            if (featureFlights != null)
                await _cache.SetFeatureFlights(tenantConfiguration.Name, query.Environment, featureFlights, query.TrackingIds);
            return featureFlights;
        }
    }
}
