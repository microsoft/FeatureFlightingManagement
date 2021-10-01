using Azure;
using Azure.Data.AppConfiguration;
using Microsoft.Extensions.Configuration;
using Microsoft.PS.FlightingService.Domain.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace Microsoft.PS.FlightingService.Domain.Tests.ConfigurationTests
{
    [TestClass]
    [TestCategory("ConfigurationClientProvider")]
    public class ConfigurationClientProviderTests
    {
        #region variables
        private Mock<IConfiguration> config = new Mock<IConfiguration>();
        private readonly string appName = "fxp";
        private readonly string _featureFlagContentType = "application/vnd.microsoft.appconfig.ff+json;charset=utf-8";
        private readonly string _featureFlagPrefix = ".appconfig.featureflag/";
        private static readonly string str = @"
            {
                'id': 'fxp_dev_Flag1',
                'description': 'Alias validation',
                'enabled': true,
                'label': 'PPE',
                'conditions': {
                            'client_filters': [
                                {
                            'name': 'Alias',
                                    'parameters': {
                                'Operator': 'Equals',
                                        'Value': 'bhchalas',
                                        'IsActive': 'true',
                                        'StageId': '0'
                            }
            }
                    ]
                }
            }";
        private FeatureFlag featureFlag = JsonConvert.DeserializeObject<FeatureFlag>(str);
        #endregion

        [TestInitialize]
        public void TestStartup()
        {
            SetConfig();
            SetConfigClient();
        }

        [TestMethod]
        public void GetConfigurationClient_Succeds()
        {
            ConfigurationClientProvider configurationClient = new ConfigurationClientProvider(config.Object);
            configurationClient.GetConfigurationClient();
        }

        private void SetConfig()
        {
            var configurationSection = new Mock<IConfigurationSection>();
            configurationSection.Setup(a => a.Value).Returns("testvalue");

            var configurationSectionApp = new Mock<IConfigurationSection>();
            configurationSectionApp.Setup(a => a.Value).Returns("Endpoint=https://tesingpoc.azconfig.io;Id=0-s0:Dwk1fZx;Secret=testing=");

            config.Setup(a => a.GetSection("Env:Label")).Returns(configurationSection.Object);
            config.Setup(a => a.GetSection("AppConfigConString")).Returns(configurationSectionApp.Object);
        }

        private void SetConfigClient()
        {
            Mock<Response<ConfigurationSetting>> response = new Mock<Response<ConfigurationSetting>>();
            Mock<ConfigurationClient> configClient = new Mock<ConfigurationClient>();
            var selector = new SettingSelector()
            {
                KeyFilter = _featureFlagPrefix + appName.ToLower(),
                LabelFilter = "PPE"
            };
            var setting = new ConfigurationSetting(_featureFlagPrefix + featureFlag.Id, str, "PPE")
            {
                ContentType = _featureFlagContentType,
                Value = "abc"
            };
            response.Setup(_ => _.Value).Returns(setting);

            configClient
                .Setup(_ => _.AddConfigurationSettingAsync(It.IsAny<string>(), It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(response.Object));

            configClient.Setup(_ => _.GetConfigurationSettingAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(response.Object));
            configClient.Setup(_ => _.SetConfigurationSettingAsync(_featureFlagPrefix + appName.ToLower(), "abc", "PPE", It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(response.Object));
            configClient.Setup(_ => _.SetConfigurationSettingAsync(_featureFlagPrefix + appName.ToLower(), "abc", "PPE", It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(response.Object));
        }
    }
}
