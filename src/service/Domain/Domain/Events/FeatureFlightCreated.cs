using System.Collections.Generic;
using Microsoft.FeatureFlighting.Common;

namespace Microsoft.FeatureFlighting.Core.Domain.Events
{
    internal class FeatureFlightCreated : BaseFeatureFlightEvent
    {
        public override string DisplayName => nameof(FeatureFlightCreated);
    
        public string CreatedBy { get; set; }

        public FeatureFlightCreated(FeatureFlightAggregateRoot flight, LoggerTrackingIds trackingIds, string source = null)
            :base(flight, trackingIds, source)
        {
            CreatedBy = flight.Audit.CreatedBy;
        }

        public override Dictionary<string, string> GetProperties()
        {
            Dictionary<string, string> properties = base.GetProperties();
            properties.AddOrUpdate(nameof(CreatedBy), CreatedBy);
            return properties;
        }
    }
}
