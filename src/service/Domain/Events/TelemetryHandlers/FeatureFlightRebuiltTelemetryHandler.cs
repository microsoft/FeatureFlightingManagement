using AppInsights.EnterpriseTelemetry;
using Microsoft.FeatureFlighting.Core.Domain.Events;

namespace Microsoft.FeatureFlighting.Core.Events.TelemetryHandlers
{
    internal class FeatureFlightRebuiltTelemetryHandler : BaseFeatureFlightEventTelemetryHandler<FeatureFlightRebuilt>
    {
        public FeatureFlightRebuiltTelemetryHandler(ILogger logger) : base(logger) { }
    }
}
