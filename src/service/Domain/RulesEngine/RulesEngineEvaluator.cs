using System;
using System.Linq;
using RulesEngine.Models;
using System.Threading.Tasks;
using RulesEngine.Interfaces;
using System.Collections.Generic;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Core.Spec;
using Microsoft.FeatureFlighting.Core.Operators;
using Microsoft.FeatureFlighting.Common.AppExceptions;

namespace Microsoft.FeatureFlighting.Core.RulesEngine
{   
    /// <inheritdoc/>
    public class RulesEngineEvaluator: IRulesEngineEvaluator
    {
        private readonly IRulesEngine _ruleEngine;
        private readonly string _workflowName;
        private readonly string _tenant;

        public RulesEngineEvaluator(IRulesEngine ruleEngine, string workflowName, string tenant)
        {
            _ruleEngine = ruleEngine ?? throw new ArgumentNullException(nameof(ruleEngine));
            _workflowName = workflowName ?? throw new ArgumentNullException(nameof(workflowName));
            _tenant = tenant;
        }

        /// <inheritdoc/>
        public async Task<EvaluationResult> Evaluate(Dictionary<string, object> context, LoggerTrackingIds trackingIds)
        {   
            try
            {
                RuleParameter[] parameters = context.Select(contextParameter =>
                new RuleParameter(contextParameter.Key, contextParameter.Value))
                .ToArray();

                List<RuleResultTree> ruleResult = await _ruleEngine.ExecuteAllRulesAsync(_workflowName, parameters);
                bool isFailed = ruleResult.Any(result => !result.IsSuccess);

                if (isFailed)
                {
                    string failureMessage =
                        string.Join(',', ruleResult.Where(result => !result.IsSuccess)
                            .Select(failedRule => failedRule.ExceptionMessage ?? failedRule.Rule.ErrorMessage).ToArray());
                    return new EvaluationResult(result: false, failureMessage);
                }
                return new EvaluationResult(result: true);
            }
            catch (Exception exception)
            {
                throw new RuleEngineException(_workflowName, _tenant, exception, "FeatureFlighting.RulesEngineEvaluator.Evaluate", trackingIds.CorrelationId, trackingIds.TransactionId);
            }
        }
    }
}
