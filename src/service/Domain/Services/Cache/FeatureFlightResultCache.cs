using Microsoft.FeatureFlighting.Common.Caching;
using Microsoft.FeatureFlighting.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.FeatureFlighting.Core.Spec;
using Microsoft.FeatureFlighting.Common.Cache;

namespace Microsoft.FeatureFlighting.Core.Services.Cache
{
    public class FeatureFlightResultCache : IFeatureFlightResultCache
    {
        private readonly IFeatureFlightResultCacheFactory _cacheFactory;
        private const string OpType_FeatureFlagResults = "FeatureFlagResults";

        public FeatureFlightResultCache(IFeatureFlightResultCacheFactory cacheFactory)
        {
            _cacheFactory = cacheFactory;
        }
        public async Task<IList<KeyValuePair<string, bool>>> GetFeatureFlightResults(string tenant, string environment, LoggerTrackingIds trackingIds)
        {
            ICache cache = _cacheFactory.Create(tenant, OpType_FeatureFlagResults, trackingIds.CorrelationId, trackingIds.TransactionId);
            if (cache == null)
                return null;

            string cacheKey = CreateFeatureFlagsCacheKey(tenant, environment);
            IList<KeyValuePair<string, bool>> serializedCachedFlightResults = await cache.GetListObject<KeyValuePair<string, bool>>(cacheKey, trackingIds.CorrelationId, trackingIds.TransactionId);
            if (serializedCachedFlightResults == null || !serializedCachedFlightResults.Any())
                return null;

            return serializedCachedFlightResults;
        }

        public async Task SetFeatureFlightResult(string tenant, string environment, KeyValuePair<string, bool> featureFlightResult,IList<KeyValuePair<String, bool>> cachedFlightResult, LoggerTrackingIds trackingIds)
        {
            if (featureFlightResult.Equals(default(KeyValuePair<string, bool>)))
                return;

            ICache featureFlightCache = _cacheFactory.Create(tenant, OpType_FeatureFlagResults, trackingIds.CorrelationId, trackingIds.TransactionId);
            string cacheKey = CreateFeatureFlagsCacheKey(tenant, environment);

            if(cachedFlightResult == null)
            {
                cachedFlightResult = new List<KeyValuePair<string, bool>>();
            }
            cachedFlightResult.Add(featureFlightResult);
            if (featureFlightCache != null)
            {
                await featureFlightCache.SetListObjects(cacheKey, cachedFlightResult, trackingIds.CorrelationId, trackingIds.TransactionId);
            }
        }

        private string CreateFeatureFlagsCacheKey(string tenant, string environment) => new StringBuilder()
           .Append("Flags:")
           .Append(tenant.ToUpperInvariant())
           .Append(":")
           .Append(environment.ToUpperInvariant())
           .ToString();
    }
}
