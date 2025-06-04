using Azure;
using System;
using Azure.Identity;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using System.Diagnostics.CodeAnalysis;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using Autofac.Extensions.DependencyInjection;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Azure.Core;

namespace Microsoft.PS.Services.FlightingService.Api
{
    [ExcludeFromCodeCoverage]
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
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
        }

        private static void AddKeyVault(IConfigurationBuilder config)
        {
            var builtConfig = config.Build();
            TokenCredential credential;
#if DEBUG
            credential = new VisualStudioCredential();
#else
            credential = new ManagedIdentityCredential();
#endif

            config.AddAzureKeyVault(
                new SecretClient(
                    new Uri(builtConfig["KeyVault:EndpointUrl"]),
                    credential
                    ),
                new AzureKeyVaultConfigurationOptions()
                {
                    ReloadInterval = TimeSpan.FromHours(int.Parse(builtConfig["KeyVault:PollingIntervalInHours"]))
                }
            );

        }

        private static void AddAzureAppConfiguration(IConfigurationBuilder config)
        {
            IConfigurationRoot builtConfig = config.Build();
            string appConfigurationUri = builtConfig["AzureAppConfigurationUri"];
            string flightingAppConfigLabel = builtConfig["AppConfiguration:FeatureFlightsLabel"];
            string configurationCommonLabel = builtConfig["AppConfiguration:ConfigurationCommonLabel"];
            string configurationEnvLabel = builtConfig["AppConfiguration:ConfigurationEnvLabel"];
            TokenCredential credential;
#if DEBUG
            credential = new VisualStudioCredential();
#else
            credential = new ManagedIdentityCredential();
#endif

            config.AddAzureAppConfiguration(options =>
            {
                options
                    .Connect(new Uri(appConfigurationUri), credential)
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
