using AppInsights.EnterpriseTelemetry;
using Microsoft.FeatureFlighting.Core.Domain.Events;

namespace Microsoft.FeatureFlighting.Core.Events.TelemetryHandlers
{
    internal class FeatureFlightAlertsEnabledTelemetryHandler : BaseFeatureFlightEventTelemetryHandler<FeatureFlightAlertsEnabled>
    {
        public FeatureFlightAlertsEnabledTelemetryHandler(ILogger logger) : base(logger) { }
    }
}
