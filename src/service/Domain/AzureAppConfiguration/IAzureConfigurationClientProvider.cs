using Azure.Data.AppConfiguration;

namespace Microsoft.FeatureFlighting.Core.AzureAppConfiguration
{
    /// <summary>
    /// Creates the client for connecting to Azure App Configuration
    /// </summary>
    public interface IAzureConfigurationClientProvider
    {
        /// <summary>
        /// Creates <see cref="ConfigurationClient"/>
        /// </summary>
        /// <returns>Configuration Client</returns>
        ConfigurationClient GetConfigurationClient();
    }
}
