using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Data.AppConfiguration;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Domain.Interfaces;
using System;

namespace Microsoft.FeatureFlighting.Domain.Configuration
{
    public class ConfigurationClientProvider : IConfigurationClientProvider
    {
        ConfigurationClient _configurationClient;
        readonly IConfiguration _configuration;

        public ConfigurationClientProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public ConfigurationClient GetConfigurationClient()
        {
            if (_configurationClient == null)
            {
                var options = new ConfigurationClientOptions();

                options.Retry.Mode = RetryMode.Exponential;
                options.Retry.MaxRetries = 10;
                options.Retry.Delay = TimeSpan.FromSeconds(1);

                var conString = _configuration.GetSection("AppConfigConString").Value;
                _configurationClient = new ConfigurationClient(conString, options);
            }

            return _configurationClient;
        }
    }
}
