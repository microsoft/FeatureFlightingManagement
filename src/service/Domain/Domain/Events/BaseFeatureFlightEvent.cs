using System;
using Newtonsoft.Json;
using CQRS.Mediatr.Lite;
using System.Collections.Generic;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Common.Model;
using Microsoft.FeatureFlighting.Core.Domain.Assembler;

namespace Microsoft.FeatureFlighting.Core.Domain.Events
{
    internal abstract class BaseFeatureFlightEvent: Event
    {   
        public override string Id { get; set; }
        public string FlagId { get; set; }
        public string FeatureName { get; set; }
        public string TenantName { get; set; }
        public string Environment { get; set; }
        public bool Enabled { get; set; }
        public bool IsIncremental { get; set; }
        public bool FlightOptimized { get; set; }
        public FeatureFlightDto Payload { get; set; }

        public BaseFeatureFlightEvent(FeatureFlightAggregateRoot flight, LoggerTrackingIds trackingIds)
        {
            Id = Guid.NewGuid().ToString();
            FlagId = flight.Id;
            FeatureName = flight.Feature.Name;
            TenantName = flight.Tenant.Id;
            Environment = flight.Tenant.Environment;
            Enabled = flight.Status.Enabled;
            IsIncremental = flight.Condition.IncrementalActivation;
            FlightOptimized = flight.ProjectedFlag != null && flight.ProjectedFlag.IsFlagOptimized;
            CorrelationId = trackingIds.CorrelationId;
            TransactionId = trackingIds.TransactionId;
            Payload = FeatureFlightDtoAssembler.Assemble(flight);
        }

        public virtual Dictionary<string, string> GetProperties()
        {
            return new Dictionary<string, string>
            {
                { nameof(FlagId), FlagId },
                { nameof(FeatureName), FeatureName},
                { nameof(TenantName), TenantName},
                { nameof(Environment), Environment },
                { nameof(Enabled), Enabled.ToString() },
                { nameof(IsIncremental), IsIncremental.ToString() },
                { nameof(FlightOptimized), FlightOptimized.ToString() },
                { nameof(Payload), JsonConvert.SerializeObject(Payload) }
            };
        }
    }
}
