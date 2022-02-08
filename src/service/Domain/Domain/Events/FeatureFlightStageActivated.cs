using System.Linq;
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
        public List<string> ActiveStages { get; set; }
        public List<string> InactiveStages { get; set; }

        public FeatureFlightStageActivated(FeatureFlightAggregateRoot flight, string activatedStageName, LoggerTrackingIds trackingIds, string source)
            : base(flight, trackingIds, source)
        {
            ActivatedStageName = activatedStageName;
            ActivatedBy = flight.Audit.LastModifiedBy;
            ActiveStages = flight.Condition.GetActiveStages()?.Select(stage => stage.Name).ToList();
            InactiveStages = flight.Condition.GetInactiveStages()?.Select(stage => stage.Name).ToList();
        }

        public override Dictionary<string, string> GetProperties()
        {
            Dictionary<string, string> properties = base.GetProperties();
            properties.Add(nameof(ActivatedStageName), ActivatedStageName);
            properties.Add(nameof(ActivatedBy), ActivatedBy);
            properties.Add(nameof(ActiveStages), ActiveStages != null ? string.Join(',', ActiveStages) : "N/A");
            properties.Add(nameof(InactiveStages), InactiveStages != null ? string.Join(',', InactiveStages) : "N/A");
            return properties;
        }
    }
}
