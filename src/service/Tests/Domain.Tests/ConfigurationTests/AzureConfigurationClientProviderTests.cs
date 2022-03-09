//using Moq;
//using Azure;
//using System.Threading;
//using System.Threading.Tasks;
//using Azure.Data.AppConfiguration;
//using Microsoft.Extensions.Configuration;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Microsoft.FeatureFlighting.Core.AzureAppConfiguration;

//namespace Microsoft.FeatureFlighting.Services.Tests
//{
//    [TestClass]
//    [TestCategory("AzureConfigurationClientProviderTests")]
//    public class AzureConfigurationClientProviderTests
//    {
//        #region variables
//        private Mock<IConfiguration> config = new Mock<IConfiguration>();
//        private readonly string appName = "fxp";
//        private readonly string _featureFlagContentType = "application/vnd.microsoft.appconfig.ff+json;charset=utf-8";
//        private readonly string _featureFlagPrefix = ".appconfig.featureflag/";
//        private static readonly string str = @"
//            {
//                'id': 'fxp_dev_Flag1',
//                'description': 'Alias validation',
//                'enabled': true,
//                'label': 'PPE',
//                'conditions': {
//                            'client_filters': [
//                                {
//                            'name': 'Alias',
//                                    'parameters': {
//                                'Operator': 'Equals',
//                                        'Value': 'bhchalas',
//                                        'IsActive': 'true',
//                                        'StageId': '0'
//                            }
//            }
//                    ]
//                }
//            }";
//        #endregion

//        [TestInitialize]
//        public void TestStartup()
//        {
//            SetConfig();
//            SetConfigClient();
//        }

//        [TestMethod]
//        public void GetConfigurationClient_Succeds()
//        {
//            var configurationClient = new AzureConfigurationClientProvider(config.Object);
//            configurationClient.GetConfigurationClient();
//        }

//        private void SetConfig()
//        {
//            var configurationSection = new Mock<IConfigurationSection>();
//            configurationSection.Setup(a => a.Value).Returns("testvalue");

//            var configurationSectionLocation = new Mock<IConfigurationSection>();
//            configurationSectionLocation.Setup(a => a.Value).Returns("AppConfiguration-ConnectionString");

//            var configurationSectionApp = new Mock<IConfigurationSection>();
//            configurationSectionApp.Setup(a => a.Value).Returns("Endpoint=https://tesingpoc.azconfig.io;Id=0-s0:Dwk1fZx;Secret=testing=");

//            config.Setup(a => a.GetSection("Env:Label")).Returns(configurationSection.Object);
//            config.Setup(a => a.GetSection("AppConfiguration:ConnectionStringLocation")).Returns(configurationSectionLocation.Object);
//            config.Setup(a => a.GetSection("AppConfiguration-ConnectionString")).Returns(configurationSectionApp.Object);
//        }

//        private void SetConfigClient()
//        {
//            Mock<Response<ConfigurationSetting>> response = new Mock<Response<ConfigurationSetting>>();
//            Mock<ConfigurationClient> configClient = new Mock<ConfigurationClient>();
//            var selector = new SettingSelector()
//            {
//                KeyFilter = _featureFlagPrefix + appName.ToLower(),
//                LabelFilter = "PPE"
//            };
//            var setting = new ConfigurationSetting(_featureFlagPrefix + "fxp_dev_Flag1", str, "PPE")
//            {
//                ContentType = _featureFlagContentType,
//                Value = "abc"
//            };
//            response.Setup(_ => _.Value).Returns(setting);

//            configClient
//                .Setup(_ => _.AddConfigurationSettingAsync(It.IsAny<string>(), It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
//                .Returns(Task.FromResult(response.Object));

//            configClient.Setup(_ => _.GetConfigurationSettingAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(response.Object));
//            configClient.Setup(_ => _.SetConfigurationSettingAsync(_featureFlagPrefix + appName.ToLower(), "abc", "PPE", It.IsAny<CancellationToken>()))
//                .Returns(Task.FromResult(response.Object));
//            configClient.Setup(_ => _.SetConfigurationSettingAsync(_featureFlagPrefix + appName.ToLower(), "abc", "PPE", It.IsAny<CancellationToken>()))
//                .Returns(Task.FromResult(response.Object));
//        }
//    }
//}
