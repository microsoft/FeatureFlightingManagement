using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Core.Spec;
using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.FeatureFlighting.Common.Caching;
using Microsoft.FeatureFlighting.Core.FeatureFilters;

namespace Microsoft.FeatureFlighting.Core.Operators
{   
    /// <inheritdoc/>
    public class OperatorStrategy : IOperatorStrategy
    {
        private readonly IEnumerable<BaseOperator> _operators;
        private readonly ICacheFactory _cacheFactory;
        private readonly ITenantConfigurationProvider _tenantConfigurationProvider;

        public OperatorStrategy(IEnumerable<BaseOperator> operators, ITenantConfigurationProvider tenantConfigurationProvider, ICacheFactory cacheFactory)
        {
            _operators = operators;
            _cacheFactory = cacheFactory;
            _tenantConfigurationProvider = tenantConfigurationProvider;
        }

        /// <inheritdoc/>
        public BaseOperator Get(Operator op)
        {
            return _operators.FirstOrDefault(evaluator => evaluator.Operator == op);
        }

        /// <inheritdoc/>
        public IEnumerable<string> GetAllOperators()
        {
            return Enum.GetNames(typeof(Operator)).Distinct();
        }

        /// <inheritdoc/>
        public async Task<IDictionary<string, List<string>>> GetFilterOperatorMapping(string tenant, string correlationId, string transactionId)
        {
            ICache cache = _cacheFactory.Create(tenant, nameof(TenantConfiguration.Cache.OperatorMapping), correlationId, transactionId);
            string cacheKey = nameof(TenantConfiguration.Cache.OperatorMapping);
            var _cachedMapping = await cache.Get<Dictionary<string, List<string>>>(cacheKey, correlationId, transactionId);
            if (_cachedMapping != null && _cachedMapping.Any())
                return _cachedMapping;


            TenantConfiguration tenantConfiguration = await _tenantConfigurationProvider.Get(tenant);
            IEnumerable<string> filterTypes = Enum.GetNames(typeof(Filters)).Distinct();
            Dictionary<string, List<string>> map = new();

            foreach (string filterType in filterTypes)
            {
                IEnumerable<Operator> supportedOperators =
                    _operators
                    .Where(op => op.SupportedFilters.Any(supportedFilter => supportedFilter.ToLowerInvariant() == Constants.Flighting.ALL.ToLowerInvariant() || supportedFilter.ToLowerInvariant() == filterType.ToLowerInvariant()))
                    .Select(op => op.Operator);

                if (supportedOperators != null && supportedOperators.Any())
                    map.Add(filterType, supportedOperators.Select(op => Enum.GetName(typeof(Operator), op)).ToList());
            }

            if (tenantConfiguration.IsBusinessRuleEngineEnabled())
                map.Add(ComplexFilters.RulesEngine.ToString(), RulesEngineFilter.SupportedOperators);

            await cache.Set(cacheKey, map, correlationId, transactionId);
            return map;
        }
    }
}
