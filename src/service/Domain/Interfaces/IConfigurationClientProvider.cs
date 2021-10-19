using Azure.Data.AppConfiguration;

namespace Microsoft.FeatureFlighting.Domain.Interfaces
{
    public interface IConfigurationClientProvider
    {
        ConfigurationClient GetConfigurationClient();
    }
}
