using Microsoft.FeatureFlighting.Common.Config;
using System.Collections.Generic;

namespace Microsoft.FeatureFlighting.Core.Evaluation
{
    internal interface IEvaluationStrategyBuilder
    {
        IEvaluationStrategy GetStrategy(IEnumerable<string> features, TenantConfiguration tenantConfiguration);
        bool IsBatchedStrategy();
    }
}