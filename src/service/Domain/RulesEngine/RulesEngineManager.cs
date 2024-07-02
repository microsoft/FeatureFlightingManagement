using System;
using RE = RulesEngine;
using RulesEngine.Models;
using System.Threading.Tasks;
using RulesEngine.Interfaces;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Core.Spec;
using Microsoft.FeatureFlighting.Common.Cache;
using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.FeatureFlighting.Common.Caching;
using Microsoft.FeatureFlighting.Common.Storage;
using Microsoft.FeatureFlighting.Common.AppExceptions;

namespace Microsoft.FeatureFlighting.Core.RulesEngine
{   
    /// <inheritdoc>/>
    public class RulesEngineManager: IRulesEngineManager, IBackgroundCacheable<IRulesEngineEvaluator>
    {   
        private readonly ITenantConfigurationProvider _tenantConfigurationProvider;
        private readonly IBlobProviderFactory _blobProviderFactory;
        private readonly ICacheFactory _cacheFactory;

        public string CacheableServiceId => nameof(RulesEngineManager);

        public event EventHandler<BackgroundCacheParameters> ObjectCached;

        public RulesEngineManager(IOperatorStrategy operatorEvaluatorStrategy, ITenantConfigurationProvider tenantConfigurationProvider, IBlobProviderFactory blobProviderFactory, ICacheFactory cacheFactory)
        {
            Operator.Initialize(operatorEvaluatorStrategy);
            _tenantConfigurationProvider = tenantConfigurationProvider;
            _blobProviderFactory = blobProviderFactory;
            _cacheFactory = cacheFactory;
        }

        /// <inheritdoc>/>
        public async Task<IRulesEngineEvaluator> Build(string tenant, string workflowName, LoggerTrackingIds trackingIds)
        {
            TenantConfiguration tenantConfiguration = await _tenantConfigurationProvider.Get(tenant);
            if (!tenantConfiguration.IsBusinessRuleEngineEnabled())
                return null;

            IRulesEngineEvaluator cachedRuleEngine = await GetCachedRuleEvaluator(tenant, workflowName, trackingIds);
            if (cachedRuleEngine != null)
                return cachedRuleEngine;

            IRulesEngineEvaluator evaluator = await CreateRulesEngine(workflowName, tenant, trackingIds);
            await CacheRuleEvaluator((RulesEngineEvaluator)evaluator, tenant, workflowName, tenantConfiguration.BusinessRuleEngine.CacheDuration, trackingIds);
            return evaluator;
        }

        /// <inheritdoc>/>
        public async Task<IRulesEngineEvaluator> Build(string tenant, string workflowName, string workflowPayload)
        {
            TenantConfiguration tenantConfiguration = await _tenantConfigurationProvider.Get(tenant);
            IRulesEngine ruleEngine = new RE.RulesEngine(
                jsonConfig: new string[] { workflowPayload },
                reSettings: new ReSettings() { CustomTypes = new Type[] { typeof(Operator) } },
                logger: null);
            IRulesEngineEvaluator evaluator = new RulesEngineEvaluator(ruleEngine, workflowName, tenantConfiguration);
            return evaluator;
        }

        private Task<IRulesEngineEvaluator> GetCachedRuleEvaluator(string tenant, string workflowName, LoggerTrackingIds trackingIds)
        {
            BackgroundCacheParameters cacheParameters = new()
            {
                CacheKey = $"{tenant}_{workflowName}",
                ObjectId = workflowName,
                Tenant = tenant
            };
            return GetCachedObject(cacheParameters, trackingIds);
        }

        private async Task<IRulesEngineEvaluator> CreateRulesEngine(string workflowName, string tenant, LoggerTrackingIds trackingIds)
        {
            BackgroundCacheParameters cacheParameters = new()
            {
                CacheKey = $"{tenant}_{workflowName}",
                ObjectId = workflowName,
                Tenant = tenant
            };
            return (await CreateCacheableObject(cacheParameters, trackingIds)).Object;
        }

        private Task CacheRuleEvaluator(RulesEngineEvaluator ruleEvaluator, string tenant, string workflowName, int cacheDuration, LoggerTrackingIds trackingIds)
        {
            BackgroundCacheParameters cacheParameters = new()
            {
                CacheKey = $"{tenant}_{workflowName}",
                ObjectId = workflowName,
                Tenant = tenant,
                CacheDuration = cacheDuration
            };
            BackgroundCacheableObject<IRulesEngineEvaluator> cacheableObject = new()
            {
                Object = ruleEvaluator,
                CacheParameters = cacheParameters
            };
            return SetCacheObject(cacheableObject, trackingIds);
        }

        public async Task<IRulesEngineEvaluator> GetCachedObject(BackgroundCacheParameters cacheParameters, LoggerTrackingIds trackingIds)
        {   
            ICache breCache = _cacheFactory.Create(cacheParameters.Tenant, nameof(TenantConfiguration.Cache.RulesEngine), trackingIds.CorrelationId, trackingIds.TransactionId);
            if (breCache == null)
                return null;

            IRulesEngineEvaluator cachedRuleEngine = await breCache.Get<RulesEngineEvaluator>(cacheParameters.CacheKey, trackingIds.CorrelationId, trackingIds.TransactionId);
            return cachedRuleEngine;
        }

        public async Task SetCacheObject(BackgroundCacheableObject<IRulesEngineEvaluator> cacheableObject, LoggerTrackingIds trackingIds)
        {   
            TenantConfiguration tenantConfiguration = await _tenantConfigurationProvider.Get(cacheableObject.CacheParameters.Tenant);
            ICache breCache = _cacheFactory.Create(cacheableObject.CacheParameters.Tenant, nameof(TenantConfiguration.Cache.RulesEngine), trackingIds.CorrelationId, trackingIds.TransactionId);
            if (breCache == null)
                return;

            await breCache.Set(cacheableObject.CacheParameters.CacheKey, cacheableObject.Object, trackingIds.CorrelationId, trackingIds.TransactionId, relativeExpirationMins: tenantConfiguration.BusinessRuleEngine.CacheDuration);
            ObjectCached?.Invoke(this, cacheableObject.CacheParameters);
        }

        public async Task<BackgroundCacheableObject<IRulesEngineEvaluator>> CreateCacheableObject(BackgroundCacheParameters cacheParameters, LoggerTrackingIds trackingIds)
        {
            string workflowName = cacheParameters.ObjectId;
            TenantConfiguration tenantConfiguration = await _tenantConfigurationProvider.Get(cacheParameters.Tenant);
            cacheParameters.CacheDuration = tenantConfiguration.BusinessRuleEngine.CacheDuration;

            IBlobProvider blobProvider = await _blobProviderFactory.CreateBreWorkflowProvider(cacheParameters.Tenant);
            string workflowJson = await blobProvider.Get($"{workflowName}.json", trackingIds);
            if (string.IsNullOrWhiteSpace(workflowJson))
                throw new RuleEngineException(workflowName, cacheParameters.Tenant, "Rule engine not found in the configured storage location", "FeatureFlighting.RuleEngineManager.Build", trackingIds.CorrelationId, trackingIds.TransactionId);

            IRulesEngine ruleEngine = new RE.RulesEngine(
                jsonConfig: new string[] { workflowJson },
                reSettings: new ReSettings() { CustomTypes = new Type[] { typeof(Operator) } },
                logger: null);

            
            IRulesEngineEvaluator evaluator = new RulesEngineEvaluator(ruleEngine, workflowName, tenantConfiguration);
            BackgroundCacheableObject<IRulesEngineEvaluator> cacheableRulesEngineEvaluator = new()
            {
                Object = evaluator,
                CacheParameters = cacheParameters
            };
            return cacheableRulesEngineEvaluator;
        }

        public async Task RebuildCache(BackgroundCacheParameters cacheParameters, LoggerTrackingIds trackingIds)
        {
            BackgroundCacheableObject<IRulesEngineEvaluator> cacheableObject = await CreateCacheableObject(cacheParameters, trackingIds);
            await SetCacheObject(cacheableObject, trackingIds);
        }
    }
}
