using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.FeatureManagement;
using AppInsights.EnterpriseTelemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Core.Spec;
using static Microsoft.FeatureFlighting.Common.Constants;
using System.Collections.Generic;

namespace Microsoft.FeatureFlighting.Core.FeatureFilters
{
    [FilterAlias(FilterKeys.Alias)]
    public class AliasFilter : BaseFilter, IContextualFeatureFilter<EvaluationContext>
    {
        protected override string FilterType => FilterKeys.Alias;
        public AliasFilter(IConfiguration configuration, ILogger logger, IOperatorStrategy evaluatorStrategy) : base(configuration, logger, evaluatorStrategy)
        {
        }

        public Task<bool> EvaluateAsync(FeatureFilterEvaluationContext featureContext ,EvaluationContext evaluationContext)
        {
            return EvaluateFlightingContextAsync(featureContext, evaluationContext, FlightingContextParams.Alias);
        }
    }
}
