using AppInsights.EnterpriseTelemetry;
using Microsoft.FeatureFlighting.Core.Domain.Events;

namespace Microsoft.FeatureFlighting.Core.Events.TelemetryHandlers
{
    internal class FeatureFlightEnabledTelemetryHandler : BaseFeatureFlightEventTelemetryHandler<FeatureFlightEnabled>
    {
        public FeatureFlightEnabledTelemetryHandler(ILogger logger): base(logger) { }
    }
}
