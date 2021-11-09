using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Core.FeatureFilters;
using Microsoft.FeatureFlighting.Core.Spec;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.FeatureFlighting.SDK
{
    public class FlightingSDKFlagEvaluator :IFlightingSDKFlagEvaluator
    {
        private readonly IConfiguration _configuration;
        private readonly IFeatureFlagEvaluator _featureFlagEvaluator;
        
        public FlightingSDKFlagEvaluator(IConfiguration configuration, IFeatureFlagEvaluator featureFlagEvaluator)
        {
            _configuration = configuration;
            _featureFlagEvaluator = featureFlagEvaluator;
        }
        public async Task<IDictionary<string, bool>> Evaluate(List<string> featureFlags, Dictionary<string,object> context, string correlationId= "" ,string transactionId = "")
        {
            string application = _configuration["FlightingSDK:Tenant"];
            string environment = _configuration["FlightingSDK:Environment"];
            bool addDisabledContext = _configuration["FlightingSDK:Evaluation:AddDisabledContext"].ToLowerInvariant() == bool.TrueString.ToLowerInvariant() ?true :false ;
            bool addEnabledContext = _configuration["FlightingSDK:Evaluation:AddEnabledContext"].ToLowerInvariant() == bool.TrueString.ToLowerInvariant() ? true : false;
            EvaluationContext evaluationContext = new EvaluationContext(context, environment, application, correlationId, transactionId, addEnabledContext, addDisabledContext);
            return await _featureFlagEvaluator.Evaluate(featureFlags, evaluationContext);
        }
    }
}
