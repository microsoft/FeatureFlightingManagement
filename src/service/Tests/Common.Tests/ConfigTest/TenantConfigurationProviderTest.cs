using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.FeatureFlighting.Common.Tests.ConfigTest
{
    [ExcludeFromCodeCoverage]
    [TestCategory("TenantConfigurationProvider")]
    [TestClass]
    public class TenantConfigurationProviderTest
    {
        private IConfiguration _mockConfiguration;
        private TenantConfiguration _defaultTenantConfiguration;
        public TenantConfigurationProviderTest() {

            var singleTenantData=JsonConvert.SerializeObject(GetTenantConfiguration());
            var multiTenantData=JsonConvert.SerializeObject(GetTenantConfigurations());
            var webhookData=JsonConvert.SerializeObject(GetWebhookConfiguration());
            var testConfig1 = new Mock<IConfigurationSection>();
            testConfig1.Setup(s => s.Value).Returns(multiTenantData);

            _mockConfiguration = new ConfigurationBuilder().AddJsonFile(@"appsettings.test.json").Build();
        }
        [TestMethod]
        public async Task Get_ShouldReturnTenantConfiguration_WhenTenantExists()
        {
            // Arrange
            var tenantConfigurationProvider = new TenantConfigurationProvider(_mockConfiguration);
            
            // Act
            var result = await tenantConfigurationProvider.Get("Tenant");

            // Assert
            Assert.IsNotNull(result);
        }

        #region Test Data
        private TenantConfiguration GetTenantConfiguration()
        {
            return new TenantConfiguration
            {
                Name = "Tenant",
                ShortName = "Tenant",
                Authorization = new AuthorizationConfiguration(),
                Cache = new CacheConfiguration(),
                Evaluation = FlagEvaluationConfiguration.GetDefault(),
                FlightsDatabase = new CosmosDbConfiguration() {
                    Disabled = false,
                    PrimaryKeyLocation="test location"
                },
                Optimization = FlightOptimizationConfiguration.GetDefault(),
                ChangeNotificationSubscription = new TenantChangeNotificationConfiguration{ IsSubscribed=true,Webhook=new WebhookConfiguration { WebhookId= "EventStore" } },
                Metrics = MetricConfiguration.GetDefault(),
                BusinessRuleEngine = new BusinessRuleEngineConfiguration() {
                    Storage = new StorageConfiguration()
                    {
                        StorageConnectionString = "connectionString",
                        ContainerName = "containerName",
                        StorageConnectionStringKey = "connectionStringKey"
                    },
                    Enabled = false
         
                }
            };
        }

        private WebhookConfiguration GetWebhookConfiguration()
        {
            return new WebhookConfiguration { WebhookId = "EventStore",ClientSecret="test1",ClientSecretLocation="test2",ClientId="test3" };
        }


        private List<TenantConfiguration> GetTenantConfigurations()
        {
            return new List<TenantConfiguration> {

                new TenantConfiguration
            {
                Name = "Tenant",
                ShortName = "Tenant",
                Authorization = new AuthorizationConfiguration(),
                Cache = new CacheConfiguration(),
                Evaluation = FlagEvaluationConfiguration.GetDefault(),
                FlightsDatabase = new CosmosDbConfiguration()
                {
                    Disabled = false,
                },
                Optimization = FlightOptimizationConfiguration.GetDefault(),
                ChangeNotificationSubscription = new TenantChangeNotificationConfiguration{ IsSubscribed=true,Webhook=new WebhookConfiguration { WebhookId= "EventStore" } },
                Metrics = MetricConfiguration.GetDefault(),
                BusinessRuleEngine = new BusinessRuleEngineConfiguration()
                {
                    Storage = new StorageConfiguration()
                    {
                        StorageConnectionString = "connectionString",
                        ContainerName = "containerName",
                        StorageConnectionStringKey = "connectionStringKey"
                    },
                    Enabled = false

                }
            },
                new TenantConfiguration
            {
                Name = "Tenant",
                ShortName = "Tenant",
                FlightsDatabase = new CosmosDbConfiguration()
                {
                    Disabled = false,
                },
                ChangeNotificationSubscription = new TenantChangeNotificationConfiguration{ IsSubscribed=true,Webhook=new WebhookConfiguration { WebhookId= "EventStore" } },
                Metrics = new MetricConfiguration{
                    Enabled = true,
                AppInsightsName = "ai-feature-flights-management-prod   ",
                TrackingEventName = "Flighting:FeatureFlags:Evaluated"},
                BusinessRuleEngine = new BusinessRuleEngineConfiguration()
                {
                    Storage = new StorageConfiguration()
                    {
                        StorageConnectionString = "connectionString",
                        ContainerName = "containerName",
                        StorageConnectionStringKey = "connectionStringKey"
                    },
                    Enabled = false

                }
            }
            };
        }
        #endregion
    }
}
