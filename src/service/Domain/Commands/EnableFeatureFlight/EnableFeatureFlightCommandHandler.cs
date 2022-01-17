using CQRS.Mediatr.Lite;
using System.Threading.Tasks;
using Microsoft.FeatureFlighting.Common;
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
    /// Enables a disabled feature flight
    /// </summary>
    internal class EnableFeatureFlightCommandHandler : CommandHandler<EnableFeatureFlightCommand, IdCommandResult>
    {
        private readonly ITenantConfigurationProvider _tenantConfigurationProvider;
        private readonly IAzureFeatureManager _azureFeatureManager;
        private readonly IFlightsDbRepositoryFactory _flightDbRepositoryFactory;
        private readonly IQueryService _queryService;
        private readonly IEventBus _eventBus;
        private readonly IIdentityContext _identityContext;

        public EnableFeatureFlightCommandHandler(ITenantConfigurationProvider tenantConfigurationProvider,
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

        protected override async Task<IdCommandResult> ProcessRequest(EnableFeatureFlightCommand command)
        {
            TenantConfiguration tenantConfiguration = await _tenantConfigurationProvider.Get(command.Tenant);
            FeatureFlightAggregateRoot flight = await GetFeatureFlight(command, tenantConfiguration);

            flight.Enable(_identityContext.GetCurrentUserPrincipalName(), command.TrackingIds, out bool isUpdated);
            if (!isUpdated)
                return new IdCommandResult(flight.Id);

            await Task.WhenAll(
                SaveFlag(flight, tenantConfiguration, command.TrackingIds),
                _azureFeatureManager.ChangeStatus(flight.Feature.Name, flight.Tenant.Id, flight.Tenant.Environment, flight.Status.IsActive, command.TrackingIds)
            );

            await flight.Commit(_eventBus);
            return new IdCommandResult(flight.Id);
        }

        private async Task<FeatureFlightAggregateRoot> GetFeatureFlight(EnableFeatureFlightCommand command, TenantConfiguration tenantConfiguration)
        {
            GetFeatureFlightQuery query = new(command.FeatureName, tenantConfiguration.Name, command.Environment, command.CorrelationId, command.TransactionId);
            FeatureFlightDto flight = await _queryService.Query(query);
            if (flight == null)
                throw new DomainException($"Flight for feature {command.FeatureName} does not existing for {tenantConfiguration.Name} in {command.Environment}",
                    "UPDATE_FLAG_001", command.CorrelationId, command.TransactionId, "EnableFeatureFlightCommandHandler:GetFeatureFlight");

            return FeatureFlightAggregateRootAssembler.Assemble(flight, tenantConfiguration);
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