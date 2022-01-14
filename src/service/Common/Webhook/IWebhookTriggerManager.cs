using System.Threading.Tasks;
using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.FeatureFlighting.Common.Model.ChangeNotification;

namespace Microsoft.FeatureFlighting.Common.Webhook
{
    /// <summary>
    /// Manages webhook triggers
    /// </summary>
    public interface IWebhookTriggerManager
    {
        /// <summary>
        /// Triggers a webhook
        /// </summary>
        /// <param name="webhook" cref="WebhookConfiguration">Webhook details</param>
        /// <param name="event" cref="FeatureFlightChangeEvent">Event containing the change details</param>
        /// <param name="trackingIds" cref="LoggerTrackingIds">Tracking IDs</param>
        /// <returns>Response message from webhook</returns>
        Task<string> Trigger(WebhookConfiguration webhook, FeatureFlightChangeEvent @event, LoggerTrackingIds trackingIds);
    }
}
