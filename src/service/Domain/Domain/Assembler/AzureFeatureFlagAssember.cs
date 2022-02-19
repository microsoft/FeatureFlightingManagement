using System.Linq;
using System.Collections.Generic;
using Microsoft.FeatureFlighting.Common.Model;
using Microsoft.FeatureFlighting.Core.Domain.ValueObjects;
using Microsoft.FeatureFlighting.Common.Model.AzureAppConfig;

namespace Microsoft.FeatureFlighting.Core.Domain.Assembler
{
    internal static class AzureFeatureFlagAssember
    {
        public static AzureFeatureFlag Assemble(FeatureFlightAggregateRoot flight)
        {
            AzureFeatureFlag azureFlag = new()
            {
                Id = flight.Id,
                Name = flight.Feature.Name,
                Description = flight.Feature.Description,
                Tenant = flight.Tenant.Id,
                Environment = flight.Tenant.Environment,
                Enabled = flight.Status.Enabled,
                Version = flight.Version.ToString(),
                IncrementalRingsEnabled = flight.Condition.IncrementalActivation,
                Conditions = new AzureFilterCollection()
                {
                    Client_Filters = new AzureFilter[] { }
                }
            };

            if (flight.Condition.Stages == null || !flight.Condition.Stages.Any())
                return azureFlag;

            List<AzureFilter> azureFilters = new();
            foreach(Stage stage in flight.Condition.Stages)
            {
                if (stage.Filters == null || !stage.Filters.Any())
                    continue;

                foreach(Filter filter in stage.Filters)
                {
                    azureFilters.Add(new AzureFilter()
                    {
                        Name = filter.Type,
                        Parameters = new AzureFilterParameters()
                        {
                            StageId = stage.Id.ToString(),
                            StageName = stage.Name,
                            FlightContextKey = filter.Name,
                            IsActive = stage.IsActive.ToString(),
                            Operator = filter.Operator.ToString(),
                            Value = filter.Value
                        }
                    });
                }
            }
            azureFlag.Conditions.Client_Filters = azureFilters.ToArray();
            return azureFlag;
        }

        public static AzureCustomFeatureFlag Assemble(FeatureFlightDto flight)
        {
            AzureCustomFeatureFlag azureFlag = new()
            {
                Id = flight.Id,
                Name = flight.Name,
                Description = flight.Description,
                Tenant = flight.Tenant,
                Environment = flight.Environment,
                Enabled = flight.Enabled,
                Version = flight.Version,
                IncrementalRingsEnabled = flight.IsIncremental,
                IsFlagOptimized = flight.IsAzureFlightOptimized,
                Conditions = new AzureFilterCollection()
                {
                    Client_Filters = new AzureFilter[] { }
                },
                Insights = new AzureFlagInsights()
                {
                    LastEvaluatedBy = flight.EvaluationMetrics?.LastEvaluatedBy,
                    LastEvaluatedOn = flight.EvaluationMetrics?.LastEvaluatedOn,
                    AverageEvaluationLatency = flight.EvaluationMetrics?.AverageLatency ?? 0,
                    TotalEvaluations = flight.EvaluationMetrics?.TotalEvaluations ?? 0,
                    WeeklyEvaluations = flight.EvaluationMetrics?.EvaluationCount ?? 0,
                    ShowWarning = flight.UsageReport?.ShowAlert ?? false,
                    WarningStatement = flight.UsageReport?.UsageStatement,
                    MetricsLastUpdatedOn = flight.EvaluationMetrics?.To
                }
            };

            if (flight.Stages == null || !flight.Stages.Any())
                return azureFlag;

            List<AzureFilter> azureFilters = new();
            foreach (StageDto stage in flight.Stages)
            {
                if (stage.Filters == null || !stage.Filters.Any())
                    continue;

                foreach (FilterDto filter in stage.Filters)
                {
                    azureFilters.Add(new AzureFilter()
                    {
                        Name = filter.FilterType,
                        Parameters = new AzureFilterParameters()
                        {
                            StageId = stage.StageId.ToString(),
                            StageName = stage.StageName,
                            FlightContextKey = filter.FilterName,
                            IsActive = stage.IsActive.ToString(),
                            Operator = filter.Operator,
                            Value = filter.Value
                        }
                    });
                }
            }
            azureFlag.Conditions.Client_Filters = azureFilters.ToArray();

            return azureFlag;
        }
    }
}
