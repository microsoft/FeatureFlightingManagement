using System.Collections.Generic;
using Microsoft.FeatureFlighting.Common;

namespace Microsoft.FeatureFlighting.Core.Domain.Events
{
    /// <summary>
    /// Events when a stage in the feature flight is activated
    /// </summary>
    internal class FeatureFlightStageActivated : BaseFeatureFlightEvent
    {
        public override string DisplayName => nameof(FeatureFlightStageActivated);
        public string ActivatedStageName { get; set; }
        public string ActivatedBy { get; set; }

        public FeatureFlightStageActivated(FeatureFlightAggregateRoot flight, string activatedStageName, LoggerTrackingIds trackingIds): base(flight, trackingIds)
        {
            ActivatedStageName = activatedStageName;
            ActivatedBy = flight.Audit.LastModifiedBy;
        }

        public override Dictionary<string, string> GetProperties()
        {
            Dictionary<string, string> properties = base.GetProperties();
            properties.Add(nameof(ActivatedStageName), ActivatedStageName);
            properties.Add(nameof(ActivatedBy), ActivatedBy);
            return properties;
        }
    }
}
