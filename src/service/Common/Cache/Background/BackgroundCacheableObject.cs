namespace Microsoft.FeatureFlighting.Common.Cache
{
    public class BackgroundCacheableObject<TCacheObject>
    {   
        public TCacheObject Object { get; set; }
        public BackgroundCacheParameters CacheParameters { get; set; }
    }
}
