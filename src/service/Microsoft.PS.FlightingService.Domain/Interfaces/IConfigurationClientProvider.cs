using Azure.Data.AppConfiguration;

namespace Microsoft.PS.FlightingService.Domain.Interfaces
{
    public interface IConfigurationClientProvider
    {
        ConfigurationClient GetConfigurationClient();
    }
}
