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
            TenantConfiguration tenantConfiguration = await _tenantConfigurationProvider.Get(applicationName);
            string context = GetContext();
            EventContext @event = CreateFeatureFlagsEvaluatedEvent(tenantConfiguration.Name, environment, context, "Azure", features);
            AddHttpContext(environment, tenantConfiguration);

            IEvaluationStrategy strategy = _strategyBuilder.GetStrategy(features, tenantConfiguration);
            IDictionary<string, bool> results = await strategy.Evaluate(features, tenantConfiguration, environment, @event);
            
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
