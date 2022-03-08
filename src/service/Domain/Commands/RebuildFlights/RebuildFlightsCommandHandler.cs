using System.Text;
using System.Linq;
using CQRS.Mediatr.Lite;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Core.Domain;
using Microsoft.FeatureFlighting.Common.Model;
using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.FeatureFlighting.Common.Storage;
using Microsoft.FeatureFlighting.Core.Optimizer;
using Microsoft.FeatureFlighting.Common.AppConfig;
using Microsoft.FeatureFlighting.Common.AppExceptions;
using Microsoft.FeatureFlighting.Core.Domain.Assembler;
using Microsoft.FeatureFlighting.Common.Authentication;

namespace Microsoft.FeatureFlighting.Core.Commands
{
    /// <summary>
    /// Handles the <see cref="RebuildFlightsCommand"/>. Gets the flag from database and forces an optimization.
    /// </summary>
    internal class RebuildFlightsCommandHandler : CommandHandler<RebuildFlightsCommand, RebuildCommandResult>
    {
        private readonly ITenantConfigurationProvider _tenantConfigurationProvider;
        private readonly IAzureFeatureManager _azureFeatureManager;
        private readonly IFlightsDbRepositoryFactory _flightDbRepositoryFactory;
        private readonly IFlightOptimizer _flightOptimizer;
        private readonly IQueryService _queryService;
        private readonly IEventBus _eventBus;
        private readonly IIdentityContext _identityContext;

        public RebuildFlightsCommandHandler(ITenantConfigurationProvider tenantConfigurationProvider,
            IAzureFeatureManager azureFeatureFlightManager,
            IFlightsDbRepositoryFactory flightsDbRepositoryFactory,
            IFlightOptimizer flightOptimizer,
            IQueryService queryService,
            IEventBus eventBus,
            IIdentityContext identityContext)
        {
            _tenantConfigurationProvider = tenantConfigurationProvider;
            _azureFeatureManager = azureFeatureFlightManager;
            _flightDbRepositoryFactory = flightsDbRepositoryFactory;
            _flightOptimizer = flightOptimizer;
            _queryService = queryService;
            _eventBus = eventBus;
            _identityContext = identityContext;
        }

        protected override async Task<RebuildCommandResult> ProcessRequest(RebuildFlightsCommand command)
        {
            TenantConfiguration tenantConfiguration = await _tenantConfigurationProvider.Get(command.Tenant);
            IEnumerable<FeatureFlightAggregateRoot> featureFlights = await GetFlagsFromDb(command, tenantConfiguration);
            if (featureFlights == null || !featureFlights.Any())
                featureFlights = await GetAzureFeatureFlags(command, tenantConfiguration);

            if (featureFlights == null || !featureFlights.Any())
                throw new DomainException("No feature flight found", "RECOMPTE_001");

            List<Task> updateTasks = new();
            foreach(FeatureFlightAggregateRoot featureFlight in featureFlights)
            {
                featureFlight.ReBuild(_identityContext.GetCurrentUserPrincipalName(), command.Reason, _flightOptimizer, command.Source, command.TrackingIds);
                updateTasks.Add(SaveFlag(featureFlight, tenantConfiguration, command.TrackingIds));
                updateTasks.Add(_azureFeatureManager.Update(featureFlight.ProjectedFlag, command.TrackingIds));
                updateTasks.Add(featureFlight.Commit(_eventBus));
            }
            await Task.WhenAll(updateTasks);
            return new RebuildCommandResult(featureFlights.Select(flight => flight.Id).ToList());
        }

        private async Task<IEnumerable<FeatureFlightAggregateRoot>> GetFlagsFromDb(RebuildFlightsCommand command, TenantConfiguration tenantConfiguration)
        {
            IDocumentRepository<FeatureFlightDto> repository = await _flightDbRepositoryFactory.GetFlightsRepository(tenantConfiguration.Name);
            if (repository == null)
                return null;

            var getFlightsDbQueryBuilder = new StringBuilder()
               .Append("SELECT * FROM c WHERE c.Tenant = '")
               .Append(tenantConfiguration.Name)
               .Append("'")
               .Append(" AND c.Environment = '")
               .Append(command.Environment.ToLowerInvariant())
               .Append("'");

            if (command.FeatureNames != null && command.FeatureNames.Any())
            {
                getFlightsDbQueryBuilder
                    .Append(" AND c.Name IN (")
                    .Append(string.Join(',', command.FeatureNames.Select(feature => $"'{feature}'")))
                    .Append(")");
            }
            string getFlightsDbQuery = getFlightsDbQueryBuilder.ToString();

            IEnumerable<FeatureFlightDto> featureFlights = await repository.QueryAll(getFlightsDbQuery, tenantConfiguration.Name, command.TrackingIds);
            return featureFlights
                .Select(flight => FeatureFlightAggregateRootAssembler.Assemble(flight, tenantConfiguration))
                .ToList();
        }

        private async Task<IEnumerable<FeatureFlightAggregateRoot>> GetAzureFeatureFlags(RebuildFlightsCommand command, TenantConfiguration tenantConfiguration)
        {
            return (await _azureFeatureManager.Get(tenantConfiguration.Name, command.Environment))
                .Select(flag => FeatureFlightAggregateRootAssembler.Assemble(flag, tenantConfiguration))
                .ToList();
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
