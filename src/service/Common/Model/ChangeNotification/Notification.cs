using System.Collections.Generic;

namespace Microsoft.FeatureFlighting.Common.Model.ChangeNotification
{
    public class Notification
    {
        public string SenderAddress { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public List<string> ReceiverAddresses { get; set; }
        public List<string> AlternateReceiverAddreses { get; set; }
    }
}
