using Newtonsoft.Json;
using CQRS.Mediatr.Lite;
using System.Threading.Tasks;
using System.Collections.Generic;
using AppInsights.EnterpriseTelemetry;
using AppInsights.EnterpriseTelemetry.Context;
using Microsoft.FeatureFlighting.Core.Domain.Events;

namespace Microsoft.FeatureFlighting.Core.Events.TelemetryHandlers
{
    internal abstract class BaseFeatureFlightEventTelemetryHandler<TEvent>: EventHandler<TEvent> where TEvent: BaseFeatureFlightEvent
    {
        private readonly ILogger _logger;

        public BaseFeatureFlightEventTelemetryHandler(ILogger logger)
        {
            _logger = logger;
        }

        protected override Task<VoidResult> ProcessRequest(TEvent @event)
        {
            try
            {
                Dictionary<string, string> properties = @event.GetProperties();
                if (@event.Payload != null)
                    properties.Add("FeatureFlight", JsonConvert.SerializeObject(@event.Payload));
                if (@event is FeatureFlightUpdated featureFlightUpdatedEvent)
                    properties.AddOrUpdate(nameof(featureFlightUpdatedEvent.OriginalPayload), JsonConvert.SerializeObject(featureFlightUpdatedEvent.OriginalPayload));

                EventContext eventContext = new(@event.DisplayName, @event.CorrelationId, @event.TransactionId, "Core", "", @event.FlagId);
                eventContext.AddProperties(properties);
                _logger.Log(eventContext);
            }
            catch (System.Exception exception) 
            {
                _logger.Log(exception);
                // Do not throw exception if logging fails
            }
            return Task.FromResult(new VoidResult());
        }
    }
}
