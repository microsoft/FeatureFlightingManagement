using AppInsights.EnterpriseTelemetry;
using Microsoft.FeatureFlighting.Core.Domain.Events;

namespace Microsoft.FeatureFlighting.Core.Events.TelemetryHandlers
{
    internal class FeatureFlightAlertsDisabledTelemetryHandler : BaseFeatureFlightEventTelemetryHandler<FeatureFlightAlertsDisabled>
    {
        public FeatureFlightAlertsDisabledTelemetryHandler(ILogger logger) : base(logger) { }
    }
}
