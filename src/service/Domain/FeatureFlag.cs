using System;
using Newtonsoft.Json;
using Microsoft.FeatureFlighting.Core.FeatureFilters;

namespace Microsoft.FeatureFlighting.Core
{
    [Serializable]
    public class FeatureFlag
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }
        [JsonProperty("enableIncrementalRings")]
        public bool EnableIncrementalRings { get; set; }
        [JsonProperty("label")]
        public string Label { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("environment")]
        public string Environment { get; set; }
        [JsonProperty("conditions")]
        public Condition Conditions { get; set; }

        public FeatureFlag() { }
        public FeatureFlag(string name)
        {
            Name = name;
        }
    }

    public class Condition
    {
        [JsonProperty("client_filters")]
        public Filter[] Client_Filters { get; set; }
    }

    public class Filter
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("parameters")]
        public FilterSettings Parameters { get; set; }
    }
}
