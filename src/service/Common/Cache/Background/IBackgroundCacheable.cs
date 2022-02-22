using System;
using System.Threading.Tasks;

namespace Microsoft.FeatureFlighting.Common.Cache
{
    public interface IBackgroundCacheable<TCacheObject>: IBackgroundCacheable
    {   
        Task<TCacheObject> GetCachedObject(BackgroundCacheParameters parameters, LoggerTrackingIds trackingIds);
        Task SetCacheObject(BackgroundCacheableObject<TCacheObject> cacheableObject, LoggerTrackingIds trackingIds);
        Task<BackgroundCacheableObject<TCacheObject>> CreateCacheableObject(BackgroundCacheParameters cacheParameters, LoggerTrackingIds trackingIds);
    }
    
    public interface IBackgroundCacheable
    {
        string CacheableServiceId { get; }
        event EventHandler<BackgroundCacheParameters> ObjectCached;
        Task Recache(BackgroundCacheParameters cacheParameters, LoggerTrackingIds trackingIds);
    };
}
