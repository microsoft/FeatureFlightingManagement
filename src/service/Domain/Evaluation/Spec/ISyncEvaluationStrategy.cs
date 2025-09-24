using System.Threading.Tasks;
using System.Collections.Generic;
using AppInsights.EnterpriseTelemetry.Context;
using Microsoft.FeatureFlighting.Common.Config;

namespace Microsoft.FeatureFlighting.Core.Evaluation
{
    /// <summary>
    /// Evaluates all feature flags synchonously
    /// </summary>
    internal interface ISyncEvaluationStrategy
    {
        /// <summary>
        /// Evaluates all feature flags in order
        /// </summary>
        /// <param name="features">Collection of feature names</param>
        /// <param name="featureKeysOnAzure" cref="IEnumerable{string}">Collection of feature keys</param>
        /// <param name="tenantConfiguration" cref="TenantConfiguration">Tenant Configuration</param>
        /// <param name="environment">Environment</param>
        /// <param name="event" cref="EventContext">Event</param>
        /// <returns cref="IDictionary{string, bool}">Dictionary of evaluation results</returns>
        Task<IDictionary<string, bool>> Evaluate(IEnumerable<string> featureFlags, IEnumerable<string> featureKeysOnAzure, TenantConfiguration tenantConfiguration, string environment, EventContext @event);
    }
}