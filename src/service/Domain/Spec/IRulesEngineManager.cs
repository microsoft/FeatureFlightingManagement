using System.Threading.Tasks;
using Microsoft.FeatureFlighting.Common;

namespace Microsoft.FeatureFlighting.Core.Spec
{
    /// <summary>
    /// Manages Rule Engines
    /// </summary>
    public interface IRulesEngineManager
    {
        /// <summary>
        /// Builds a <see cref="IRulesEngineEvaluator"/> for the given tenant and workflow (feature flag parameter)
        /// </summary>
        /// <param name="tenant">Name of the tenant</param>
        /// <param name="workflowName">Name of the workflow</param>
        /// <param name="trackingIds">Tracking IDs</param>
        /// <returns cref="IRulesEngineEvaluator">Rule engine evaluator</returns>
        Task<IRulesEngineEvaluator> Build(string tenant, string workflowName, LoggerTrackingIds trackingIds);


        /// <summary>
        /// Builds a <see cref="IRulesEngineEvaluator"/> for the given payload
        /// </summary>
        /// <param name="tenant">Name of the tenant</param>
        /// <param name="workflowName">Name of the workflow</param>
        /// <param name="workflowPayload">Rule engine payload</param>
        /// <returns></returns>
        Task<IRulesEngineEvaluator> Build(string tenant, string workflowName, string workflowPayload);
    }
}
