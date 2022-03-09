using System.Linq;
using System.Collections.Generic;
using Microsoft.FeatureFlighting.Common.Config;

namespace Microsoft.FeatureFlighting.Core.Evaluation
{    
    // <inheritdoc/>
    internal class FeatureBatchBuilder : IFeatureBatchBuilder
    {
        // <inheritdoc/>
        public IEnumerable<IGrouping<int, string>> CreateBatches(IEnumerable<string> featureFlags, TenantConfiguration tenantConfiguration)
        {
            int batchSize = GetBatchSize(tenantConfiguration);
            IEnumerable<IGrouping<int, string>> featureFlagBatches = featureFlags.Select((flag, index) => new
            {
                Index = index,
                Flag = flag
            }).GroupBy(indexedFlag => indexedFlag.Index / batchSize, indexedFlag => indexedFlag.Flag).ToList();
            return featureFlagBatches;
        }

        private int GetBatchSize(TenantConfiguration tenantConfiguration)
        {
            return tenantConfiguration.Evaluation?.ParallelEvaluation == null
                ? ParallelEvaluationConfiguration.DefaultBatchSize
                : tenantConfiguration.Evaluation.ParallelEvaluation.BatchSize;
        }
    }
}
