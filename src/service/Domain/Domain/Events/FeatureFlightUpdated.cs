using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Common.Model;

namespace Microsoft.FeatureFlighting.Core.Domain.Events
{
    /// <summary>
    /// Event when an feature flight is updated
    /// </summary>
    internal class FeatureFlightUpdated : BaseFeatureFlightEvent
    {
        public override string DisplayName => nameof(FeatureFlightUpdated);

        public string UpdatedBy { get; set; }
        public string UpdateType { get; set; }
        public FeatureFlightDto OriginalPayload { get; set; }

        public FeatureFlightUpdated(FeatureFlightAggregateRoot flight, FeatureFlightDto originalPayload, string updateType, LoggerTrackingIds trackingIds)
            : base(flight, trackingIds)
        {
            UpdatedBy = flight.Audit.LastModifiedBy;
            OriginalPayload = originalPayload;
            UpdateType = updateType;
        }

        public override Dictionary<string, string> GetProperties()
        {
            Dictionary<string, string> properties = base.GetProperties();
            properties.AddOrUpdate(nameof(UpdatedBy), UpdatedBy);
            properties.AddOrUpdate(nameof(UpdateType), UpdateType);
            properties.AddOrUpdate(nameof(OriginalPayload), JsonConvert.SerializeObject(OriginalPayload));
            return properties;
        }
    }
}
