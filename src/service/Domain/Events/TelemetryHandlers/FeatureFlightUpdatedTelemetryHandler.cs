using AppInsights.EnterpriseTelemetry;
using Microsoft.FeatureFlighting.Core.Domain.Events;

namespace Microsoft.FeatureFlighting.Core.Events.TelemetryHandlers
{
    internal class FeatureFlightUpdatedTelemetryHandler : BaseFeatureFlightEventTelemetryHandler<FeatureFlightUpdated>
    {
        public FeatureFlightUpdatedTelemetryHandler(ILogger logger): base(logger) { }
    }
}
