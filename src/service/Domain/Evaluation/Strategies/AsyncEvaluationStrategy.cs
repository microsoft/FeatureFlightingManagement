using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using AppInsights.EnterpriseTelemetry.Context;
using Microsoft.FeatureFlighting.Common.Config;
using System.Linq;

namespace Microsoft.FeatureFlighting.Core.Evaluation
{
    /// <summary>
    /// Executes feature flags asynchronously
    /// </summary>
    internal class AsyncEvaluationStrategy : IEvaluationStrategy, IAsyncEvaluationStrategy
    {
        private readonly ISingleFlagEvaluator _singleFlagEvaluator;

        public AsyncEvaluationStrategy(ISingleFlagEvaluator singleFlagEvaluator)
        {
            _singleFlagEvaluator = singleFlagEvaluator;
        }

        public async Task<IDictionary<string, bool>> Evaluate(IEnumerable<string> features, IEnumerable<string> featureKeysOnAzure, TenantConfiguration tenantConfiguration, string environment, EventContext @event)
        {
            ConcurrentDictionary<string, bool> result = new();
            ConcurrentDictionary<string, string> telemetryProp = new();
            List<Task> evaluationTasks = new();
            foreach (string feature in features)
            {
                evaluationTasks.Add(Task.Run(async () =>
                {
                    var startedAt = DateTime.UtcNow;
                    bool isEnabled = await _singleFlagEvaluator.IsEnabled(feature, featureKeysOnAzure, tenantConfiguration, environment).ConfigureAwait(false);
                    var completedAt = DateTime.UtcNow;
                    telemetryProp.AddOrUpdate(feature, isEnabled.ToString());
                    telemetryProp.AddOrUpdate(new StringBuilder().Append(feature).Append(":TimeTaken").ToString(), (completedAt - startedAt).TotalMilliseconds.ToString());
                    result.AddOrUpdate(feature, isEnabled, (feature, eval) => eval);
                }));
            }
            await Task.WhenAll(evaluationTasks).ConfigureAwait(false);
            @event.AddProperties(telemetryProp.ToDictionary(kv => kv.Key, kv => kv.Value));
            return result;
        }
    }
}
