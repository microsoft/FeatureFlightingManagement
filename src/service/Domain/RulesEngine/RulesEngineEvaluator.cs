using System;
using System.Linq;
using RulesEngine.Models;
using System.Threading.Tasks;
using RulesEngine.Interfaces;
using System.Collections.Generic;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Core.Spec;
using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.FeatureFlighting.Core.Operators;
using Microsoft.FeatureFlighting.Common.AppExceptions;

namespace Microsoft.FeatureFlighting.Core.RulesEngine
{   
    /// <inheritdoc/>
    public class RulesEngineEvaluator: IRulesEngineEvaluator
    {
        private readonly IRulesEngine _ruleEngine;
        private readonly string _workflowName;
        private readonly TenantConfiguration _tenantConfiguration;

        public RulesEngineEvaluator(IRulesEngine ruleEngine, string workflowName, TenantConfiguration tenantConfiguration)
        {
            _ruleEngine = ruleEngine;
            _workflowName = workflowName;
            _tenantConfiguration = tenantConfiguration;
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
                    return new EvaluationResult(isSuccess: false, failureMessage);
                }
                return new EvaluationResult(true, $"{_workflowName} rule engine passsed");
            }
            catch (Exception exception)
            {
                throw new RuleEngineException(_workflowName, _tenantConfiguration.Name, exception, "FeatureFlighting.RulesEngineEvaluator.Evaluate", trackingIds.CorrelationId, trackingIds.TransactionId);
            }
        }
    }
}
