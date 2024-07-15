using System;
using Azure.Core;
using Azure.Data.AppConfiguration;
using Azure.Identity;
using Microsoft.Extensions.Configuration;

namespace Microsoft.FeatureFlighting.Infrastructure.AppConfig
{   
    // <inheritdoc/>
    internal class AzureConfigurationClientProvider : IAzureConfigurationClientProvider
    {
        private static ConfigurationClient? _configurationClient;
        private readonly IConfiguration _configuration;
        private static readonly object _lock = new();

        public AzureConfigurationClientProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // <inheritdoc/>
        public ConfigurationClient GetConfigurationClient()
        {
            lock( _lock)
            {
                if (_configurationClient != null)
                    return _configurationClient;

                var options = new ConfigurationClientOptions();

                options.Retry.Mode = RetryMode.Exponential;
                options.Retry.MaxRetries = 10;
                options.Retry.Delay = TimeSpan.FromSeconds(1);

                string appConfigUri = _configuration["AzureAppConfigurationUri"];
                _configurationClient = new ConfigurationClient(new Uri(appConfigUri), new DefaultAzureCredential(), options);
                return _configurationClient;

            }
        }
    }
}
