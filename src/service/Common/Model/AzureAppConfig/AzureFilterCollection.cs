using System.Linq;
using Newtonsoft.Json;

namespace Microsoft.FeatureFlighting.Common.Model.AzureAppConfig
{
    public class AzureFilterCollection
    {
        [JsonProperty("client_filters")]
        public AzureFilter[] Client_Filters { get; set; }

        public bool IsValid(out string validationErrorMessage)
        {
            validationErrorMessage = null;
            if (Client_Filters == null || !Client_Filters.Any())
                return true;

            foreach(var filter in Client_Filters)
            {
                if (!filter.IsValid(out string filterErrorMessage))
                {
                    validationErrorMessage = filterErrorMessage;
                    return false;
                }
            }
            return true;
        }
    }
}
