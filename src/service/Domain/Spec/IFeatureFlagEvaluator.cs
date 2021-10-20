using System.Threading.Tasks;
using System.Collections.Generic;

namespace Microsoft.FeatureFlighting.Core.Spec
{
    /// <summary>
    /// Evaluates feature flags
    /// </summary>
    public interface IFeatureFlagEvaluator
    {
        /// <summary>
        /// Evaluates the given feature flags
        /// </summary>
        /// <param name="applicationName">Tenant Name</param>
        /// <param name="environment">Environment</param>
        /// <param name="featureFlags">List of feature flags</param>
        /// <returns>Dictionary of results</returns>
        Task<IDictionary<string, bool>> Evaluate(string applicationName, string environment, List<string> featureFlags);
    }
}
