namespace Microsoft.FeatureFlighting.Common.Model.ChangeNotification
{
    public class EventPublisher
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public EventPublisher() { }

        public EventPublisher(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
