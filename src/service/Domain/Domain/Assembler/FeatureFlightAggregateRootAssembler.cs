using Microsoft.FeatureFlighting.Common.Model;
using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.FeatureFlighting.Core.Domain.ValueObjects;
using Microsoft.FeatureFlighting.Common.Model.AzureAppConfig;

namespace Microsoft.FeatureFlighting.Core.Domain.Assembler
{
    internal static class FeatureFlightAggregateRootAssembler
    {
        public static FeatureFlightAggregateRoot Assemble(AzureFeatureFlag flag, TenantConfiguration tenantConfiguration)
        {
            FeatureFlightAggregateRoot aggregateRoot = new(
                feature: new Feature(flag.Name, flag.Description),
                status: new Status(flag.Enabled, flag.IsFlagOptimized),
                tenant: new Tenant(tenantConfiguration.Name, flag.Environment),
                settings: new Settings(tenantConfiguration?.Optimization),
                condition: new Condition(flag.IncrementalRingsEnabled, flag.Conditions),
                audit: new Audit("SYSTEM", flag.LastModifiedOn ?? System.DateTime.UtcNow, flag.Enabled),
                version: new Version(flag.Version));
            return aggregateRoot;
        }

        public static FeatureFlightAggregateRoot Assemble(FeatureFlightDto flag, TenantConfiguration tenantConfiguration)
        {
            FeatureFlightAggregateRoot aggregateRoot = new(
                feature: new Feature(flag.Name, flag.Description),
                status: new Status(flag.Enabled, flag.IsAzureFlightOptimized),
                tenant: new Tenant(tenantConfiguration.Name, flag.Environment),
                settings: new Settings(tenantConfiguration?.Optimization),
                condition: new Condition(flag.IsIncremental, flag.Stages),
                version: new Version(flag.Version),
                audit: new Audit(flag.Audit),
                evaluationMetrics: new Metric(flag.EvaluationMetrics),
                report: new Report(tenantConfiguration, flag.UsageReport));
            return aggregateRoot;
        }
    }
}
