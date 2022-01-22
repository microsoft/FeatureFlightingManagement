using Newtonsoft.Json;

namespace Microsoft.FeatureFlighting.Common.Model.AzureAppConfig
{
    public class AzureFilter
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("parameters")]
        public AzureFilterParameters Parameters { get; set; }

        public bool IsValid(out string validationErrorMessage)
        {   
            if (string.IsNullOrWhiteSpace(Name) || Parameters == null)
            {
                validationErrorMessage = "Filter name and filer parameters must be present";
                return false;
            }
            if (!int.TryParse(Parameters.StageId, out _))
            {
                validationErrorMessage = "Stage ID must be a number";
                return false;
            }
            if (!bool.TryParse(Parameters.IsActive, out _))
            {
                validationErrorMessage = "IsActive must be boolean";
                return false;
            }
            validationErrorMessage = null;
            return true;
        }

        public bool IsActive()
        {
            return !string.IsNullOrWhiteSpace(Parameters.IsActive) &&
                Parameters.IsActive.ToLowerInvariant() == bool.TrueString.ToLowerInvariant();
        }
    }
}
