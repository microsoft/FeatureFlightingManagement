using System;
using System.Linq;
using System.Threading.Tasks;
using AppInsights.EnterpriseTelemetry;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.FeatureManagement;
using System.Collections.Concurrent;
using AppInsights.EnterpriseTelemetry.Context;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Domain.Interfaces;
using Microsoft.FeatureFlighting.Services.Interfaces;

namespace Microsoft.FeatureFlighting.Domain
{
    public class FeatureFlagEvaluator : IFeatureFlagEvaluator
    {
        private readonly IFeatureManager _featureManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IBackwardCompatibleFeatureManager _backwardCompatibleFeatureManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        public FeatureFlagEvaluator(IFeatureManager featureManager, IBackwardCompatibleFeatureManager backwardCompatibleFeatureManager, IHttpContextAccessor httpContextAccessor, IConfiguration configuration, ILogger logger)
        {
            _featureManager = featureManager;
            _backwardCompatibleFeatureManager = backwardCompatibleFeatureManager;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<Dictionary<string, bool>> Evaluate(string applicationName, string environment, List<string> featureFlags)
        {
            var performanceContext = new HighPrecisionPerformanceContext("Feature Flag Evaluation Time");
            featureFlags = featureFlags.Distinct().ToList();
            var componentName = GetComponentName(applicationName);
            var incompatibleFeatureFlags = featureFlags
                .Where(feature => _backwardCompatibleFeatureManager.IsBackwardCompatibityRequired(componentName, environment, feature))
                .Distinct()
                .ToList();
            var compatibleFeatureFlags = featureFlags
                .Except(incompatibleFeatureFlags)
                .ToList();
            var context =
                _httpContextAccessor.HttpContext.Request.Headers.Any(header => header.Key.ToLowerInvariant() == Constants.Flighting.FLIGHT_CONTEXT_HEADER.ToLowerInvariant())
                ? _httpContextAccessor.HttpContext.Request.Headers.First(header => header.Key.ToLowerInvariant() == Constants.Flighting.FLIGHT_CONTEXT_HEADER.ToLowerInvariant()).Value.FirstOrDefault()
                : null;

            var compatibleFeatureResults = await EvaluateCompatibleFlags(componentName, environment, compatibleFeatureFlags, context);
            var incompatibleFeatureResults = await EvaluateIncompatibleFlags(componentName, environment, incompatibleFeatureFlags, context);

            var results = new Dictionary<string, bool>();
            foreach (var result in compatibleFeatureResults)
                results[result.Key] = result.Value;
            foreach (var result in incompatibleFeatureResults)
                results[result.Key] = result.Value;

            performanceContext.Stop();
            _logger.Log(performanceContext);
            return results;
        }

        private string GetComponentName(string appName)
        {
            var compatibleHeaderKey = $"BackwardCompatibleFlags:ReverseTenantMapping:{appName.ToUpperInvariant()}";
            var componentName = _configuration[compatibleHeaderKey];
            if (!string.IsNullOrWhiteSpace(componentName))
            {
                var message = new MessageContext()
                {
                    Message = "Incompatible App Name",
                    TraceLevel = TraceLevel.Warning
                };
                message.AddProperty("IncompatibleAppName", appName);
                message.AddProperty("ConvertedComponentName", componentName);
                _logger.Log(message);
                return componentName;
            }
            return appName;
        }

        private async Task<IDictionary<string, bool>> EvaluateCompatibleFlags(string componentName, string environment, List<string> compatibleFeatureFlags, string context)
        {
            var performanceContext = new HighPrecisionPerformanceContext("Compatible Feature Flags (Azure) Evaluation Time");
            performanceContext.AddProperty("FlagsCount", compatibleFeatureFlags.Count.ToString());
            var @event = CreateFeatureFlagsEvaluatedEvent(componentName, environment, context, "Azure", compatibleFeatureFlags.Count);

            if (compatibleFeatureFlags == null || !compatibleFeatureFlags.Any())
                return new Dictionary<string, bool>();

            var result = new ConcurrentDictionary<string, bool>();
            _httpContextAccessor.HttpContext.Items[Constants.Flighting.FEATURE_ENV_PARAM] = environment;
            _httpContextAccessor.HttpContext.Items[Constants.Flighting.FEATURE_APP_PARAM] = componentName;

            @event.AddProperty("DataSource", "Azure");
            var evaluationTasks = new List<Task>();

            foreach (var feature in compatibleFeatureFlags)
            {
                evaluationTasks.Add(Task.Run(async () =>
                {
                    var startedAt = DateTime.UtcNow;
                    var isEnabled = await _featureManager.IsEnabledAsync(Utility.GetFeatureFlagId(componentName.ToLowerInvariant(), environment.ToLowerInvariant(), feature)).ConfigureAwait(false);
                    var completedAt = DateTime.UtcNow;
                    @event.AddProperty(feature, isEnabled.ToString());
                    @event.AddProperty($"{feature}:TimeTaken", (completedAt - startedAt).TotalMilliseconds.ToString());
                    result.AddOrUpdate(feature, isEnabled, (feature, eval) => eval);
                }));
            }
            await Task.WhenAll(evaluationTasks).ConfigureAwait(false);

            performanceContext.Stop();
            @event.AddProperty("TotalTimeTaken", performanceContext.GetEllapsedMilliseconds().ToString());
            _logger.Log(@event);
            _logger.Log(performanceContext);

            return result;
        }

        private async Task<IDictionary<string, bool>> EvaluateIncompatibleFlags(string componentName, string environment, List<string> incompatibleFeatureFlags, string context)
        {
            var performanceContext = new PerformanceContext("Incompatible Feature Flags (Carbon) Evaluation Time");
            performanceContext.AddProperty("FlagsCount", incompatibleFeatureFlags.Count.ToString());
            var @event = CreateFeatureFlagsEvaluatedEvent(componentName, environment, context, "Carbon", incompatibleFeatureFlags.Count);

            var result = new Dictionary<string, bool>();
            if (incompatibleFeatureFlags == null || !incompatibleFeatureFlags.Any())
                return result;

            var responses = await _backwardCompatibleFeatureManager.IsEnabledAsync(
                componentName,
                environment,
                incompatibleFeatureFlags,
                context);
            if (responses != null && responses.Any())
            {
                var timeTaken = performanceContext.GetCurrentEllapsedMilliSeconds / incompatibleFeatureFlags.Count;
                foreach (var response in responses)
                {
                    result.Add(response.Key, response.Value);
                    @event.AddProperty(response.Key, response.Value.ToString());
                    @event.AddProperty($"{response.Key}:TimeTaken", timeTaken.ToString());
                }
            }

            performanceContext.Stop();
            @event.AddProperty("TotalTimeTaken", performanceContext.GetEllapsedMilliseconds().ToString());
            _logger.Log(@event);
            _logger.Log(performanceContext);
            return result;
        }

        private EventContext CreateFeatureFlagsEvaluatedEvent(string componentName, string environment, string context, string dataSource, int flagCount)
        {
            var @event = new EventContext()
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
