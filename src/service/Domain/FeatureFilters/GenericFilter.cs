using System.Threading.Tasks;
using AppInsights.EnterpriseTelemetry;
using Microsoft.AspNetCore.Http;
using Microsoft.FeatureManagement;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Core.Spec;
using static Microsoft.FeatureFlighting.Common.Constants;

namespace Microsoft.FeatureFlighting.Core.FeatureFilters
{
    [FilterAlias(FilterKeys.Generic)]
    public class GenericFilter : BaseFilter, IFeatureFilter
    {
        protected override string FilterType => FilterKeys.Generic;
        public GenericFilter(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, ILogger logger, IOperatorEvaluatorStrategy evaluatorStrategy) : base(configuration, httpContextAccessor, logger, evaluatorStrategy)
        {
        }

        public Task<bool> EvaluateAsync(FeatureFilterEvaluationContext context)
        {
            return EvaluateFlightingContextAsync(context);
        }
    }
}
