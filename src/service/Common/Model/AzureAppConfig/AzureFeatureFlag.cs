using System;
using System.Linq;
using Newtonsoft.Json;

namespace Microsoft.FeatureFlighting.Common.Model.AzureAppConfig
{
    /// <summary>
    /// Feature Flag configuration in Azure App Configuration
    /// </summary>
    [Serializable]
    public class AzureFeatureFlag
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("tenant")]
        public string Tenant { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("incrementalRingsEnabled")]
        public bool IncrementalRingsEnabled { get; set; }

        [JsonProperty("isFlagOptimized")]
        public bool IsFlagOptimized { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("environment")]
        public string Environment { get; set; }

        [JsonProperty("conditions")]
        public AzureFilterCollection Conditions { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }


        public AzureFeatureFlag() { }
        public AzureFeatureFlag(string name)
        {
            Name = name;
        }

        public bool IsValid(out string validationErrorMessage)
        {
            if (string.IsNullOrWhiteSpace(Tenant))
            {
                validationErrorMessage = "Tenant cannot be null";
                return false;
            }   
            if (string.IsNullOrWhiteSpace(Environment))
            {
                validationErrorMessage = "Environment cannot be null";
                return false;
            }
            if (Conditions != null && Conditions.Client_Filters != null && Conditions.Client_Filters.Any())
            {
                if (!Conditions.IsValid(out string conditionsErrorMessage))
                {
                    validationErrorMessage = conditionsErrorMessage;
                    return false;
                }
            }
            validationErrorMessage = null;
            return true;
        }
    }
}
