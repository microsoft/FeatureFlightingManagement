using AppInsights.EnterpriseTelemetry;
using Microsoft.FeatureFlighting.Core.Domain.Events;

namespace Microsoft.FeatureFlighting.Core.Events.TelemetryHandlers
{
    internal class FeatureFlightDisabledTelemetryHandler : BaseFeatureFlightEventTelemetryHandler<FeatureFlightDisabled>
    {
        public FeatureFlightDisabledTelemetryHandler(ILogger logger): base(logger) { }
    }
}
