using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Common.Cache;
using AppInsights.EnterpriseTelemetry;
using AppInsights.EnterpriseTelemetry.Context;

namespace Microsoft.FeatureFlighting.Infrastructure.Cache
{
    // <inheritdoc />
    internal class BackgroundCacheManager : IBackgroundCacheManager
    {
        private readonly IList<IBackgroundCacheable> _cacheables;
        private static readonly Dictionary<string, List<BackgroundCacheParameters>> _backgroundCacheableParams = new();
        private int _period;
        private const int DEFAULT_CACHE_REBUILD_PERIOD = 5;
        private readonly ILogger _logger;

        public BackgroundCacheManager(IEnumerable<IBackgroundCacheable> backgroundCacheables, ILogger logger)
        {
            _cacheables = backgroundCacheables.ToList();
            _logger = logger;
        }

        // <inheritdoc />
        public void Init(int period)
        {
            _period = period > 0 ? period : DEFAULT_CACHE_REBUILD_PERIOD;
            foreach (IBackgroundCacheable cacheable in _cacheables)
            {
                if (!_backgroundCacheableParams.ContainsKey(cacheable.CacheableServiceId))
                    _backgroundCacheableParams.Add(cacheable.CacheableServiceId, new());
                cacheable.ObjectCached += AddCacheParameter;
            }
        }

        private void AddCacheParameter(object sender, BackgroundCacheParameters cacheParameters)
        {
            string cacheableServiceId = (sender as IBackgroundCacheable)?.CacheableServiceId ?? string.Empty;
            if (string.IsNullOrWhiteSpace(cacheableServiceId) || !_backgroundCacheableParams.ContainsKey(cacheableServiceId))
                return;

            List<BackgroundCacheParameters> cachedParameters = _backgroundCacheableParams[cacheableServiceId];
            if (!cachedParameters.Any(param => param.CacheKey == cacheParameters.CacheKey))
            {
                cacheParameters.UpdateRebuildTimestamp();
                _backgroundCacheableParams[cacheableServiceId].Add(cacheParameters);
            }
        }

        // <inheritdoc />
        public async Task RebuildCache(LoggerTrackingIds trackingIds, CancellationToken cancellationToken = default)
        {
            if (_backgroundCacheableParams == null || !_backgroundCacheableParams.Any())
                return;

            foreach (IBackgroundCacheable cacheable in _cacheables)
            {
                if (_backgroundCacheableParams.ContainsKey(cacheable.CacheableServiceId))
                {
                    foreach (BackgroundCacheParameters cacheParameters in
                        _backgroundCacheableParams[cacheable.CacheableServiceId].Where(param => param.ShouldRebuildCache(_period)))
                    {
                        await cacheable.RebuildCache(cacheParameters, trackingIds).ConfigureAwait(false);
                        cacheParameters.UpdateRebuildTimestamp();

                        EventContext @event = new("BackgroundCache:Rebuilt", trackingIds.CorrelationId, trackingIds.TransactionId);
                        @event.AddProperty("CacheKey", cacheParameters.CacheKey);
                        @event.AddProperty("ServiceId", cacheable.CacheableServiceId);
                        @event.AddProperty("Tenant", cacheParameters.Tenant);
                        @event.AddProperty("NextRebuildTime", cacheParameters.NextRebuildTimestamp.ToString());
                        _logger.Log(@event);
                    }
                }
            }
        }

        public IEnumerable<string> GetCacheableServiceIds()
        {
            if (_cacheables == null || !_cacheables.Any())
                return new List<string>();

            return _cacheables.Select(cacheable => cacheable.CacheableServiceId).ToList();
        }

        // <inheritdoc />
        public void Dispose()
        {
            _cacheables.Clear();
            _backgroundCacheableParams.Clear();
        }
    }

}
