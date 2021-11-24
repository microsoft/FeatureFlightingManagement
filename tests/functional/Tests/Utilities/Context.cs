using Newtonsoft.Json;

namespace Microsoft.FeatureFlighting.Tests.Functional.Utilities
{
    public class Context
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Role { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Region { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Alias { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Country { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Upn { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string number { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string RoleGroup { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Date { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string EOU { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string UserPrincipalName { get; set; }
    }
}