using System.Collections.Generic;
using Microsoft.FeatureFlighting.Common;

namespace Microsoft.FeatureFlighting.Core.Domain.Events
{
    /// <summary>
    /// Evnet when a feature flight is rebuilt (re-optimized)
    /// </summary>
    internal class FeatureFlightRebuilt: BaseFeatureFlightEvent
    {
        public override string DisplayName => nameof(FeatureFlightRebuilt);

        public string RebuildBy { get; set; }
        public string RebuildReason { get; set; }

        public FeatureFlightRebuilt(FeatureFlightAggregateRoot flight, string reason, LoggerTrackingIds trackingIds)
            : base(flight, trackingIds)
        {
            RebuildBy = flight.Audit.LastModifiedBy;
            RebuildReason = reason;
        }

        public override Dictionary<string, string> GetProperties()
        {
            Dictionary<string, string> properties = base.GetProperties();
            properties.AddOrUpdate(nameof(RebuildBy), RebuildBy);
            properties.AddOrUpdate(nameof(RebuildReason), RebuildReason);
            return properties;
        }
    }
}
