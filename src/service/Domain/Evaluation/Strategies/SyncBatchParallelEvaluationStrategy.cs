using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using AppInsights.EnterpriseTelemetry.Context;
using Microsoft.FeatureFlighting.Common.Config;

namespace Microsoft.FeatureFlighting.Core.Evaluation
{
    /// <summary>
    /// Batches features, and evaluates the batches in order (with flags in each batch being evaluated synchronously)
    /// </summary>
    internal class SyncBatchParallelEvaluationStrategy : IEvaluationStrategy, IBatchEvaluationStrategy
    {
        private readonly IAsyncEvaluationStrategy _asyncEvaluationStrategy;
        private readonly IFeatureBatchBuilder _featureBatchBuilder;

        public SyncBatchParallelEvaluationStrategy(IAsyncEvaluationStrategy asyncEvaluationStrategy, IFeatureBatchBuilder featureBatchBuilder)
        {
            _asyncEvaluationStrategy = asyncEvaluationStrategy;
            _featureBatchBuilder = featureBatchBuilder;
        }

        public async Task<IDictionary<string, bool>> Evaluate(IEnumerable<string> features, TenantConfiguration tenantConfiguration, string environment, EventContext @event)
        {
            IEnumerable<IGrouping<int, string>> batches = _featureBatchBuilder.CreateBatches(features, tenantConfiguration);
            IDictionary<string, bool> results = new Dictionary<string, bool>();
            foreach (IGrouping<int, string> batch in batches)
            {
                IDictionary<string, bool> currentGroupResult = await _asyncEvaluationStrategy.Evaluate(batch.ToList(), tenantConfiguration, environment, @event);
                results.Merge(currentGroupResult);
            }
            return results;
        }
    }
}
