using System.Collections.Generic;

namespace Microsoft.FeatureFlighting.Common.Model.ChangeNotification
{
    public class FeatureFlightChangeEvent
    {
        public string EventName { get; set; }
        public string EventType { get; set; }
        public string EventSubject { get; set; }
        public Dictionary<string, string> Properties { get; set; }
        public string Payload { get; set; }
        public EventPublisher Publisher { get; set; }
    }
}
