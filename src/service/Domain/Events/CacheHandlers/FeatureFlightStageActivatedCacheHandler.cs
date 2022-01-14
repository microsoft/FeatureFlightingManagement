using AppInsights.EnterpriseTelemetry;
using Microsoft.FeatureFlighting.Core.Spec;
using Microsoft.FeatureFlighting.Core.Domain.Events;

namespace Microsoft.FeatureFlighting.Core.Events.CacheHandlers
{
    internal class FeatureFlightStageActivatedCacheHandler : BaseFeatureFlightCacheEventHandler<FeatureFlightStageActivated>
    {
        public FeatureFlightStageActivatedCacheHandler(IFeatureFlightCache cache, ILogger logger): base(cache, logger) { }
    }
}
