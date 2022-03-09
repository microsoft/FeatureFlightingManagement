using AppInsights.EnterpriseTelemetry;
using Microsoft.FeatureFlighting.Core.Domain.Events;

namespace Microsoft.FeatureFlighting.Core.Events.TelemetryHandlers
{
    internal class FeatureMetricsUpdatedTelemetryHandler : BaseFeatureFlightEventTelemetryHandler<FeatureFlightMetricsUpdated>
    {
        public FeatureMetricsUpdatedTelemetryHandler(ILogger logger): base(logger) { }
    }
}
