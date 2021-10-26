using RulesEngine.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Core.Operators;

namespace Microsoft.FeatureFlighting.Core.Spec
{
    /// <summary>
    /// Evaluates a Rule Engine
    /// </summary>
    public interface IRulesEngineEvaluator
    {
        /// <summary>
        /// Evaluates a <see cref="IRuleEngine"/> for feature flag filter
        /// </summary>
        /// <param name="context" cref="Dictionary{string, Object}">Context parameters for evaluating feature flags</param>
        /// <param name="trackingIds">Tracking ID of the operation</param>
        /// <returns cref="EvaluationResult">Evaluation Result</returns>
        Task<EvaluationResult> Evaluate(Dictionary<string, object> context, LoggerTrackingIds trackingIds);
    }
}
