using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.FeatureManagement;
using Exception = System.Exception;
using AppInsights.EnterpriseTelemetry;
using Microsoft.Extensions.Primitives;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Core.Spec;
using AppInsights.EnterpriseTelemetry.Context;
using Microsoft.FeatureFlighting.Core.Operators;
using Microsoft.FeatureFlighting.Common.AppExceptions;
using static Microsoft.FeatureFlighting.Common.Constants;

namespace Microsoft.FeatureFlighting.Core.FeatureFilters
{
    [FilterAlias(FilterKeys.RuleEngine)]
    public class RuleEngineFilter : IFeatureFilter
    {
        private readonly IRulesEngineManager _rulesEngineManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        public RuleEngineFilter(IRulesEngineManager ruleEngineManager, IHttpContextAccessor httpContextAccessor, IConfiguration configuration, ILogger logger)
        {
            _rulesEngineManager = ruleEngineManager ?? throw new ArgumentNullException(nameof(ruleEngineManager));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger;
        }

        public async Task<bool> EvaluateAsync(FeatureFilterEvaluationContext context)
        {
            LoggerTrackingIds trackingIds = _httpContextAccessor.HttpContext.Items.ContainsKey(Flighting.FLIGHT_TRACKER_PARAM)
                 ? JsonSerializer.Deserialize<LoggerTrackingIds>(_httpContextAccessor.HttpContext.Items[Flighting.FLIGHT_TRACKER_PARAM].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                 : new LoggerTrackingIds();

            try
            {
                FilterSettings filterSettings = context.Parameters.Get<FilterSettings>() ?? new FilterSettings();
                if (!ValidateFilterSettings(filterSettings, FilterKeys.RuleEngine, trackingIds))
                    return false;

                Operator op = (Operator)Enum.Parse(typeof(Operator), filterSettings.Operator, true);
                string workflowName = filterSettings.Value;
                string tenant = _httpContextAccessor.HttpContext.Items[Flighting.FEATURE_APP_PARAM]?.ToString();
                IRulesEngineEvaluator evaluator = await _rulesEngineManager.Build(tenant, workflowName, trackingIds);
                if (evaluator == null)
                    throw new RuleEngineException(workflowName, tenant, "Rule Evaluation could not be created. Ensure that BRE is enabled for the tenant", "FeatureFlighting.RuleEngineFilter.EvaluateAsync", trackingIds.CorrelationId, trackingIds.TransactionId);

                Dictionary<string, object> flightContext = GetFlightContext(trackingIds);
                EvaluationResult evaluationResult = await evaluator.Evaluate(flightContext, trackingIds);
                if (op == Operator.Evaluates)
                    return evaluationResult.Result;
                return !evaluationResult.Result;
            }
            catch (RuleEngineException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new EvaluationException(
                    message: $"There was an error in evaluating the filter {FilterKeys.RuleEngine}. See the inner exception for more details.",
                    innerException: exception,
                    correlationId: trackingIds.CorrelationId,
                    source: "FeatureFlighting.RuleEngine.EvaluateAsync",
                    transactionId: trackingIds.TransactionId);
            }
        }

        private Dictionary<string, object> GetFlightContext(LoggerTrackingIds trackingIds)
        {
            Dictionary<string, object> contextParams = new(StringComparer.InvariantCultureIgnoreCase);

            if (_httpContextAccessor.HttpContext.Request.Headers.TryGetValue(Flighting.FLIGHT_CONTEXT_HEADER, out StringValues flightContext))
            {
                contextParams = JsonSerializer.Deserialize<Dictionary<string, object>>(flightContext);
            }
            else
            {
                _logger.Log(new ExceptionContext()
                {
                    Exception = new Exception(Logging.MissingFlightingContextHeader),
                    CorrelationId = trackingIds.CorrelationId,
                    TransactionId = trackingIds.TransactionId
                });
            }
            contextParams = contextParams.ToDictionary(item => item.Key.ToUpperInvariant(), item => item.Value);

            var defaultContextParam = _configuration.GetSection("FlightingDefaultContextParams:ContextParam").Value.Split(",");
            foreach (var contextParamPair in defaultContextParam)
            {
                var contextParam = contextParamPair.Split(":");
                string key = contextParam[0].ToUpperInvariant();
                contextParams.AddOrUpdate(key, contextParam[1]);
            }

            DateTime date = Convert.ToDateTime(DateTime.UtcNow.ToString("MM/dd/yyyy"));
            string currentTimestamp = Convert.ToString((double)date.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMilliseconds);
            contextParams.AddOrUpdate(FilterKeys.Date.ToUpperInvariant(), currentTimestamp);

            return contextParams;
        }

        private bool ValidateFilterSettings(FilterSettings settings, string filterName, LoggerTrackingIds trackingIds)
        {
            if (string.IsNullOrWhiteSpace(settings.Value) ||
                string.IsNullOrWhiteSpace(settings.Operator) ||
                (int.TryParse(settings.StageId, out int stage) && stage < 0))
            {
                var context = new ExceptionContext()
                {
                    Exception = new Exception(Logging.InvalidFilterSettings),
                    CorrelationId = trackingIds.CorrelationId,
                    TransactionId = trackingIds.TransactionId
                };
                context.AddProperty(Flighting.FEATURE_FILTER_PARAM, filterName);
                context.AddProperty(Flighting.FEATURE_ENV_PARAM, _httpContextAccessor.HttpContext.Items[Flighting.FEATURE_ENV_PARAM]?.ToString());
                context.AddProperty(Flighting.FEATURE_APP_PARAM, _httpContextAccessor.HttpContext.Items[Flighting.FEATURE_APP_PARAM]?.ToString());
                _logger.Log(context);
                return false;
            }

            return bool.TryParse(settings.IsActive, out bool isStageACtive) && isStageACtive;
        }
    }
}
