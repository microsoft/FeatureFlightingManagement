using System.Linq;
using System.Collections.Generic;
using Microsoft.FeatureFlighting.Common.Model;
using Microsoft.FeatureFlighting.Common.Model.AzureAppConfig;

namespace Microsoft.FeatureFlighting.Core.Domain.Assembler
{
    internal static class FeatureFlightDtoAssembler
    {
        public static FeatureFlightDto Assemble(FeatureFlightAggregateRoot flight)
        {
            FeatureFlightDto dto = new()
            {
                Id = flight.Id,
                Name = flight.Feature.Name,
                Description = flight.Feature.Description,
                Tenant = flight.Tenant.Id,
                Environment = flight.Tenant.Environment,
                Enabled = flight.Status.Enabled,
                IsIncremental = flight.Condition.IncrementalActivation,
                Version = flight.Version.ToString(),
                IsAzureFlightOptimized = flight.Status.IsOptimized,
                Optimizations = flight.Status.Optimizations,
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
                    TotalEvaluations = flight.EvaluationMetrics.TotalEvaluations,
                    AverageLatency = flight.EvaluationMetrics.Performance != null ? flight.EvaluationMetrics.Performance.AverageLatency : -1,
                    P90Latency = flight.EvaluationMetrics.Performance != null ? flight.EvaluationMetrics.Performance.Percentile90Latency : -1,
                    P95Latency = flight.EvaluationMetrics.Performance != null ? flight.EvaluationMetrics.Performance.Percentile95Latency : -1,
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
                    IsFirstStage = flight?.Condition?.InitialStage != null && stage.Id == flight.Condition.InitialStage.Id,
                    IsLastStage = flight?.Condition?.FinalStage != null && stage.Id == flight.Condition.FinalStage.Id,
                    Filters = stage.Filters?.Select(filter => new FilterDto()
                    {
                        FilterName = filter.Name,
                        FilterType = filter.Type,
                        Operator = filter.Operator.ToString(),
                        Value = filter.Value
                    }).ToList()
                }).ToList(),
                UsageReport = flight.Report != null
                    ? new FeatureUsageReportDto()
                    {
                        EnableAlerts = flight.Report.Settings.Status,
                        UsageStatement = flight.Report.DisplayStatement,
                        IsNew = flight.Report.IsNew,
                        GeneratedBy = flight.Report.GeneratedBy,
                        GeneratedOn = flight.Report.GeneratedOn,
                        ActivePeriod = flight.Report.ActivePeriod,
                        InactivePeriod = flight.Report.InactivePeriod,
                        UnusedPeriod = flight.Report.UnusedPeriod,
                        LaunchedPeriod = flight.Report.LaunchedPeriod,
                        HasActivationPeriodExceeded = flight.Report.HasActivePeriodCrossed,
                        HasDisabledPeriodExceeded = flight.Report.HasInactivePeriodCrossed,
                        HasUnsedPeriodExceeded = flight.Report.HasUnusedPeriodCrossed,
                        HasLaunchedPeriodExceeded = flight.Report.HasLaunchedPeriodCrossed,
                        ShowAlert = flight.Report.TriggerAlert
                    }
                    : null
            };
            return dto;
        }

        public static FeatureFlightDto Assemble(AzureFeatureFlag azureFeatureFlag, bool ignoreDetailedFilter = false)
        {
            FeatureFlightDto dto = new()
            {
                Id = azureFeatureFlag.Id,
                Name = azureFeatureFlag.Name,
                Description = azureFeatureFlag.Description,
                Tenant = azureFeatureFlag.Tenant,
                Environment = azureFeatureFlag.Environment,
                Enabled = azureFeatureFlag.Enabled,
                Version = azureFeatureFlag.Version,
                IsAzureFlightOptimized = azureFeatureFlag.IsFlagOptimized,
                Optimizations = azureFeatureFlag.Optimizations,
                Audit = azureFeatureFlag.LastModifiedOn != null ? new AuditDto()
                {
                    CreatedBy = "SYSTEM",
                    CreatedOn = azureFeatureFlag.LastModifiedOn.Value,
                    EnabledOn = azureFeatureFlag.Enabled ? azureFeatureFlag.LastModifiedOn.Value : null,
                    DisabledOn = !azureFeatureFlag.Enabled ? azureFeatureFlag.LastModifiedOn.Value : null,
                    LastModifiedBy = "SYSTEM",
                    LastModifiedOn = azureFeatureFlag.LastModifiedOn.Value,
                    LastUpdateType = "Flag Created"
                } : null,
                EvaluationMetrics = null
            };

            if (azureFeatureFlag.Conditions == null ||
                azureFeatureFlag.Conditions.Client_Filters == null || !azureFeatureFlag.Conditions.Client_Filters.Any())
                return dto;

            IEnumerable<AzureFilter> azureFilters = azureFeatureFlag.Conditions.Client_Filters.OrderBy(azureFilter => azureFilter.Parameters.StageId);
            int initialStageId = int.Parse(azureFilters.First().Parameters.StageId);
            int finalStageId = int.Parse(azureFilters.Last().Parameters.StageId);

            List<StageDto> stages = new();
            foreach(AzureFilter azureFilter in azureFilters)
            {
                StageDto currentStage = stages.FirstOrDefault(stage => stage.StageId.ToString() == azureFilter.Parameters.StageId);
                if (currentStage == null)
                {
                    currentStage = new StageDto()
                    {
                        StageId = int.Parse(azureFilter.Parameters.StageId),
                        StageName = azureFilter.Parameters.StageName,
                        IsActive = bool.Parse(azureFilter.Parameters.IsActive),
                        IsFirstStage = int.Parse(azureFilter.Parameters.StageId) == initialStageId,
                        IsLastStage = int.Parse(azureFilter.Parameters.StageId) == finalStageId,
                        LastActivatedOn = null,
                        LastDeactivatedOn = null,
                        Filters = ignoreDetailedFilter ? null : new List<FilterDto>
                        {
                            new FilterDto
                            {
                                FilterType = azureFilter.Name,
                                FilterName = azureFilter.Parameters.FlightContextKey,
                                Operator = azureFilter.Parameters.Operator,
                                Value =  azureFilter.Parameters.Value
                            }
                        }
                    };
                    stages.Add(currentStage);
                }
                else
                {
                    if (ignoreDetailedFilter)
                        continue;
                    FilterDto filter = new()
                    {
                        FilterType = azureFilter.Name,
                        FilterName = azureFilter.Parameters.FlightContextKey,
                        Operator = azureFilter.Parameters.Operator,
                        Value = azureFilter.Parameters.Value
                    };
                    currentStage.Filters.Add(filter);
                }
            }

            dto.Stages = stages;
            return dto;
        }
    }
}
