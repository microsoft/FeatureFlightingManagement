using CQRS.Mediatr.Lite;
using System.Threading.Tasks;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Common.Model;
using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.FeatureFlighting.Common.Storage;
using Microsoft.FeatureFlighting.Common.AppConfig;
using Microsoft.FeatureFlighting.Core.Domain.Assembler;
using Microsoft.FeatureFlighting.Common.Model.AzureAppConfig;

namespace Microsoft.FeatureFlighting.Core.Queries
{
    /// <summary>
    /// Handles <see cref="GetFeatureFlightQuery"/>. Gets feature flag from db or azure (fallback)
    /// </summary>
    internal class GetFeatureFlightQueryHandler : QueryHandler<GetFeatureFlightQuery, FeatureFlightDto>
    {
        private readonly ITenantConfigurationProvider _tenantConfigurationProvider;
        private readonly IAzureFeatureManager _azureFeatureManager;
        private readonly IFlightsDbRepositoryFactory _flightDbRepositoryFactory;
        
        public GetFeatureFlightQueryHandler(ITenantConfigurationProvider tenantConfigurationProvider,
            IAzureFeatureManager azureFeatureFlightManager,
            IFlightsDbRepositoryFactory flightsDbRepositoryFactory)
        {
            _tenantConfigurationProvider = tenantConfigurationProvider;
            _azureFeatureManager = azureFeatureFlightManager;
            _flightDbRepositoryFactory = flightsDbRepositoryFactory;
        }


        protected override async Task<FeatureFlightDto> ProcessRequest(GetFeatureFlightQuery query)
        {
            TenantConfiguration tenantConfiguration = await _tenantConfigurationProvider.Get(query.Tenant);
            FeatureFlightDto? flight = await GetFlightFromDb(query, tenantConfiguration);
            if (flight != null)
                return flight;
            return await GetFlightFromAzure(query, tenantConfiguration);
        }

        private async Task<FeatureFlightDto?> GetFlightFromDb(GetFeatureFlightQuery query, TenantConfiguration tenantConfiguration)
        {
            IDocumentRepository<FeatureFlightDto> repository = await _flightDbRepositoryFactory.GetFlightsRepository(tenantConfiguration.Name);
            if (repository == null)
                return null;

            string flagId = FlagUtilities.GetFeatureFlagId(tenantConfiguration.Name, query.Environment, query.FeatureName);
            return await repository.Get(flagId, tenantConfiguration.Name, query.TrackingIds);
        }

        private async Task<FeatureFlightDto?> GetFlightFromAzure(GetFeatureFlightQuery query, TenantConfiguration tenantConfiguration)
        {   
            AzureFeatureFlag azureFlag = await _azureFeatureManager.Get(query.FeatureName, tenantConfiguration.Name, query.Environment, query.TrackingIds);
            if (azureFlag == null)
                return null;
            return FeatureFlightDtoAssembler.Assemble(azureFlag);
        }
    }
}
