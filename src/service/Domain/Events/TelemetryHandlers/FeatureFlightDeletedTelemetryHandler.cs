using AppInsights.EnterpriseTelemetry;
using Microsoft.FeatureFlighting.Core.Domain.Events;

namespace Microsoft.FeatureFlighting.Core.Events.TelemetryHandlers
{
    internal class FeatureFlightDeletedTelemetryHandler : BaseFeatureFlightEventTelemetryHandler<FeatureFlightDeleted>
    {
        public FeatureFlightDeletedTelemetryHandler(ILogger logger): base(logger) { }
    }
}
