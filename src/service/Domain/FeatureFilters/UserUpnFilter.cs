﻿using System.Threading.Tasks;
using AppInsights.EnterpriseTelemetry;
using Microsoft.AspNetCore.Http;
using Microsoft.FeatureManagement;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Core.Spec;
using static Microsoft.FeatureFlighting.Common.Constants;

namespace Microsoft.FeatureFlighting.Core.FeatureFilters
{
    [FilterAlias(FilterKeys.UserUpn)]
    public class UserUpnFilter : BaseFilter, IContextualFeatureFilter<EvaluationContext>
    {
        protected override string FilterType => FilterKeys.UserUpn;
        public UserUpnFilter(IConfiguration configuration, ILogger logger, IOperatorStrategy evaluatorStrategy) : base(configuration, logger, evaluatorStrategy)
        { }

        public Task<bool> EvaluateAsync(FeatureFilterEvaluationContext context, EvaluationContext evaluationContext)
        {
            return EvaluateFlightingContextAsync(context, evaluationContext,FlightingContextParams.Upn);
        }
    }
}
