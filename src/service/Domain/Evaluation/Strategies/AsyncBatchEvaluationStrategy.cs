using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using AppInsights.EnterpriseTelemetry.Context;
using Microsoft.FeatureFlighting.Common.Config;

namespace Microsoft.FeatureFlighting.Core.Evaluation
{
    /// <summary>
    /// Batches the features into groups and evaluates all the batches in parallel (with features in each batch being evaluated synchronously)
    /// </summary>
    internal class AsyncBatchEvaluationStrategy : IEvaluationStrategy, IBatchEvaluationStrategy
    {
        private readonly ISyncEvaluationStrategy _syncEvaluationStrategy;
        private readonly IFeatureBatchBuilder _featureBatchBuilder;

        public AsyncBatchEvaluationStrategy(ISyncEvaluationStrategy syncEvaluationStrategy, IFeatureBatchBuilder featureBatchBuilder)
        {
            _syncEvaluationStrategy = syncEvaluationStrategy;
            _featureBatchBuilder = featureBatchBuilder;
        }

        public async Task<IDictionary<string, bool>> Evaluate(IEnumerable<string> features, TenantConfiguration tenantConfiguration, string environment, EventContext @event)
        {
            IEnumerable<IGrouping<int,string>> batches = _featureBatchBuilder.CreateBatches(features, tenantConfiguration);
            IDictionary<string, bool> results = new Dictionary<string, bool>();
            List<Task<IDictionary<string, bool>>> evaluationTasks = new();

            foreach (IGrouping<int, string> batch in batches)
            {
                evaluationTasks.Add(_syncEvaluationStrategy.Evaluate(batch.ToList(), tenantConfiguration, environment, @event));
            }

            while (evaluationTasks.Any())
            {
                Task<IDictionary<string, bool>> completedTask = await Task.WhenAny(evaluationTasks);
                results.Merge(completedTask.Result);
                evaluationTasks.Remove(completedTask);
            }
            return results;
        }
    }
}
