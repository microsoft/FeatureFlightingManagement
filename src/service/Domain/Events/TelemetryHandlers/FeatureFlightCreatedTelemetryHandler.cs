using AppInsights.EnterpriseTelemetry;
using Microsoft.FeatureFlighting.Core.Domain.Events;

namespace Microsoft.FeatureFlighting.Core.Events.TelemetryHandlers
{
    internal class FeatureFlightCreatedTelemetryHandler : BaseFeatureFlightEventTelemetryHandler<FeatureFlightCreated>
    {
        public FeatureFlightCreatedTelemetryHandler(ILogger logger): base(logger) { }
    }
}
