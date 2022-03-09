using System.Threading.Tasks;
using System.Collections.Generic;
using AppInsights.EnterpriseTelemetry.Context;
using Microsoft.FeatureFlighting.Common.Config;

namespace Microsoft.FeatureFlighting.Core.Evaluation
{
    /// <summary>
    /// Strategy to evaluate feeaturee flags
    /// </summary>
    internal interface IEvaluationStrategy
    {
        /// <summary>
        /// Evaluates feature flags
        /// </summary>
        /// <param name="features" cref="IEnumerable{string}">Collection of feature names</param>
        /// <param name="tenantConfiguration" cref="TenantConfiguration">Tenant Configuration</param>
        /// <param name="environment">Environment</param>
        /// <param name="event" cref="EventContext">Event Context</param>
        /// <returns></returns>
        Task<IDictionary<string, bool>> Evaluate(IEnumerable<string> features, TenantConfiguration tenantConfiguration, string environment, EventContext @event);
    }
}
