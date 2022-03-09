using System.Collections.Generic;
using Microsoft.FeatureFlighting.Common;

namespace Microsoft.FeatureFlighting.Core.Domain.Events
{
    /// <summary>
    /// Event when alerts for a feature fight is disabled
    /// </summary>
    internal class FeatureFlightAlertsDisabled : BaseFeatureFlightEvent
    {
        public override string DisplayName => nameof(FeatureFlightAlertsDisabled);

        public string AlertsDisabledBy { get; set; }

        public FeatureFlightAlertsDisabled(FeatureFlightAggregateRoot flight, LoggerTrackingIds trackingIds, string source)
            : base(flight, trackingIds, source)
        {
            AlertsDisabledBy = flight.Audit?.LastModifiedBy;
        }

        public override Dictionary<string, string> GetProperties()
        {
            Dictionary<string, string> properties = base.GetProperties();
            properties.AddOrUpdate(nameof(AlertsDisabledBy), AlertsDisabledBy);
            return properties;
        }
    }
}
