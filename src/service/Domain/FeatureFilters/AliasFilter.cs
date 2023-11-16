using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.FeatureManagement;
using AppInsights.EnterpriseTelemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Core.Spec;
using static Microsoft.FeatureFlighting.Common.Constants;

namespace Microsoft.FeatureFlighting.Core.FeatureFilters
{
    [FilterAlias(FilterKeys.Alias)]
    public class AliasFilter : BaseFilter, IFeatureFilter, IFilterParametersBinder
    {
        protected override string FilterType => FilterKeys.Alias;
        public AliasFilter(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, ILogger logger, IOperatorStrategy evaluatorStrategy) : base(configuration ,httpContextAccessor, logger, evaluatorStrategy)
        {
        }

        public object BindParameters(IConfiguration filterParameters)
        {
            return filterParameters.Get<FilterSettings>();
        }

        public Task<bool> EvaluateAsync(FeatureFilterEvaluationContext context)
        {
            return EvaluateFlightingContextAsync(context, FlightingContextParams.Alias);
        }
    }
}
