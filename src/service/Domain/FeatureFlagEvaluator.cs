using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.FeatureManagement;
using System.Collections.Concurrent;
using AppInsights.EnterpriseTelemetry;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Core.Spec;
using AppInsights.EnterpriseTelemetry.Context;
using Microsoft.FeatureFlighting.Common.Config;

namespace Microsoft.FeatureFlighting.Core
{
    // <inheritdoc>
    public class FeatureFlagEvaluator : IFeatureFlagEvaluator
    {
        private readonly IFeatureManager _featureManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITenantConfigurationProvider _tenantConfigurationProvider;
        private readonly ILogger _logger;

        public FeatureFlagEvaluator(IFeatureManager featureManager, IHttpContextAccessor httpContextAccessor, ITenantConfigurationProvider tenantConfigurationProvider, ILogger logger)
        {
            _featureManager = featureManager;
            _httpContextAccessor = httpContextAccessor;
            _tenantConfigurationProvider = tenantConfigurationProvider;
            _logger = logger;
        }

        // <inheritdoc>
        public async Task<IDictionary<string, bool>> Evaluate(string applicationName, string environment, List<string> featureFlags)
        {
            if (featureFlags == null || !featureFlags.Any())
                return new Dictionary<string, bool>();

            PerformanceContext performanceContext = new("Feature Flag Evaluation Time");
            featureFlags = featureFlags.Distinct().ToList();
            TenantConfiguration tenantConfiguration = await _tenantConfigurationProvider.Get(applicationName);
            string context = GetContext();
            var @event = CreateFeatureFlagsEvaluatedEvent(tenantConfiguration.Name, environment, context, "Azure", featureFlags.Count);

            ConcurrentDictionary<string, bool> result = new();
            _httpContextAccessor.HttpContext.Items[Constants.Flighting.FEATURE_ENV_PARAM] = environment;
            _httpContextAccessor.HttpContext.Items[Constants.Flighting.FEATURE_APP_PARAM] = tenantConfiguration.Name;
            _httpContextAccessor.HttpContext.Items[Constants.Flighting.FEATURE_ADD_DISABLED_CONTEXT] = tenantConfiguration.Evaluation.AddDisabledContext;
            var evaluationTasks = new List<Task>();

            foreach (string featureFlag in featureFlags)
            {
                evaluationTasks.Add(Task.Run(async () =>
                {
                    var startedAt = DateTime.UtcNow;
                    bool isEnabled = await _featureManager.IsEnabledAsync(FlagUtilities.GetFeatureFlagId(tenantConfiguration.Name.ToLowerInvariant(), environment.ToLowerInvariant(), featureFlag));
                    var completedAt = DateTime.UtcNow;
                    @event.AddProperty(featureFlag, isEnabled.ToString());
                    @event.AddProperty($"{featureFlag}:TimeTaken", (completedAt - startedAt).TotalMilliseconds.ToString());
                    result.AddOrUpdate(featureFlag, isEnabled, (feature, eval) => eval);
                }));
            }
            await Task.WhenAll(evaluationTasks).ConfigureAwait(false);

            performanceContext.Stop();
            _logger.Log(performanceContext);
            _logger.Log(@event);
            return result;
        }
        
        private string GetContext()
        {
            return _httpContextAccessor.HttpContext.Request.Headers.Any(header => header.Key.ToLowerInvariant() == Constants.Flighting.FLIGHT_CONTEXT_HEADER.ToLowerInvariant())
                ? _httpContextAccessor.HttpContext.Request.Headers.First(header => header.Key.ToLowerInvariant() == Constants.Flighting.FLIGHT_CONTEXT_HEADER.ToLowerInvariant()).Value.FirstOrDefault()
                : null;
        }

        private EventContext CreateFeatureFlagsEvaluatedEvent(string componentName, string environment, string context, string dataSource, int flagCount)
        {
            EventContext @event = new()
            {
                EventName = "Flighting:FeatureFlags:Evaluated"
            };
            @event.AddProperty("CallerComponent", componentName);
            @event.AddProperty("EvaluationEnvironment", environment);
            @event.AddProperty("DataSource", dataSource);
            @event.AddProperty("Context", context);
            @event.AddProperty("FlagsCount", flagCount.ToString());
            return @event;
        }
    }
}
