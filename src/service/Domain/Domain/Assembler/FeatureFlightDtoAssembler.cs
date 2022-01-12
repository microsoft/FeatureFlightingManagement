using System.Linq;
using Microsoft.FeatureFlighting.Common.Model;

namespace Microsoft.FeatureFlighting.Core.Domain.Assembler
{
    public static class FeatureFlightDtoAssembler
    {
        public static FeatureFlightDto Assemble(FeatureFlightAggregateRoot flight)
        {
            FeatureFlightDto dto = new()
            {
                Id = flight.Id,
                FeatureName = flight.Feature.Name,
                Description = flight.Feature.Description,
                Tenant = flight.Tenant.Id,
                Environment = flight.Tenant.Environment,
                Enabled = flight.Status.Enabled,
                IsIncremental = flight.Condition.IncrementalActivation,
                Version = flight.Version.ToString(),
                IsAzureFlightOptimized = flight.Status.IsOptimized,
                Audit = flight.Audit != null ? new AuditDto
                {
                    CreatedBy = flight.Audit.CreatedBy,
                    CreatedOn = flight.Audit.CreatedOn,
                    LastModifiedBy = flight.Audit.LastModifiedBy,
                    LastModifiedOn = flight.Audit.LastModifiedOn,
                    EnabledOn = flight.Audit.EnabledOn,
                    DisabledOn = flight.Audit.DisabledOn,
                    LastUpdateType = flight.Audit.LastUpdateType
                } : null,
                EvaluationMetrics = flight.EvaluationMetrics != null ? new EvaluationMetricsDto
                {
                    LastEvaluatedOn = flight.EvaluationMetrics.LastEvaluatedOn,
                    LastEvaluatedBy = flight.EvaluationMetrics.LastEvaluatedBy,
                    EvaluationCount = flight.EvaluationMetrics.EvaluationCount,
                    AverageLatency = flight.EvaluationMetrics.Performance.AverageLatency,
                    P90Latency = flight.EvaluationMetrics.Performance.Percentile90Latency,
                    P95Latency = flight.EvaluationMetrics.Performance.Percentile95Latency,
                    From = flight.EvaluationMetrics.StartedOn,
                    To = flight.EvaluationMetrics.CompletedOn
                } : null,
                Stages = flight.Condition?.Stages?.Select(stage => new StageDto()
                {
                    StageId = stage.Id,
                    StageName = stage.Name,
                    IsActive = stage.IsActive,
                    LastActivatedOn = stage.ActivatedOn,
                    LastDeactivatedOn = stage.DeactivatedOn,
                    IsFirstStage = stage.Id == flight.Condition.InitialStage.Id,
                    IsLastStage = stage.Id == flight.Condition.FinalStage.Id,
                    Filters = stage.Filters?.Select(filter => new FilterDto()
                    {
                        FilterName = filter.Name,
                        FilterType = filter.Type,
                        Operator = filter.Operator.ToString(),
                        Value = filter.Value
                    }).ToList()
                }).ToList()
            };
            return dto;
        }
    }
}
