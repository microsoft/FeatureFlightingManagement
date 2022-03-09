using System.Threading.Tasks;
using Microsoft.FeatureFlighting.Common.Config;

namespace Microsoft.FeatureFlighting.Core.Evaluation
{
    /// <summary>
    /// Evaluates a single flag
    /// </summary>
    internal interface ISingleFlagEvaluator
    {
        /// <summary>
        /// Evaluates a feature flag
        /// </summary>
        /// <param name="featureFlag">Name of the flag</param>
        /// <param name="tenantConfiguration" cref="TenantConfiguration">Tenant Configuration</param>
        /// <param name="environment">Environment</param>
        /// <returns>Evaluation reesult</returns>
        Task<bool> IsEnabled(string featureFlag, TenantConfiguration tenantConfiguration, string environment);
    }
}