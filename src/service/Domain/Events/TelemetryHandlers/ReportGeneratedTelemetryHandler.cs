using CQRS.Mediatr.Lite;
using System.Threading.Tasks;
using AppInsights.EnterpriseTelemetry;
using AppInsights.EnterpriseTelemetry.Context;
using Microsoft.FeatureFlighting.Core.Domain.Events;

namespace Microsoft.FeatureFlighting.Core.Events.TelemetryHandlers
{
    /// <summary>
    /// Handles <see cref="ReportGenerated"/>
    /// </summary>
    internal class ReportGeneratedTelemetryHandler : EventHandler<ReportGenerated>
    {
        private readonly ILogger _logger;
        public ReportGeneratedTelemetryHandler(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Logs the usage report
        /// </summary>
        protected override Task<VoidResult> ProcessRequest(ReportGenerated @event)
        {
            try
            {
                EventContext context = new(@event.DisplayName, @event.CorrelationId, @event.TransactionId,
                "ReportGeneratedTelemetryHandler", @event.GeneratedBy, @event.Id);
                context.AddProperties(@event.CreateProperties());
                _logger.Log(context);
            }
            catch (System.Exception) { }

            return Task.FromResult(new VoidResult());
        }
    }
}
