using CQRS.Mediatr.Lite;
using System.Threading.Tasks;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Core.Domain;
using Microsoft.FeatureFlighting.Common.Model;
using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.FeatureFlighting.Core.Queries;
using Microsoft.FeatureFlighting.Common.Storage;
using Microsoft.FeatureFlighting.Core.Optimizer;
using Microsoft.FeatureFlighting.Common.AppConfig;
using Microsoft.FeatureFlighting.Common.AppExceptions;
using Microsoft.FeatureFlighting.Core.Domain.Assembler;
using Microsoft.FeatureFlighting.Common.Authentication;
using Microsoft.FeatureFlighting.Common.Model.AzureAppConfig;

namespace Microsoft.FeatureFlighting.Core.Commands
{
    /// <summary>
    /// Handles the command <see cref="CreateFeatureFlightCommand"/>
    /// </summary>
    internal class CreateFeatureFlightCommandHandler : CommandHandler<CreateFeatureFlightCommand, IdCommandResult>
    {
        private readonly ITenantConfigurationProvider _tenantConfigurationProvider;
        private readonly IAzureFeatureManager _azureFeatureManager;
        private readonly IFlightsDbRepositoryFactory _flightDbRepositoryFactory;
        private readonly IFlightOptimizer _flightOptimizer;
        private readonly IQueryService _queryService;
        private readonly IEventBus _eventBus;
        private readonly IIdentityContext _identityContext;

        public CreateFeatureFlightCommandHandler(ITenantConfigurationProvider tenantConfigurationProvider,
            IAzureFeatureManager azureFeatureManager,
            IFlightsDbRepositoryFactory flightDbRepositoryFactory,
            IFlightOptimizer flightOptimizer,
            IQueryService queryService,
            IEventBus eventBus,
            IIdentityContext identityContext)
        {
            _tenantConfigurationProvider = tenantConfigurationProvider;
            _azureFeatureManager = azureFeatureManager;
            _flightDbRepositoryFactory = flightDbRepositoryFactory;
            _flightOptimizer = flightOptimizer;
            _queryService = queryService;
            _eventBus = eventBus;
            _identityContext = identityContext;
        }

        protected override async Task<IdCommandResult> ProcessRequest(CreateFeatureFlightCommand command)
        {
            TenantConfiguration tenantConfiguration = await _tenantConfigurationProvider.Get(command.AzureFeatureFlag.Tenant);
            FeatureFlightAggregateRoot flight = FeatureFlightAggregateRootAssembler.Assemble(command.AzureFeatureFlag, tenantConfiguration);
            
            await VerifyUniqueFlag(flight, tenantConfiguration, command.TrackingIds);
            flight.CreateFeatureFlag(_flightOptimizer, _identityContext.GetCurrentUserPrincipalName(), command.Source, command.TrackingIds);

            await Task.WhenAll(
                SaveFlag(flight, tenantConfiguration, command.TrackingIds),
                _azureFeatureManager.Update(flight.ProjectedFlag, command.TrackingIds)
            );

            await flight.Commit(_eventBus);
            return new IdCommandResult(flight.Id);
        }

        private async Task VerifyUniqueFlag(FeatureFlightAggregateRoot flight, TenantConfiguration tenantConfiguration, LoggerTrackingIds trackingIds)
        {
            GetAzureFeatureFlagQuery query = new(flight.Feature.Name, tenantConfiguration.Name, flight.Tenant.Environment, trackingIds.CorrelationId, trackingIds.TransactionId);
            AzureFeatureFlag? flag = await _queryService.Query(query);
            if (flag == null)
                return;
            throw new DomainException($"Flight for feature {flight.Feature.Name} already exists under {tenantConfiguration.Name} in {flight.Tenant.Environment}", "CREATE_FLIGHT_001",
                trackingIds.CorrelationId, trackingIds.TransactionId, "CreateFeatureFlightCommandHandler:Verify");
        }

        private async Task SaveFlag(FeatureFlightAggregateRoot flight, TenantConfiguration tenantConfiguration, LoggerTrackingIds trackingIds)
        {
            if (tenantConfiguration.FlightsDatabase == null || tenantConfiguration.FlightsDatabase.Disabled)
                return;

            IDocumentRepository<FeatureFlightDto> repository = await _flightDbRepositoryFactory.GetFlightsRepository(tenantConfiguration.Name);
            if (repository == null)
                return;

            FeatureFlightDto flightDto = FeatureFlightDtoAssembler.Assemble(flight);
            await repository.Save(flightDto, flightDto.Tenant, trackingIds);
        }
    }
}
