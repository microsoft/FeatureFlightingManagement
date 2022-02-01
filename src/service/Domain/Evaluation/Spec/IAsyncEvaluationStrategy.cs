using System.Threading.Tasks;
using System.Collections.Generic;
using AppInsights.EnterpriseTelemetry.Context;
using Microsoft.FeatureFlighting.Common.Config;

namespace Microsoft.FeatureFlighting.Core.Evaluation
{
    /// <summary>
    /// Evaluates all feature flags in async
    /// </summary>
    internal interface IAsyncEvaluationStrategy
    {
        /// <summary>
        /// Creates a <see cref="Task"/> for each feature and evaluates it separetely
        /// </summary>
        /// <param name="features">Collection of feature names</param>
        /// <param name="tenantConfiguration" cref="TenantConfiguration">Tenant Configuration</param>
        /// <param name="environment">Environment</param>
        /// <param name="event" cref="EventContext">Event</param>
        /// <returns cref="IDictionary{string, bool}">Dictionary of evaluation results</returns>
        Task<IDictionary<string, bool>> Evaluate(IEnumerable<string> features, TenantConfiguration tenantConfiguration, string environment, EventContext @event);
    }
}