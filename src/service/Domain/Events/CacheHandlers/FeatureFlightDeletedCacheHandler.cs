using AppInsights.EnterpriseTelemetry;
using Microsoft.FeatureFlighting.Core.Spec;
using Microsoft.FeatureFlighting.Core.Domain.Events;

namespace Microsoft.FeatureFlighting.Core.Events.CacheHandlers
{
    internal class FeatureFlightDeletedCacheHandler : BaseFeatureFlightCacheEventHandler<FeatureFlightDeleted>
    {
        public FeatureFlightDeletedCacheHandler(IFeatureFlightCache cache, ILogger logger): base(cache, logger) { }
    }
}
