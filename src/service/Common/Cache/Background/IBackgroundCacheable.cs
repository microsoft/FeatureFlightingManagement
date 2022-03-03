using System;
using System.Threading.Tasks;

namespace Microsoft.FeatureFlighting.Common.Cache
{
    /// <summary>
    /// Service which can cache in the background
    /// </summary>
    public interface IBackgroundCacheable
    {
        /// <summary>
        /// ID of the service (needed to uniquely identify the service)
        /// </summary>
        string CacheableServiceId { get; }
        
        /// <summary>
        /// Event when an object is cached
        /// </summary>
        event EventHandler<BackgroundCacheParameters> ObjectCached;
        
        /// <summary>
        /// Rebuilds the cache
        /// </summary>
        /// <param name="cacheParameters" cref="BackgroundCacheParameters">Parameters needed for caching</param>
        /// <param name="trackingIds">Tracking IDs</param>
        Task RebuildCache(BackgroundCacheParameters cacheParameters, LoggerTrackingIds trackingIds);
    };

    public interface IBackgroundCacheable<TCacheObject>: IBackgroundCacheable
    {   
        Task<TCacheObject> GetCachedObject(BackgroundCacheParameters parameters, LoggerTrackingIds trackingIds);
        Task SetCacheObject(BackgroundCacheableObject<TCacheObject> cacheableObject, LoggerTrackingIds trackingIds);
        Task<BackgroundCacheableObject<TCacheObject>> CreateCacheableObject(BackgroundCacheParameters cacheParameters, LoggerTrackingIds trackingIds);
    }
}
