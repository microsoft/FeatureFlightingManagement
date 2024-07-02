namespace Microsoft.FeatureFlighting.Common.Config
{
    /// <summary>
    /// Configuration to connect to an HTTP webhook
    /// </summary>
    public class WebhookConfiguration
    {
        public string WebhookId { get; set; }
        public string BaseEndpoint { get; set; }
        public string Uri { get; set; }
        public string HttpMethod { get; set; }
        public string AuthenticationAuthority { get; set; }
        public string ResourceId { get; set; }
        public string ClientId { get; set; }   
    }
}
