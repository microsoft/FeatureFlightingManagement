using System;
using Microsoft.Azure.KeyVault;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

namespace Microsoft.PS.Services.FlightingService.Api
{
    [ExcludeFromCodeCoverage]
    public static class Program
    {
        public static void Main(string[] args)
        {   
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureAppConfiguration((context, config) =>
                    {
                        AddKeyVault(config);
                        AddAzureAppConfiguration(config);
                    });
                    webBuilder.UseStartup<Startup>();
                });

        private static void AddKeyVault(IConfigurationBuilder config)
        {
            var builtConfig = config.Build();
            var azureTokenProvider = new AzureServiceTokenProvider();
            var tokenCallback = new KeyVaultClient.AuthenticationCallback(azureTokenProvider.KeyVaultTokenCallback);
            var keyVaultClient = new KeyVaultClient(tokenCallback);

            config.AddAzureKeyVault(new AzureKeyVaultConfigurationOptions()
            {
                Vault = builtConfig["KeyVault:EndpointUrl"],
                Client = keyVaultClient,
                Manager = new DefaultKeyVaultSecretManager(),
                ReloadInterval = TimeSpan.FromHours(int.Parse(builtConfig["KeyVault:PollingIntervalInHours"]))
            });
        }

        private static void AddAzureAppConfiguration(IConfigurationBuilder config)
        {
            IConfigurationRoot builtConfig = config.Build();
            string appConfigurationConnectionStringLocation = builtConfig["AppConfiguration:ConnectionStringLocation"];
            string appConfigurationConnectionString = builtConfig[appConfigurationConnectionStringLocation];
            string flightingAppConfigLabel = builtConfig["AppConfiguration:FeatureFlightsLabel"];
            string configurationCommonLabel = builtConfig["AppConfiguration:ConfigurationCommonLabel"];
            string configurationEnvLabel = builtConfig["AppConfiguration:ConfigurationEnvLabel"];

            config.AddAzureAppConfiguration(options =>
            {
                options
                    .Connect(appConfigurationConnectionString)
                    .UseFeatureFlags(configure =>
                    {
                        configure.Label = flightingAppConfigLabel;
                    })
                    .Select(KeyFilter.Any, configurationCommonLabel)
                    .Select(KeyFilter.Any, configurationEnvLabel);
            });
        }
    }
}
