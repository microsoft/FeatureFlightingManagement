using AppInsights.EnterpriseTelemetry;
using Microsoft.FeatureFlighting.Core.Domain.Events;

namespace Microsoft.FeatureFlighting.Core.Events.TelemetryHandlers
{
    internal class FeatureFlightStageActivatedTelemetryHandler : BaseFeatureFlightEventTelemetryHandler<FeatureFlightStageActivated>
    {
        public FeatureFlightStageActivatedTelemetryHandler(ILogger logger): base(logger) { }
    }
}
