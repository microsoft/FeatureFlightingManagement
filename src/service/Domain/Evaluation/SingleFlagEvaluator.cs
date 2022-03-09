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

namespace Microsoft.FeatureFlighting.Core.Evaluation
{
    
    /// <inheritdoc/>
    internal class SingleFlagEvaluator : ISingleFlagEvaluator
    {
        private readonly IFeatureManager _featureManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger _logger;

        public SingleFlagEvaluator(IFeatureManager featureManager, IHttpContextAccessor httpContextAccessor, ILogger logger)
        {
            _featureManager = featureManager;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        /// <inheritdoc/>
        public virtual async Task<bool> IsEnabled(string featureFlag, TenantConfiguration tenantConfiguration, string environment)
        {
            try
            {
                return await _featureManager.IsEnabledAsync(FlagUtilities.GetFeatureFlagId(tenantConfiguration.Name.ToLowerInvariant(), environment.ToLowerInvariant(), featureFlag));
            }
            catch (Exception exception)
            {
                if (tenantConfiguration.Evaluation == null || !tenantConfiguration.Evaluation.IgnoreException)
                    throw;

                string correlationId = _httpContextAccessor.HttpContext.Request.Headers.GetOrDefault("x-correlationId", Guid.NewGuid().ToString()).ToString();
                string transactionId = _httpContextAccessor.HttpContext.Request.Headers.GetOrDefault("x-messageId", Guid.NewGuid().ToString()).ToString();

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
