using AppInsights.EnterpriseTelemetry;
using Microsoft.UnifiedRedisPlatform.Core;

namespace Microsoft.FeatureFlighting.Caching
{
    public class UnifiedRedisCache: RedisCache
    {
        public UnifiedRedisCache(string cluster, string app, string appSecret, string location, ILogger logger)
            :base(UnifiedConnectionMultiplexer.Connect(cluster, app, appSecret, preferredLocation: location), logger, $"{cluster}-{app}")
        { }
    }
}
