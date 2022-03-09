using AppInsights.EnterpriseTelemetry;
using Microsoft.FeatureFlighting.Core.Spec;
using Microsoft.FeatureFlighting.Core.Domain.Events;

namespace Microsoft.FeatureFlighting.Core.Events.CacheHandlers
{
    internal class FeatureFlightCreatedCacheHandler: BaseFeatureFlightCacheEventHandler<FeatureFlightCreated>
    {
        public FeatureFlightCreatedCacheHandler(IFeatureFlightCache cache, ILogger logger): base(cache, logger) { }
    }
}
