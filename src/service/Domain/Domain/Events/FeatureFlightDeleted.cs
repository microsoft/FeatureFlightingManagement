using System.Collections.Generic;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Common.Model;
using Microsoft.FeatureFlighting.Core.Domain.Assembler;

namespace Microsoft.FeatureFlighting.Core.Domain.Events
{
    /// <summary>
    /// Event when a feature flight is deleted
    /// </summary>
    internal class FeatureFlightDeleted: BaseFeatureFlightEvent
    {
        public override string DisplayName => nameof(FeatureFlightDeleted);

        public string DeletedBy { get; set; }

        public FeatureFlightDeleted(FeatureFlightAggregateRoot flight, LoggerTrackingIds trackingIds, string source)
            : base(flight, trackingIds, source)
        {
            DeletedBy = flight.Audit.LastModifiedBy;
            Payload = FeatureFlightDtoAssembler.Assemble(flight);
        }

        public override Dictionary<string, string> GetProperties()
        {
            Dictionary<string, string> properties = base.GetProperties();
            properties.AddOrUpdate(nameof(DeletedBy), DeletedBy);
            return properties;
        }
    }
}
