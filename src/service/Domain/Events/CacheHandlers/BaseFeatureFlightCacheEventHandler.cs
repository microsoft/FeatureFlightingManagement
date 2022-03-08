using CQRS.Mediatr.Lite;
using System.Threading.Tasks;
using AppInsights.EnterpriseTelemetry;
using Microsoft.FeatureFlighting.Core.Spec;
using AppInsights.EnterpriseTelemetry.Context;
using Microsoft.FeatureFlighting.Core.Domain.Events;

namespace Microsoft.FeatureFlighting.Core.Events.CacheHandlers
{
    internal abstract class BaseFeatureFlightCacheEventHandler<TEvent> : EventHandler<TEvent> where TEvent : BaseFeatureFlightEvent
    {
        private readonly IFeatureFlightCache _cache;
        private readonly ILogger _logger;

        public BaseFeatureFlightCacheEventHandler(IFeatureFlightCache cache, ILogger logger)
        {
            _cache = cache;
            _logger = logger;
        }

        protected override async Task<VoidResult> ProcessRequest(TEvent @event)
        {
            try
            {
                await _cache.DeleteFeatureFlights(@event.TenantName, @event.Environment, new Common.LoggerTrackingIds(@event.CorrelationId, @event.TransactionId));
                return new VoidResult();
            }
            catch (System.Exception exception)
            {
                System.Exception cacheDeletionException = new("There was an error in deleting the feature flight cache, please delete the cache manually to avoid any errors. See inner exception for more details", exception);
                ExceptionContext context = new(cacheDeletionException, TraceLevel.Critical, @event.CorrelationId, @event.TransactionId, $"{@event.DisplayName}:CacheHandler:{nameof(ProcessRequest)}", "", @event.FlagId);
                context.AddProperty("FlagId", @event.FlagId);
                _logger.Log(context);
                throw cacheDeletionException;
            }
        }
    }
}
