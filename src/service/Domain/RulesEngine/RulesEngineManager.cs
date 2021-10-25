using System;
using RE = RulesEngine;
using RulesEngine.Models;
using System.Threading.Tasks;
using RulesEngine.Interfaces;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Core.Spec;
using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.FeatureFlighting.Common.Caching;
using Microsoft.FeatureFlighting.Common.Storage;
using Microsoft.FeatureFlighting.Common.AppExceptions;

namespace Microsoft.FeatureFlighting.Core.RulesEngine
{   
    /// <inheritdoc>/>
    public class RulesEngineManager: IRulesEngineManager
    {   
        private readonly ITenantConfigurationProvider _tenantConfigurationProvider;
        private readonly IBlobProviderFactory _blobProviderFactory;
        private readonly ICacheFactory _cacheFactory;

        public RulesEngineManager(IOperatorStrategy operatorEvaluatorStrategy, ITenantConfigurationProvider tenantConfigurationProvider, IBlobProviderFactory blobProviderFactory, ICacheFactory cacheFactory)
        {
            if (operatorEvaluatorStrategy == null)
                throw new ArgumentNullException(nameof(operatorEvaluatorStrategy));

            Operator.Initialize(operatorEvaluatorStrategy);
            _tenantConfigurationProvider = tenantConfigurationProvider ?? throw new ArgumentNullException(nameof(tenantConfigurationProvider));
            _blobProviderFactory = blobProviderFactory ?? throw new ArgumentNullException(nameof(blobProviderFactory));
            _cacheFactory = cacheFactory ?? throw new ArgumentNullException(nameof(cacheFactory));
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

            IBlobProvider blobProvider = await _blobProviderFactory.CreateBreWorkflowProvider(tenant);
            string workflowJson = await blobProvider.Get($"{workflowName}.json", trackingIds);
            if (string.IsNullOrWhiteSpace(workflowJson))
                throw new RuleEngineException(workflowName, tenant, "Rule engine not found in the configured storage location", "FeatureFlighting.RuleEngineManager.Build", trackingIds.CorrelationId, trackingIds.TransactionId);

            IRulesEngine ruleEngine = new RE.RulesEngine(
                jsonConfig: new string[] { workflowJson },
                reSettings: new ReSettings() { CustomTypes = new Type[] { typeof(Operator) } },
                logger: null);

            IRulesEngineEvaluator evaluator = new RulesEngineEvaluator(ruleEngine, workflowName, tenant);
            await CacheRuleEvaluator((RulesEngineEvaluator)evaluator, tenant, workflowName, tenantConfiguration.BusinessRuleEngine.CacheDuration, trackingIds);
            return evaluator;
        }

        private async Task<IRulesEngineEvaluator> GetCachedRuleEvaluator(string tenant, string workflowName, LoggerTrackingIds trackingIds)
        {
            ICache breCache = _cacheFactory.Create(tenant, nameof(TenantConfiguration.Cache.RulesEngine), trackingIds.CorrelationId, trackingIds.TransactionId);
            if (breCache == null)
                return null;

            IRulesEngineEvaluator cachedRuleEngine = await breCache.Get<RulesEngineEvaluator>(workflowName, trackingIds.CorrelationId, trackingIds.TransactionId);
            return cachedRuleEngine;
        }

        private async Task CacheRuleEvaluator(RulesEngineEvaluator ruleEvaluator, string tenant, string workflowName, int cacheDuration, LoggerTrackingIds trackingIds)
        {
            ICache breCache = _cacheFactory.Create(tenant, nameof(TenantConfiguration.Cache.RulesEngine), trackingIds.CorrelationId, trackingIds.TransactionId);
            if (breCache == null)
                return;
            await breCache.Set(workflowName, ruleEvaluator, trackingIds.CorrelationId, trackingIds.TransactionId, relativeExpirationMins: cacheDuration);
        }
    }
}
