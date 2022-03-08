using System.Collections.Generic;

namespace Microsoft.FeatureFlighting.Common.Model.ChangeNotification
{
    public class EmailNotification
    {
        public bool Enabled { get; set; }
        public string Channel { get;set; }
        public List<Notification> Notifications { get; set; }
    }
}
