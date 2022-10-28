using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using AppInsights.EnterpriseTelemetry;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Core.Spec;
using AppInsights.EnterpriseTelemetry.Context;
using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.FeatureFlighting.Core.Evaluation;
using System.Collections.ObjectModel;

namespace Microsoft.FeatureFlighting.Core
{
    // <inheritdoc>
    internal class FeatureFlagEvaluator : IFeatureFlagEvaluator
    {
        private readonly IEvaluationStrategyBuilder _strategyBuilder;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITenantConfigurationProvider _tenantConfigurationProvider;
        private readonly ILogger _logger;

        public FeatureFlagEvaluator(IEvaluationStrategyBuilder strategyBuilder, IHttpContextAccessor httpContextAccessor, ITenantConfigurationProvider tenantConfigurationProvider, ILogger logger)
        {
            _strategyBuilder = strategyBuilder;
            _httpContextAccessor = httpContextAccessor;
            _tenantConfigurationProvider = tenantConfigurationProvider;
            _logger = logger;
        }

        // <inheritdoc>
        public async Task<IDictionary<string, bool>> Evaluate(string applicationName, string environment, List<string> features)
        {
            if (features == null || !features.Any())
                return new Dictionary<string, bool>();

            PerformanceContext performanceContext = new("Feature Flag Evaluation Time");
            features = features.Distinct().ToList();
            IEnumerable<TenantConfiguration> tenantConfigurations = _tenantConfigurationProvider.GetAllTenants();
            string context = GetContext();

            EventContext @event = CreateFeatureFlagsEvaluatedEvent(applicationName, environment, context, "Azure", features);

            Dictionary<string, List<string>> featureToTenantMap = new Dictionary<string, List<string>>(); 
            foreach (var feature in features)
            {
                var featureSplit = feature.Split(':');
                // For Verge:Snap where Verge is Tenant Name and Snap is the Feature Name
                if (featureSplit.Length == 2)   
                {
                    string tenantName = featureSplit[0], featureName = featureSplit[1]; 
                    if (!featureToTenantMap.ContainsKey(featureSplit[0]))
                        featureToTenantMap[tenantName] = new List<string>();
                    featureToTenantMap[tenantName].Add(featureName);
                }
                else
                {
                    if(!featureToTenantMap.ContainsKey(applicationName))
                        featureToTenantMap[applicationName] = new List<string>();
                    featureToTenantMap[applicationName].Add(feature);
                }
            }

            IEvaluationStrategy strategy=null;
            IDictionary<string, bool> results = new Dictionary<string, bool>();
            foreach (var tenantName in featureToTenantMap.Keys)
            {
                TenantConfiguration tenantConfiguration = tenantConfigurations.FirstOrDefault(t => string.Equals(t.Name,tenantName, System.StringComparison.OrdinalIgnoreCase));
                if (tenantConfiguration == null)
                    continue; 
                AddHttpContext(environment, tenantConfiguration);
                strategy = _strategyBuilder.GetStrategy(featureToTenantMap[tenantConfiguration.Name], tenantConfiguration);
                var featureEvaluationResult = await strategy.Evaluate(featureToTenantMap[tenantConfiguration.Name], tenantConfiguration, environment, @event);
                bool isDefaultTenant = string.Equals(tenantConfiguration.Name, applicationName, System.StringComparison.OrdinalIgnoreCase);

                foreach (var evalResult in featureEvaluationResult)
                {
                    var featureKey = isDefaultTenant ? evalResult.Key : $"{tenantConfiguration.Name}:{evalResult.Key}";
                    if (!results.ContainsKey(featureKey))
                    {
                        results.Add(featureKey, evalResult.Value);
                    }
                    else
                    {
                        results[featureKey] = results[featureKey] || evalResult.Value;
                    }
                }

            } 

            LogEvaluationResults(performanceContext, @event, strategy);
            return results;
        }

        private string GetContext()
        {
            return _httpContextAccessor.HttpContext.Request.Headers.Any(header => header.Key.ToLowerInvariant() == Constants.Flighting.FLIGHT_CONTEXT_HEADER.ToLowerInvariant())
                ? _httpContextAccessor.HttpContext.Request.Headers.First(header => header.Key.ToLowerInvariant() == Constants.Flighting.FLIGHT_CONTEXT_HEADER.ToLowerInvariant()).Value.FirstOrDefault()
                : null;
        }

        private EventContext CreateFeatureFlagsEvaluatedEvent(string componentName, string environment, string context, string dataSource, IEnumerable<string> features)
        {
            EventContext @event = new()
            {
                EventName = "Flighting:FeatureFlags:Evaluated"
            };
            @event.AddProperty("CallerComponent", componentName);
            @event.AddProperty("EvaluationEnvironment", environment);
            @event.AddProperty("DataSource", dataSource);
            @event.AddProperty("Context", context);
            @event.AddProperty("FlagsCount", features.Count().ToString());
            @event.AddProperty("Features", string.Join(',', features));
            return @event;
        }

        private void AddHttpContext(string environment, TenantConfiguration tenantConfiguration)
        {
            _httpContextAccessor.HttpContext.Items[Constants.Flighting.FEATURE_ENV_PARAM] = environment;
            _httpContextAccessor.HttpContext.Items[Constants.Flighting.FEATURE_APP_PARAM] = tenantConfiguration.Name;
            _httpContextAccessor.HttpContext.Items[Constants.Flighting.FEATURE_ADD_DISABLED_CONTEXT] =
                tenantConfiguration.Evaluation.AddDisabledContext
                || _httpContextAccessor.HttpContext.Request.Headers.GetOrDefault(Constants.Flighting.FLIGHT_ADD_RESULT_CONTEXT_HEADER, bool.FalseString).ToString().ToLowerInvariant() == bool.TrueString.ToLowerInvariant();
            _httpContextAccessor.HttpContext.Items[Constants.Flighting.FEATURE_ADD_ENABLED_CONTEXT] =
                tenantConfiguration.Evaluation.AddEnabledContext
                || _httpContextAccessor.HttpContext.Request.Headers.GetOrDefault(Constants.Flighting.FLIGHT_ADD_RESULT_CONTEXT_HEADER, bool.FalseString).ToString().ToLowerInvariant() == bool.TrueString.ToLowerInvariant();
        }

        private void LogEvaluationResults(PerformanceContext performanceContext, EventContext @event, IEvaluationStrategy strategy)
        {
            performanceContext.Stop();
            @event.AddProperty("TotalTimeTaken", (performanceContext.EndTime - performanceContext.StartTime).TotalMilliseconds.ToString());
            @event.AddProperty("EvaluationStrategy", strategy.GetType().Name);
            _logger.Log(performanceContext);
            _logger.Log(@event);
        }
    }
}
