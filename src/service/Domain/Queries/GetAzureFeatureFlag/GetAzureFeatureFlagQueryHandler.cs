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
    /// Gets <see cref="AzureFeatureFlag"/> from database or Azure App Config
    /// </summary>
    public class GetAzureFeatureFlagQueryHandler : QueryHandler<GetAzureFeatureFlagQuery, AzureFeatureFlag?>
    {
        private readonly ITenantConfigurationProvider _tenantConfigurationProvider;
        private readonly IFlightsDbRepositoryFactory _flightDbRepositoryFactory;
        private readonly IAzureFeatureManager _azureFeatureFlagManager;
        
        public GetAzureFeatureFlagQueryHandler(ITenantConfigurationProvider tenantConfigurationProvider, IFlightsDbRepositoryFactory flightsDbRepositoryFactory, IAzureFeatureManager azureFeatureManager)
        {
            _tenantConfigurationProvider = tenantConfigurationProvider;
            _flightDbRepositoryFactory = flightsDbRepositoryFactory;
            _azureFeatureFlagManager = azureFeatureManager;
        }

        protected override async Task<AzureFeatureFlag> ProcessRequest(GetAzureFeatureFlagQuery query)
        {
            TenantConfiguration tenantConfiguration = await _tenantConfigurationProvider.Get(query.TenantName);
            return (await GetFlagFromRepository(query, tenantConfiguration)) ?? (await GetFlagFromAzureAppConfig(query, tenantConfiguration));
        }

        private async Task<AzureFeatureFlag?> GetFlagFromRepository(GetAzureFeatureFlagQuery query, TenantConfiguration tenantConfiguration)
        {
            if (tenantConfiguration.FlightsDatabase == null || tenantConfiguration.FlightsDatabase.Disabled)
                return null;

            IDocumentRepository<FeatureFlightDto> repository = await _flightDbRepositoryFactory.GetFlightsRepository(tenantConfiguration.Name);
            if (repository == null)
                return null;

            FeatureFlightDto flightDto = await repository.Get(FlagUtilities.GetFeatureFlagId(tenantConfiguration.Name, query.EnvironmentName, query.FeatureName), tenantConfiguration.Name, query.TrackingIds);
            if (flightDto == null)
                return null;

            return AzureFeatureFlagAssember.Assemble(flightDto);
        }

        private async Task<AzureFeatureFlag?> GetFlagFromAzureAppConfig(GetAzureFeatureFlagQuery query, TenantConfiguration tenantConfiguration)
        {
            return await _azureFeatureFlagManager.Get(query.FeatureName, tenantConfiguration.Name, query.EnvironmentName, query.TrackingIds);
        }
    }
}
