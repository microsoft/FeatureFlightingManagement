using AppInsights.EnterpriseTelemetry;
using Microsoft.FeatureFlighting.Core.Spec;
using Microsoft.FeatureFlighting.Core.Domain.Events;

namespace Microsoft.FeatureFlighting.Core.Events.CacheHandlers
{
    internal class FeatureFlightEnabledCacheHandler : BaseFeatureFlightCacheEventHandler<FeatureFlightEnabled>
    {
        public FeatureFlightEnabledCacheHandler(IFeatureFlightCache cache, ILogger logger): base(cache, logger) { }
    }
}
