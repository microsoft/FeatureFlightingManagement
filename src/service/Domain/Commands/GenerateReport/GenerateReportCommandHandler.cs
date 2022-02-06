using System.Linq;
using CQRS.Mediatr.Lite;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Core.Domain;
using Microsoft.FeatureFlighting.Common.Model;
using Microsoft.FeatureFlighting.Core.Queries;
using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.FeatureFlighting.Common.Storage;
using Microsoft.FeatureFlighting.Core.Domain.Events;
using Microsoft.FeatureFlighting.Core.Domain.Assembler;
using Microsoft.FeatureFlighting.Common.Authentication;

namespace Microsoft.FeatureFlighting.Core.Commands
{
    /// <summary>
    /// Handles <see cref="GenerateReportCommand"/>
    /// </summary>
    internal class GenerateReportCommandHandler : CommandHandler<GenerateReportCommand, ReportCommandResult>
    {
        private readonly ITenantConfigurationProvider _tenantConfigurationProvider;
        private readonly IFlightsDbRepositoryFactory _flightDbRepositoryFactory;
        private readonly IQueryService _queryService;
        private readonly IEventBus _eventBus;
        private readonly IIdentityContext _identityContext;

        public GenerateReportCommandHandler(ITenantConfigurationProvider tenantConfigurationProvider,
            IFlightsDbRepositoryFactory flightDbRepositoryFactory,
            IQueryService queryService, IEventBus eventBus, IIdentityContext identityContext)
        {
            _tenantConfigurationProvider = tenantConfigurationProvider;
            _flightDbRepositoryFactory = flightDbRepositoryFactory;
            _queryService = queryService;
            _eventBus = eventBus;
            _identityContext = identityContext;
        }

        protected override async Task<ReportCommandResult> ProcessRequest(GenerateReportCommand command)
        {
            TenantConfiguration tenantConfiguration = await _tenantConfigurationProvider.Get(command.Tenant);
            if (tenantConfiguration.IntelligentAlerts == null || !tenantConfiguration.IsReportingEnabled())
            {
                return new ReportCommandResult("Alert generation is disabled for this tenant");
            }

            UsageReportDto report = new(tenantConfiguration.Name, command.Environment, _identityContext.GetCurrentUserPrincipalName());
            IEnumerable<FeatureFlightDto> flightsDto = await GetFeatureFlights(command, tenantConfiguration);

            if (flightsDto == null || !flightsDto.Any())
                return new ReportCommandResult(report);

            List<FeatureFlightAggregateRoot> flights = new();
            foreach (FeatureFlightDto flightDto in flightsDto)
            {
                FeatureFlightAggregateRoot flight = FeatureFlightAggregateRootAssembler.Assemble(flightDto, tenantConfiguration);
                flight.GenerateReport(_identityContext.GetCurrentUserPrincipalName(), command.TrackingIds);
                flights.Add(flight);
            }

            report = GenerateTenantReport(flights, tenantConfiguration, command);
            ReportGenerated @event = new(report, command.CorrelationId, command.TransactionId);

            if (command.TriggerAlert)
                await _eventBus.Send(@event);
            
            await SaveFlights(flights, tenantConfiguration, command.TrackingIds);
            return new ReportCommandResult(report);
        }

        private Task<IEnumerable<FeatureFlightDto>> GetFeatureFlights(GenerateReportCommand command, TenantConfiguration tenantConfiguration)
        {
            GetFeatureFlightsQuery query = new(tenantConfiguration.Name, command.Environment, command.CorrelationId, command.TransactionId);
            return _queryService.Query(query);
        }

        private async Task SaveFlights(List<FeatureFlightAggregateRoot> flights, TenantConfiguration tenantConfiguration, LoggerTrackingIds trackingIds)
        {
            if (tenantConfiguration.FlightsDatabase == null || tenantConfiguration.FlightsDatabase.Disabled)
                return;

            IDocumentRepository<FeatureFlightDto> repository = await _flightDbRepositoryFactory.GetFlightsRepository(tenantConfiguration.Name);
            if (repository == null)
                return;

            List<FeatureFlightDto> dtos = flights.Select(flight => FeatureFlightDtoAssembler.Assemble(flight)).ToList();

            var dtoGroups = dtos.Select((dto, index) => new
            {
                Index = index,
                Dto = dto
            }).GroupBy(obj => obj.Index / 10, obj => obj.Dto);
            
            foreach(var dtoGroup in dtoGroups)
            {
                List<Task> flightUpdateTasks = new();
                List<FeatureFlightDto> currentBatch = dtoGroup.ToList();
                foreach(var dto in currentBatch)
                {
                    flightUpdateTasks.Add(repository.Save(dto, dto.Tenant, trackingIds));
                }
                await Task.WhenAll(flightUpdateTasks);
            }
        }

        private UsageReportDto GenerateTenantReport(List<FeatureFlightAggregateRoot> flights, TenantConfiguration tenantConfiguration, GenerateReportCommand command)
        {
            UsageReportDto report = new(tenantConfiguration.Name, command.Environment, _identityContext.GetCurrentUserPrincipalName());
            report.ActiveFeatures = flights
                .Where(flight => flight.Status.Enabled && flight.Status.IsActive)
                .Select(flight => flight.Feature.Name)
                .ToList();

            report.NewlyAddedFeatures = flights
                .Where(flight => flight.Report.IsNew)
                .Select(flight => flight.Feature.Name)
                .ToList();

            report.InactiveFeatures = flights
                .Where(flight => !flight.Status.Enabled || !flight.Status.IsActive)
                .Select(flight => flight.Feature.Name)
                .ToList();

            report.TotalEvaluations = flights.Sum(flight => flight.EvaluationMetrics?.EvaluationCount ?? 0);

            report.UnusedFeatures = flights
                .Where(flight => flight.Report.HasUnusedPeriodCrossed && flight.Report.TriggerAlert)
                .Select(flight => new ThresholdExceededReportDto()
                {
                    FeatureId = flight.Id,
                    FeatureName = flight.Feature.Name,
                    Environment = command.Environment,
                    Threshold = flight.Report.Settings.MaximumUnusedPeriod,
                    Value = flight.Report.UnusedPeriod,
                    ThresholdUnit = "Days"
                })
                .ToList();

            report.LongInactiveFeatures = flights
                .Where(flight => flight.Report.HasInactivePeriodCrossed && flight.Report.TriggerAlert)
                .Where(flight => !flight.Report.HasUnusedPeriodCrossed)
                .Select(flight => new ThresholdExceededReportDto()
                {
                    FeatureId = flight.Id,
                    FeatureName = flight.Feature.Name,
                    Environment = command.Environment,
                    Threshold = flight.Report.Settings.MaximumInactivePeriod,
                    Value = flight.Report.InactivePeriod,
                    ThresholdUnit = "Days"
                })
                .ToList();

            report.LongActiveFeatures = flights
                .Where(flight => flight.Report.HasActivePeriodCrossed && flight.Report.TriggerAlert)
                .Where(flight => !flight.Report.HasUnusedPeriodCrossed && !flight.Report.HasInactivePeriodCrossed)
                .Select(flight => new ThresholdExceededReportDto()
                {
                    FeatureId = flight.Id,
                    FeatureName = flight.Feature.Name,
                    Environment = command.Environment,
                    Threshold = flight.Report.Settings.MaximumActivationPeriod,
                    Value = flight.Report.ActivePeriod,
                    ThresholdUnit = "Days"
                })
                .ToList();

            report.CreateFlightSelectorBody();
            report.UpdatePendingAction();
            return report;
        }
    }
}
