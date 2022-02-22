using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Common.Cache;

namespace Microsoft.FeatureFlighting.Infrastructure.Cache
{
    internal class BackgroundCacheManager : IBackgroundCacheManager
    {
        private readonly IList<IBackgroundCacheable> _cacheables;
        private static readonly Dictionary<string, List<BackgroundCacheParameters>> _backgroundCacheablesMap = new();
        private int _period = 5;

        public BackgroundCacheManager(IEnumerable<IBackgroundCacheable> backgroundCacheables)
        {
            _cacheables = backgroundCacheables.ToList();
        }

        public void Init(int period = 5)
        {
            _period = period > 0 ? period : _period;
            foreach (IBackgroundCacheable cacheable in _cacheables)
            {
                if (!(_backgroundCacheablesMap.ContainsKey(cacheable.CacheableServiceId)))
                    _backgroundCacheablesMap.Add(cacheable.CacheableServiceId, new());
                cacheable.ObjectCached += AddCacheParameter;
            }
        }

        private void AddCacheParameter(object sender, BackgroundCacheParameters cacheParameters)
        {
            string cacheableServiceId = (sender as IBackgroundCacheable)?.CacheableServiceId ?? string.Empty;
            if (!_backgroundCacheablesMap.ContainsKey(cacheableServiceId))
                return;

            cacheParameters.UpdateNextRecacheTimestamp();
            List<BackgroundCacheParameters> cachedParameters = _backgroundCacheablesMap[cacheableServiceId];
            if (!cachedParameters.Any(param => param.CacheKey == cacheParameters.CacheKey))
            {
                _backgroundCacheablesMap[((IBackgroundCacheable)sender).CacheableServiceId].Add(cacheParameters);
            }
        }

        public async Task Recache(LoggerTrackingIds trackingIds, CancellationToken cancellationToken = default)
        {
            if (_backgroundCacheablesMap == null || !_backgroundCacheablesMap.Any())
                return;

            foreach (IBackgroundCacheable cacheable in _cacheables)
            {
                if (_backgroundCacheablesMap.ContainsKey(cacheable.CacheableServiceId))
                {
                    foreach (BackgroundCacheParameters cacheParameters in
                        _backgroundCacheablesMap[cacheable.CacheableServiceId].Where(param => param.ShouldRecache(_period)))
                    {

                        await cacheable.Recache(cacheParameters, trackingIds).ConfigureAwait(false);
                        cacheParameters.UpdateNextRecacheTimestamp();
                    }
                }
            }
        }


        public void Cleanup()
        {
            _cacheables.Clear();
            _backgroundCacheablesMap.Clear();
        }
    }
}
