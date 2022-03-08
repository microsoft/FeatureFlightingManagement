using Azure.Data.AppConfiguration;

namespace Microsoft.FeatureFlighting.Infrastructure.AppConfig
{
    /// <summary>
    /// Creates the client for connecting to Azure App Configuration
    /// </summary>
    internal interface IAzureConfigurationClientProvider
    {
        /// <summary>
        /// Creates <see cref="ConfigurationClient"/>
        /// </summary>
        /// <returns>Configuration Client</returns>
        ConfigurationClient GetConfigurationClient();
    }
}
