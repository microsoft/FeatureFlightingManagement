using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.FeatureFlighting.Common.Cache
{
    /// <summary>
    /// Manages cache in the background
    /// </summary>
    public interface IBackgroundCacheManager
    {
        /// <summary>
        /// Initializes the Cach e Manager
        /// </summary>
        /// <param name="period">Refresh period in mis</param>
        void Init(int period);
        
        /// <summary>
        /// Rebuils the cache
        /// </summary>
        /// <param name="trackingIds" cref="LoggerTrackingIds">Tracking IDs</param>
        Task RebuildCache(LoggerTrackingIds trackingIds, CancellationToken cancellationToken);

        /// <summary>
        /// Gets a collection of Cacheable Service IDs
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetCacheableServiceIds();

        /// <summary>
        /// Disposes the cache manager
        /// </summary>
        void Dispose();
    }
}
