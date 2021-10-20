using System;
using Microsoft.Azure.KeyVault;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration.AzureKeyVault;

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
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureAppConfiguration((context, config) =>
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

                        // TODO: Add azure app configuration
                        builtConfig = config.Build();
                        config.AddAzureAppConfiguration(options =>
                        {
                            options
                                .Connect(builtConfig["AzureAppConfigurationConnectionString"])
                                .UseFeatureFlags(configure =>
                                {
                                    configure.Label = builtConfig["Env:Label"];
                                });
                        });
                        //string azureAppConfigurationConnectionString = builtConfig["AzureAppConfigConnectionstring"];
                        //config.AddAzureAppConfiguration(options =>
                        //    options
                        //        .Connect(azureAppConfigurationConnectionString)
                        //        .Select(KeyFilter.Any, builtConfig["AzureAppConfiguration:CommonLabel"])
                        //        .Select(KeyFilter.Any, builtConfig["AzureAppConfiguration:WebNotificationApiLabel"]));
                    });
                    webBuilder.UseStartup<Startup>();
                });
    }
}
