using System.Threading.Tasks;
using AppInsights.EnterpriseTelemetry;
using Microsoft.AspNetCore.Http;
using Microsoft.FeatureManagement;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Domain.Interfaces;
using static Microsoft.FeatureFlighting.Common.Constants;

namespace Microsoft.FeatureFlighting.Domain.FeatureFilters
{
    [FilterAlias(FilterKeys.Alias)]
    public class AliasFilter : BaseFilter, IFeatureFilter
    {
        protected override string FilterType => FilterKeys.Alias;
        public AliasFilter(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, ILogger logger, IOperatorEvaluatorStrategy evaluatorStrategy) : base(configuration ,httpContextAccessor, logger, evaluatorStrategy)
        {
        }

        public Task<bool> EvaluateAsync(FeatureFilterEvaluationContext context)
        {
            return EvaluateFlightingContextAsync(context, FlightingContextParams.Alias);
        }
    }
}
