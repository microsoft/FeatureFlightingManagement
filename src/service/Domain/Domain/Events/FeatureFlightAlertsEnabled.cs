using System.Collections.Generic;
using Microsoft.FeatureFlighting.Common;

namespace Microsoft.FeatureFlighting.Core.Domain.Events
{
    /// <summary>
    /// Event when alerts for a feature fight is disabled
    /// </summary>
    internal class FeatureFlightAlertsEnabled : BaseFeatureFlightEvent
    {
        public override string DisplayName => nameof(FeatureFlightAlertsEnabled);

        public string AlertsEnabledBy { get; set; }

        public FeatureFlightAlertsEnabled(FeatureFlightAggregateRoot flight, LoggerTrackingIds trackingIds, string source)
            : base(flight, trackingIds, source)
        {
            AlertsEnabledBy = flight.Audit?.LastModifiedBy;
        }

        public override Dictionary<string, string> GetProperties()
        {
            Dictionary<string, string> properties = base.GetProperties();
            properties.AddOrUpdate(nameof(AlertsEnabledBy), AlertsEnabledBy);
            return properties;
        }
    }
}
