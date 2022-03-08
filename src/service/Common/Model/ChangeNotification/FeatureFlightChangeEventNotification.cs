namespace Microsoft.FeatureFlighting.Common.Model.ChangeNotification
{
    public class FeatureFlightChangeEventNotification: FeatureFlightChangeEvent
    {
        public NotificationCollection Notification { get; set; }
    }
}
