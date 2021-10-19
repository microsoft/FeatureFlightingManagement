using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AppInsights.EnterpriseTelemetry;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.FeatureManagement;
using Exception = System.Exception;
using Microsoft.Extensions.Primitives;
using AppInsights.EnterpriseTelemetry.Context;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Domain.Interfaces;
using Microsoft.FeatureFlighting.Common.AppExcpetions;
using static Microsoft.FeatureFlighting.Common.Constants;
using Microsoft.FeatureFlighting.Domain.Evaluators;

namespace Microsoft.FeatureFlighting.Domain.FeatureFilters
{
    public abstract class BaseFilter
    {
        protected readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly IOperatorEvaluatorStrategy _evaluatorStrategy;

        protected abstract string FilterType { get; }

        public BaseFilter(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, ILogger logger, IOperatorEvaluatorStrategy evaluatorStrategy)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _logger = logger;
            _configuration = configuration;
            _evaluatorStrategy = evaluatorStrategy;
        }

        public async Task<bool> EvaluateFlightingContextAsync(FeatureFilterEvaluationContext featureFlag, string defaultFilterKey = null)
        {
            var trackingIds = _httpContextAccessor.HttpContext.Items.ContainsKey(Flighting.FLIGHT_TRACKER_PARAM)
                 ? JsonSerializer.Deserialize<LoggerTrackingIds>(_httpContextAccessor.HttpContext.Items[Flighting.FLIGHT_TRACKER_PARAM].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                 : new LoggerTrackingIds();
            try
            {
                var contextValue = string.Empty;

                FilterSettings settings = GetFilterSettings(featureFlag);
                string filterKey = !string.IsNullOrWhiteSpace(settings.FlightContextKey)
                    ? settings.FlightContextKey.ToUpperInvariant()
                    : defaultFilterKey.ToUpperInvariant();

                Dictionary<string, object> contextParams = GetFlightContext(filterKey, trackingIds);
                if (!IsFilterKeyPresentInContext(contextParams, filterKey))
                    return false;

                if (!ValidateFilterSettings(settings, filterKey, trackingIds))
                    return false;

                Operator op = (Operator)Enum.Parse(typeof(Operator), settings.Operator, true);
                contextValue = (contextParams[filterKey]!=null)? contextParams[filterKey].ToString().ToLowerInvariant() : string.Empty;
                string settingsValue = settings.Value.ToLowerInvariant();
                BaseOperatorEvaluator evaluator = _evaluatorStrategy.Get(op);

                if (evaluator == null)
                    throw new Exception($"No evaluator has been assigned for operator - {Enum.GetName(typeof(Operator), op)}");

                EvaluationResult evaluationResult = await evaluator.Evaluate(settingsValue, contextValue, FilterType, trackingIds);
                return evaluationResult.Result;
            }
            catch (Exception ex)
            {
                throw new EvaluationException(
                    message: ex.Message,
                    correlationId: trackingIds.CorrelationId,
                    transactionId: trackingIds.TransactionId,
                    failedMethod: $"{FilterType}-EvaluateAsync",
                    innerException: ex);
            }
        }

        private Dictionary<string, object> GetFlightContext(string filterKey, LoggerTrackingIds trackingIds)
        {
            var contextParams = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);

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

            if (FilterType.ToLowerInvariant() == FilterKeys.Date.ToLowerInvariant() && !contextParams.ContainsKey(filterKey.ToUpperInvariant()))
            {
                DateTime date = Convert.ToDateTime(DateTime.UtcNow.ToString("MM/dd/yyyy"));
                string currentTimestamp = Convert.ToString((double)date.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMilliseconds);
                contextParams.AddOrUpdate(filterKey.ToUpperInvariant(), currentTimestamp);
            }

            return contextParams;
        }

        

        private FilterSettings GetFilterSettings(FeatureFilterEvaluationContext featureFlag)
        {
            return featureFlag.Parameters.Get<FilterSettings>() ?? new FilterSettings();
        }

        private bool IsFilterKeyPresentInContext(Dictionary<string, object> contextParams, string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;
            return contextParams.Any() && contextParams.ContainsKey(key);
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
