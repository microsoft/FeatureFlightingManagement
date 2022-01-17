using CQRS.Mediatr.Lite;
using System.Threading.Tasks;
using Microsoft.FeatureFlighting.Core.Domain;
using Microsoft.FeatureFlighting.Common.Model;
using Microsoft.FeatureFlighting.Core.Queries;
using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.FeatureFlighting.Common.Storage;
using Microsoft.FeatureFlighting.Common.AppConfig;
using Microsoft.FeatureFlighting.Common.AppExceptions;
using Microsoft.FeatureFlighting.Core.Domain.Assembler;
using Microsoft.FeatureFlighting.Common.Authentication;

namespace Microsoft.FeatureFlighting.Core.Commands
{
    /// <summary>
    /// Handles <see cref="DeleteFeatureFlightCommand"/>. Deletes the feature flight from database and Azure App config.
    /// </summary>
    internal class DeleteFeatureFlightCommandHandler : CommandHandler<DeleteFeatureFlightCommand, IdCommandResult>
    {
        private readonly ITenantConfigurationProvider _tenantConfigurationProvider;
        private readonly IAzureFeatureManager _azureFeatureManager;
        private readonly IFlightsDbRepositoryFactory _flightDbRepositoryFactory;
        private readonly IQueryService _queryService;
        private readonly IEventBus _eventBus;
        private readonly IIdentityContext _identityContext;

        public DeleteFeatureFlightCommandHandler(ITenantConfigurationProvider tenantConfigurationProvider,
            IAzureFeatureManager azureFeatureFlightManager,
            IFlightsDbRepositoryFactory flightsDbRepositoryFactory,
            IQueryService queryService,
            IEventBus eventBus,
            IIdentityContext identityContext)
        {
            _tenantConfigurationProvider = tenantConfigurationProvider;
            _azureFeatureManager = azureFeatureFlightManager;
            _flightDbRepositoryFactory = flightsDbRepositoryFactory;
            _queryService = queryService;
            _eventBus = eventBus;
            _identityContext = identityContext;
        }

        protected override async Task<IdCommandResult> ProcessRequest(DeleteFeatureFlightCommand command)
        {
            TenantConfiguration tenantConfiguration = await _tenantConfigurationProvider.Get(command.Tenant);
            FeatureFlightAggregateRoot flight = await GetFeatureFlight(command, tenantConfiguration);
            flight.Delete(_identityContext.GetCurrentUserPrincipalName(), command.TrackingIds);
            await DeleteFromDatabase(flight, tenantConfiguration, command);
            await DeleteFromAzure(flight, tenantConfiguration, command);
            await flight.Commit(_eventBus);

            return new IdCommandResult(flight.Id);
        }

        private async Task<FeatureFlightAggregateRoot> GetFeatureFlight(DeleteFeatureFlightCommand command, TenantConfiguration tenantConfiguration)
        {
            GetFeatureFlightQuery query = new(command.FeatureName, tenantConfiguration.Name, command.Environment, command.CorrelationId, command.TransactionId);
            FeatureFlightDto flight = await _queryService.Query(query);
            if (flight == null)
                throw new DomainException($"Flight for feature {command.FeatureName} does not existing for {tenantConfiguration.Name} in {command.Environment}",
                    "DELETE_FLAG_001", command.CorrelationId, command.TransactionId, "DeleteFeatureFlightCommandHandler:ProcessRequest");

            return FeatureFlightAggregateRootAssembler.Assemble(flight, tenantConfiguration);
        }

        private async Task DeleteFromDatabase(FeatureFlightAggregateRoot flight, TenantConfiguration tenantConfiguration, DeleteFeatureFlightCommand command)
        {
            if (tenantConfiguration.FlightsDatabase == null || tenantConfiguration.FlightsDatabase.Disabled)
                return;

            IDocumentRepository<FeatureFlightDto> repository = await _flightDbRepositoryFactory.GetFlightsRepository(tenantConfiguration.Name);
            if (repository == null)
                return;

            await repository.Delete(flight.Id, tenantConfiguration.Name, command.TrackingIds);
        }

        private async Task DeleteFromAzure(FeatureFlightAggregateRoot flight, TenantConfiguration tenantConfiguration, DeleteFeatureFlightCommand command)
        {
            await _azureFeatureManager.Delete(flight.Feature.Name, tenantConfiguration.Name, flight.Tenant.Environment, command.TrackingIds);
        }
    }
}
