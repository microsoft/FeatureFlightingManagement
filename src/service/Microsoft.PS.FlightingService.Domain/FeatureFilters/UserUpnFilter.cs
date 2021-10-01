using System.Threading.Tasks;
using AppInsights.EnterpriseTelemetry;
using Microsoft.AspNetCore.Http;
using Microsoft.FeatureManagement;
using Microsoft.Extensions.Configuration;
using Microsoft.PS.FlightingService.Domain.Interfaces;
using static Microsoft.PS.FlightingService.Common.Constants;

namespace Microsoft.PS.FlightingService.Domain.FeatureFilters
{
    [FilterAlias(FilterKeys.UserUpn)]
    public class UserUpnFilter : BaseFilter, IFeatureFilter
    {
        protected override string FilterType => FilterKeys.UserUpn;
        public UserUpnFilter(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, ILogger logger, IOperatorEvaluatorStrategy evaluatorStrategy) : base(configuration, httpContextAccessor, logger, evaluatorStrategy)
        { }

        public Task<bool> EvaluateAsync(FeatureFilterEvaluationContext context)
        {
            return EvaluateFlightingContextAsync(context, FlightingContextParams.Upn);
        }
    }
}
