using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Core.Spec;
using Microsoft.FeatureFlighting.Common.Model;
using Microsoft.FeatureFlighting.Common.Caching;

namespace Microsoft.FeatureFlighting.Core.Cache
{   
    /// <inheritdoc/>
    internal class FeatureFlightCache: IFeatureFlightCache
    {
        private readonly ICacheFactory _cacheFactory;

        private const string OpType_FeatureFlags = "FeatureFlags";
        private const string OpType_FeatureFlagNames = "FeatureFlagNames";

        public FeatureFlightCache(ICacheFactory cacheFactory)
        {
            _cacheFactory = cacheFactory;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FeatureFlightDto>> GetFeatureFlights(string tenant, string environment, LoggerTrackingIds trackingIds)
        {
            ICache cache = _cacheFactory.Create(tenant, OpType_FeatureFlags, trackingIds.CorrelationId, trackingIds.TransactionId);
            if (cache == null)
                return null;

            string cacheKey = CreateFeatureFlagsCacheKey(tenant, environment);
            IList<string> serializedCachedFlights = await cache.GetList(cacheKey, trackingIds.CorrelationId, trackingIds.TransactionId);
            if (serializedCachedFlights == null || !serializedCachedFlights.Any())
                return null;

            return serializedCachedFlights.Select(serializedFlight =>
                JsonConvert.DeserializeObject<FeatureFlightDto>(serializedFlight))
                .ToList();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> GetFeatureNames(string tenant, string environment, LoggerTrackingIds trackingIds)
        {
            ICache cache = _cacheFactory.Create(tenant, OpType_FeatureFlagNames, trackingIds.CorrelationId, trackingIds.TransactionId);
            if (cache == null)
                return null;

            string cacheKey = CreateFeatureFlagsCacheKey(tenant, environment);
            IList<string> cachedFlights = await cache.GetList(cacheKey, trackingIds.CorrelationId, trackingIds.TransactionId);
            return cachedFlights;
        }

        /// <inheritdoc/>
        public async Task SetFeatureFlights(string tenant, string environment, IEnumerable<FeatureFlightDto> featureFlights, LoggerTrackingIds trackingIds)
        {
            if (featureFlights == null || !featureFlights.Any())
                return;

            ICache featureFlightCache = _cacheFactory.Create(tenant, OpType_FeatureFlags, trackingIds.CorrelationId, trackingIds.TransactionId);
            ICache featureNameCache = _cacheFactory.Create(tenant, OpType_FeatureFlagNames, trackingIds.CorrelationId, trackingIds.TransactionId);
            string cacheKey = CreateFeatureFlagsCacheKey(tenant, environment);
            
            if (featureFlightCache != null)
            {
                IList<string> serializedCachedFlights = featureFlights.Select(flight => JsonConvert.SerializeObject(flight)).ToList();
                await featureFlightCache.SetList(cacheKey, serializedCachedFlights, trackingIds.CorrelationId, trackingIds.TransactionId);
            }

            if (featureNameCache != null)
            {
                IList<string> featureNames = featureFlights.Select(flight => flight.Name).ToList();
                await featureNameCache.SetList(cacheKey, featureNames, trackingIds.CorrelationId, trackingIds.TransactionId);
            }
        }

        /// <inheritdoc/>
        public async Task DeleteFeatureFlights(string tenant, string environment, LoggerTrackingIds trackingIds)
        {
            ICache featureFlightCache = _cacheFactory.Create(tenant, OpType_FeatureFlags, trackingIds.CorrelationId, trackingIds.TransactionId);
            ICache featureNameCache = _cacheFactory.Create(tenant, OpType_FeatureFlagNames, trackingIds.CorrelationId, trackingIds.TransactionId);
            string cacheKey = CreateFeatureFlagsCacheKey(tenant, environment);

            if (featureFlightCache != null)
                await featureFlightCache.Delete(cacheKey, trackingIds.CorrelationId, trackingIds.TransactionId);

            if (featureNameCache != null)
                await featureNameCache.Delete(cacheKey, trackingIds.CorrelationId, trackingIds.TransactionId);
        }

        private string CreateFeatureFlagsCacheKey(string tenant, string environment) => new StringBuilder()
            .Append("Flags:")
            .Append(tenant.ToUpperInvariant())
            .Append(":")
            .Append(environment.ToUpperInvariant())
            .ToString();
    }
}
