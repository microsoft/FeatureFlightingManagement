using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.FeatureManagement;
using AppInsights.EnterpriseTelemetry;
using Microsoft.FeatureFlighting.Common;
using AppInsights.EnterpriseTelemetry.Context;
using Microsoft.FeatureFlighting.Common.Config;
using AppInsights.EnterpriseTelemetry.Exceptions;
using Microsoft.FeatureFlighting.Common.AppExceptions;
using Microsoft.FeatureFlighting.Core.Spec;
using Microsoft.FeatureFlighting.Common.Model;
using Microsoft.FeatureFlighting.Core.Queries;
using System.Linq;
using System.Transactions;
using CQRS.Mediatr.Lite;
using Microsoft.Extensions.Configuration;

namespace Microsoft.FeatureFlighting.Core.Evaluation
{

    /// <inheritdoc/>
    internal class SingleFlagEvaluator : ISingleFlagEvaluator
    {
        private readonly IFeatureManager _featureManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger _logger;
        private readonly IFeatureFlightResultCache _cache;
        private readonly IConfiguration _configuration;
        private readonly Dictionary<string, IList<string>> _featureFlightResultCacheConfigs;
        private readonly bool isCachingEnabled;

        public SingleFlagEvaluator(IFeatureManager featureManager, IHttpContextAccessor httpContextAccessor, ILogger logger, IFeatureFlightResultCache cache, IConfiguration configuration)
        {
            _featureManager = featureManager;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _cache = cache;
            _configuration = configuration;
            _featureFlightResultCacheConfigs = _configuration.GetSection("FeatureFlightResultCacheConfig").Get<Dictionary<string, IList<string>>>();
            isCachingEnabled = _configuration.GetSection("EnableFeatureFlightResultCaching").Get<bool>();
        }

        /// <inheritdoc/>
        public virtual async Task<bool> IsEnabled(string featureFlag, TenantConfiguration tenantConfiguration, string environment)
        {
            string correlationId = _httpContextAccessor.HttpContext.Request.Headers.GetOrDefault("x-correlationId", Guid.NewGuid().ToString()).ToString();
            string transactionId = _httpContextAccessor.HttpContext.Request.Headers.GetOrDefault("x-messageId", Guid.NewGuid().ToString()).ToString();

            try
            {
                var isFeatureCachingEnabled = _featureFlightResultCacheConfigs.Where(x => x.Key.ToLowerInvariant() == tenantConfiguration.Name.ToLowerInvariant())
                    .Any(y => y.Value.Any(x => x.ToLowerInvariant() == featureFlag.ToLowerInvariant()));
                if (isCachingEnabled && isFeatureCachingEnabled) // Todo: As of now feature flight result caching is enabled only for FXp platform tenant.
                {
                    var cachedFlightResult = await _cache.GetFeatureFlightResults(tenantConfiguration.Name, environment, new LoggerTrackingIds { CorrelationId = correlationId, TransactionId = transactionId });
                    if (cachedFlightResult != null)
                    {
                        var result = cachedFlightResult.Where(x => x.Key.Equals(featureFlag.ToLowerInvariant())).FirstOrDefault();
                        if (!result.Equals(default(KeyValuePair<string, bool>)))
                        {
                            return result.Value;
                        }
                    }
                    var isEnabled = await _featureManager.IsEnabledAsync(FlagUtilities.GetFeatureFlagId(tenantConfiguration.Name.ToLowerInvariant(), environment.ToLowerInvariant(), featureFlag));
                    await _cache.SetFeatureFlightResult(tenantConfiguration.Name, environment, new KeyValuePair<string, bool>(featureFlag.ToLowerInvariant(), isEnabled), cachedFlightResult,
                    new LoggerTrackingIds { CorrelationId = correlationId, TransactionId = transactionId });
                    return isEnabled;
                }
                else
                {
                    return await _featureManager.IsEnabledAsync(FlagUtilities.GetFeatureFlagId(tenantConfiguration.Name.ToLowerInvariant(), environment.ToLowerInvariant(), featureFlag));
                }
            }
            catch (Exception exception)
            {
                if (tenantConfiguration.Evaluation == null || !tenantConfiguration.Evaluation.IgnoreException)
                    throw;


                BaseAppException appException;
                if (exception is not EvaluationException)
                {
                    _logger.Log(exception, correlationId: correlationId, transactionId: transactionId, source: "FeatureFlighting:FeatureFlagEvaluator:IsEnabled");
                    appException = new EvaluationException($"Fault in evaluating {featureFlag}", correlationId, transactionId, "FeatureFlighting:FeatureFlagEvaluator:IsEnabled", exception);
                }
                else
                {
                    appException = exception as EvaluationException;
                }
                ExceptionContext context = appException.CreateLogContext();
                context.AddProperty("FeatureFlag", featureFlag);
                context.AddProperty("Tenant", tenantConfiguration.Name);
                context.AddProperty("TenantShortName", tenantConfiguration.ShortName);
                _logger.Log(context);

                string disabledContextKey = $"x-flag-{featureFlag.ToLowerInvariant()}-disabled-context";
                _httpContextAccessor.HttpContext.Response.Headers.AddOrUpdate(disabledContextKey.RemoveSpecialCharacters(), appException.Message.RemoveSpecialCharacters());

                return false;
            }
        }
    }
}
