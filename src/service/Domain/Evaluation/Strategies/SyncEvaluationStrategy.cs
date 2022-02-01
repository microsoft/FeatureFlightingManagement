using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using AppInsights.EnterpriseTelemetry.Context;
using Microsoft.FeatureFlighting.Common.Config;

namespace Microsoft.FeatureFlighting.Core.Evaluation
{
    /// <summary>
    /// Evaluates all flags synchronously
    /// </summary>
    internal class SyncEvaluationStrategy : IEvaluationStrategy, ISyncEvaluationStrategy
    {
        private readonly ISingleFlagEvaluator _singleFlagEvaluator;

        public SyncEvaluationStrategy(ISingleFlagEvaluator singleFlagEvaluator)
        {
            _singleFlagEvaluator = singleFlagEvaluator;
        }

        public async Task<IDictionary<string, bool>> Evaluate(IEnumerable<string> featureFlags, TenantConfiguration tenantConfiguration, string environment, EventContext @event)
        {
            Dictionary<string, bool> result = new();
            foreach (string featureFlag in featureFlags)
            {
                DateTime startedAt = DateTime.UtcNow;
                bool isEnabled = await _singleFlagEvaluator.IsEnabled(featureFlag, tenantConfiguration, environment).ConfigureAwait(false);
                DateTime completedAt = DateTime.UtcNow;
                @event.AddProperty(featureFlag, isEnabled.ToString());
                @event.AddProperty(new StringBuilder().Append(featureFlag).Append(":TimeTaken").ToString(), (completedAt - startedAt).TotalMilliseconds.ToString());
                result.Add(featureFlag, isEnabled);
            }
            return result;
        }
    }
}
