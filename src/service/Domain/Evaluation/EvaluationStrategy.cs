using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.FeatureFlighting.Common;
using AppInsights.EnterpriseTelemetry.Context;
using Microsoft.FeatureFlighting.Common.Config;

namespace Microsoft.FeatureFlighting.Core.Evaluation
{
    internal class EvaluationStrategyBuilder : IEvaluationStrategyBuilder
    {
        private readonly IDictionary<Type, IEvaluationStrategy> _strategies;

        public EvaluationStrategyBuilder(IEnumerable<IEvaluationStrategy> strategies)
        {
            _strategies = new Dictionary<Type, IEvaluationStrategy>();
            foreach (IEvaluationStrategy strategy in strategies.Distinct())
            {
                _strategies.Add(strategy.GetType(), strategy);
            }
        }

        public IEvaluationStrategy GetStrategy(IEnumerable<string> features, TenantConfiguration tenantConfiguration)
        {
            IEvaluationStrategy strategy = GetStrategy(tenantConfiguration.Evaluation?.ParallelEvaluation?.ParallelMode);
            if (strategy is IBatchEvaluationStrategy)
            {
                int batchSize = GetBatchSize(tenantConfiguration);
                if (batchSize > features.Count())
                {
                    strategy = strategy is AsyncBatchEvaluationStrategy
                        ? GetStrategy(Constants.EvaluationStrategies.None)
                        : GetStrategy(Constants.EvaluationStrategies.Full);
                }
            }
            return strategy;
        }

        private IEvaluationStrategy GetStrategy(string strategy)
        {
            return strategy?.ToUpperInvariant() switch
            {
                Constants.EvaluationStrategies.Full => _strategies[typeof(SyncEvaluationStrategy)],
                Constants.EvaluationStrategies.AsyncBatch => _strategies[typeof(AsyncBatchEvaluationStrategy)],
                Constants.EvaluationStrategies.SyncParallelBatch => _strategies[typeof(SyncBatchParallelEvaluationStrategy)],
                _ => _strategies[typeof(AsyncEvaluationStrategy)],
            };
        }

        private int GetBatchSize(TenantConfiguration tenantConfiguration)
        {
            return tenantConfiguration.Evaluation?.ParallelEvaluation != null
                ? tenantConfiguration.Evaluation.ParallelEvaluation.BatchSize
                : ParallelEvaluationConfiguration.DefaultBatchSize;
        }

        public bool IsBatchedStrategy()
        {
            throw new NotImplementedException();
        }
    }
}
