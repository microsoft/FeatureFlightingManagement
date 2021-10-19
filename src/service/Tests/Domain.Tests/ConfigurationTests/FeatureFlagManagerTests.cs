using Azure;
using Azure.Data.AppConfiguration;
using Microsoft.Extensions.Configuration;
using AppInsights.EnterpriseTelemetry;
using AppInsights.EnterpriseTelemetry.Context;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Common.AppExcpetions;
using Microsoft.FeatureFlighting.Common.Caching;
using Microsoft.FeatureFlighting.Domain.Configuration;
using Microsoft.FeatureFlighting.Domain.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.FeatureFlighting.Domain.Tests.ConfigurationTests
{
    [TestClass]
    [TestCategory("FeatureFlagManagerTests")]
    public class FeatureFlagManagerTests
    {
        #region variables
        private readonly Mock<IConfiguration> config = new Mock<IConfiguration>();
        private readonly Mock<IConfigurationClientProvider> configClientProvider = new Mock<IConfigurationClientProvider>();
        private readonly Mock<ILogger> logger = new Mock<ILogger>();
        private readonly Mock<ICacheFactory> mockCacheFactory = new Mock<ICacheFactory>();
        private LoggerTrackingIds loggerTrackingIds;
        private FeatureFlagManager featureFlagManager;
        private const string str = @"
        {
            'id': 'fxp_dev_Flag1',
            'description': 'Check if Alias is a member of a given list',
            'enabled': true,
            'label': 'PPE',
            'name' : 'Flag1',
            'environment' : 'dev',
            'conditions': {
                'client_filters': [
                    {
                        'name': 'CheckIfAliasExist',
                        'parameters': {
                            'operator': 'MemberOfCustomGroup',
                            'value': 'bhchalas,prkulkar,ancha',
                            'isactive': 'true',
                            'StageId': '0',
                            'stageName' : 'Stage1'
                        }
                    },
                    {
                        'name': 'CheckIfAliasExist',
                        'parameters': {
                            'operator': 'Equals',
                            'value': 'prkulkar',
                            'isactive': 'false',
                            'StageId': '1',
                            'stageName' : 'Stage2'
                        }
                    }
                ]
            }
        }";
        private FeatureFlag featureFlag = JsonConvert.DeserializeObject<FeatureFlag>(str);
        private readonly string appName = "fxp";
        private readonly string envName = "dev";
        private readonly string id = "id";
        private readonly string _featureFlagContenetType = "application/vnd.microsoft.appconfig.ff+json;charset=utf-8";
        private readonly string _featureFlagPrefix = ".appconfig.featureflag/";

        #endregion

        #region Arrange
        [TestInitialize]
        public void TestStartup()
        {
            SetConfig();
            SetConfigClientProvider(str);
            SetLogger();
            SetCacheFactory();
            featureFlagManager = new FeatureFlagManager(config.Object, configClientProvider.Object, mockCacheFactory.Object, logger.Object);
        }

        private void SetConfig()
        {
            var configurationSection = new Mock<IConfigurationSection>();
            configurationSection.Setup(a => a.Value).Returns("testvalue");

            var configurationSectionApp = new Mock<IConfigurationSection>();
            configurationSectionApp.Setup(a => a.Value).Returns("testvalue");

            var configurationSectionEnvs = new Mock<IConfigurationSection>();
            configurationSectionEnvs.Setup(a => a.Value).Returns("UAT,PROD,DEV,SIT");

            config.Setup(a => a.GetSection("Env:Label")).Returns(configurationSection.Object);
            config.Setup(a => a.GetSection("AppConfigConString")).Returns(configurationSectionApp.Object);
            config.Setup(a => a.GetSection("Env:Supported")).Returns(configurationSectionEnvs.Object);
        }

        private void SetLogger()
        {
            loggerTrackingIds = new LoggerTrackingIds()
            {
                CorrelationId = "TCId",
                TransactionId = "TTId"
            };

            logger.Setup(m => m.Log(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
            logger.Setup(m => m.Log(It.IsAny<System.Exception>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
            logger.Setup(m => m.Log(It.IsAny<ExceptionContext>()));
            logger.Setup(m => m.Log(It.IsAny<MessageContext>()));
            logger.Setup(m => m.Log(It.IsAny<EventContext>()));
            logger.Setup(m => m.Log(It.IsAny<MetricContext>()));
        }

        private void SetCacheFactory()
        {
            var mockCache = new Mock<ICache>();
            mockCacheFactory.Setup(factory => factory.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(mockCache.Object);
        }

        private async IAsyncEnumerable<ConfigurationSetting> returnConfigSetting(string str)
        {
            ConfigurationSetting setting = new ConfigurationSetting(_featureFlagPrefix + featureFlag.Id, str, "PPE")
            {
                ContentType = _featureFlagContenetType,
                Value = str
            };
            yield return setting;
        }

        private void SetConfigClientProvider(string str, FeatureFlag featureFLag = null, bool setError = false)
        {
            ConfigurationSetting setting = new ConfigurationSetting(_featureFlagPrefix + featureFlag.Id, str, "PPE")
            {
                ContentType = _featureFlagContenetType,
                Value = str
            };
            Mock<Response<ConfigurationSetting>> response = new Mock<Response<ConfigurationSetting>>();
            Mock<ConfigurationClient> configClient = new Mock<ConfigurationClient>();
            Mock<AsyncPageable<ConfigurationSetting>> mockAsyncConfig = new Mock<AsyncPageable<ConfigurationSetting>>();
            Mock<Response> response1 = new Mock<Response>();
            var selector = new SettingSelector()
            {
                KeyFilter = _featureFlagPrefix + appName.ToLowerInvariant(),
                LabelFilter = "PPE"
            };
            mockAsyncConfig.Setup(_ => _.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(returnConfigSetting(str).GetAsyncEnumerator());
            response.Setup(_ => _.Value).Returns(setting);
            configClient.Setup(_ => _.AddConfigurationSettingAsync(It.IsAny<string>(), It.IsAny<string>(), null, It.IsAny<CancellationToken>())).Returns(Task.FromResult(response.Object));
            configClient.Setup(_ => _.GetConfigurationSettingAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(response.Object));
            configClient.Setup(_ => _.SetConfigurationSettingAsync(_featureFlagPrefix + appName.ToLowerInvariant(), "abc", "PPE", It.IsAny<CancellationToken>())).Returns(Task.FromResult(response.Object));
            configClient.Setup(_ => _.DeleteConfigurationSettingAsync(_featureFlagPrefix + appName.ToLowerInvariant(), "PPE", It.IsAny<CancellationToken>())).Returns(Task.FromResult(response1.Object));
            configClient.Setup(_ => _.SetConfigurationSetting(_featureFlagPrefix + appName.ToLowerInvariant(), "abc", "PPE", It.IsAny<CancellationToken>())).Returns(response.Object);
            configClient.Setup(_ => _.GetConfigurationSettingsAsync(It.IsAny<SettingSelector>(), It.IsAny<CancellationToken>())).Returns(mockAsyncConfig.Object);

            if (setError)
                configClientProvider.Setup(_ => _.GetConfigurationClient()).Throws(new RequestFailedException("Testing RequestFailed Exception"));
            else
                configClientProvider.Setup(_ => _.GetConfigurationClient()).Returns(configClient.Object);
        }
        #endregion

        #region TestMethods
        [TestMethod]
        public async Task CreateFeatureFlag_Must_Return_True_When_Valid_Data_Is_Sent()
        {
            await featureFlagManager.CreateFeatureFlag(appName, envName, featureFlag, loggerTrackingIds);
        }

        [TestMethod]
        [ExpectedException(typeof(DomainException))]
        public async Task CreateFeatureFlag_Must_Return_DomainException_With_GeneralException()
        {
            featureFlag = null;
            await featureFlagManager.CreateFeatureFlag(appName, envName, featureFlag, loggerTrackingIds);
        }

        [TestMethod]
        [ExpectedException(typeof(AzureRequestException))]
        public async Task CreateFeatureFlag_Must_Return_DomainException_With_RequestFailedException()
        {
            SetConfigClientProvider(str, featureFlag, true);
            featureFlagManager = new FeatureFlagManager(config.Object, configClientProvider.Object, mockCacheFactory.Object, logger.Object);
            await featureFlagManager.CreateFeatureFlag(appName, envName, featureFlag, loggerTrackingIds);
        }

        [TestMethod]
        public async Task UpdateFeatureFlag_Must_Return_True_When_Valid_Data_Is_Sent()
        {
            await featureFlagManager.UpdateFeatureFlag(appName, envName, featureFlag, loggerTrackingIds);
        }

        [TestMethod]
        [ExpectedException(typeof(DomainException))]
        public async Task UpdateFeatureFlag_Must_Return_DomainException_With_GeneralException()
        {
            featureFlag = null;
            await featureFlagManager.UpdateFeatureFlag(appName, envName, featureFlag, loggerTrackingIds);
        }

        [TestMethod]
        [ExpectedException(typeof(AzureRequestException))]
        public async Task UpdateFeatureFlag_Must_Return_DomainException_With_RequestFailedException()
        {
            SetConfigClientProvider(str, featureFlag, true);
            featureFlagManager = new FeatureFlagManager(config.Object, configClientProvider.Object, mockCacheFactory.Object, logger.Object);
            await featureFlagManager.UpdateFeatureFlag(appName, envName, featureFlag, loggerTrackingIds);
        }

        [TestMethod]
        public async Task GetFeatureFlag_Must_Return_The_Correct_Data()
        {
            var res = await featureFlagManager.GetFeatureFlag(appName, envName, id, loggerTrackingIds);
            Assert.IsNotNull(res);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetFeatureFlag_Must_Return_Exception()
        {
            Mock<ConfigurationClient> configClient = new Mock<ConfigurationClient>();
            Mock<Response<ConfigurationSetting>> response = new Mock<Response<ConfigurationSetting>>();
            response.Setup(_ => _.Value).Throws(new ArgumentNullException());
            configClient.Setup(_ => _.GetConfigurationSettingAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(response.Object));
            configClientProvider.Setup(_ => _.GetConfigurationClient()).Returns(configClient.Object);
            featureFlagManager = new FeatureFlagManager(config.Object, configClientProvider.Object, mockCacheFactory.Object, logger.Object);

            await featureFlagManager.GetFeatureFlag(appName, envName, id, loggerTrackingIds);
        }

        [TestMethod]
        public async Task GetFeatureFlag_Must_Return_RequestFailedException()
        {
            SetConfigClientProvider(str, featureFlag, true);
            var res = await featureFlagManager.GetFeatureFlag(appName, envName, id, loggerTrackingIds);
            Assert.IsNull(res);
        }

        [TestMethod]
        public async Task UpdateFeatureFlagStatus_Must_Return_The_Correct_Data()
        {
            await featureFlagManager.UpdateFeatureFlagStatus(appName, envName, featureFlag, true, loggerTrackingIds);
        }

        [TestMethod]
        public async Task GetFeatureFlags_Must_Return_Right_Data()
        {
            Assert.IsNotNull(await featureFlagManager.GetFeatureFlags(appName, envName, loggerTrackingIds));
        }

        [TestMethod]
        public async Task GetFeatureFlags_Must_Return_Right_Data_With_1_Stage()
        {
            string str1 = @"
        {
            'id': 'fxp_dev_Flag1',
            'description': 'Check if Alias is a member of a given list',
            'enabled': true,
            'label': 'PPE',
            'name' : 'Flag1',
            'environment' : 'dev',
            'conditions': {
                'client_filters': [
                    {
                        'name': 'CheckIfAliasExist',
                        'parameters': {
                            'operator': 'MemberOfCustomGroup',
                            'value': 'bhchalas,prkulkar,ancha',
                            'isactive': 'true',
                            'StageId': '0',
                            'stageName' : 'Stage1'
                        }
                    }
                ]
            }
        }";
            featureFlag = JsonConvert.DeserializeObject<FeatureFlag>(str1);
            SetConfigClientProvider(str1, featureFlag, false);
            Assert.IsNotNull(await featureFlagManager.GetFeatureFlags(appName, envName, loggerTrackingIds));
        }

        [TestMethod]
        public async Task ActivateStage_Must_Return_Right_Data()
        {
            await featureFlagManager.ActivateStage(appName, envName, featureFlag, "Stage1", loggerTrackingIds);
        }

        [TestMethod]
        public async Task DeleteFlag_Must_Return_Right_Data()
        {
            await featureFlagManager.DeleteFeatureFlag(appName, envName, "Flag1", loggerTrackingIds);
        }

        [TestMethod]
        [ExpectedException(typeof(DomainException))]
        public async Task DeleteFlag_Must_Return_Exception_AzureRequest()
        {
            SetConfigClientProvider(str, featureFlag, true);
            featureFlagManager = new FeatureFlagManager(config.Object, configClientProvider.Object, mockCacheFactory.Object, logger.Object);
            await featureFlagManager.DeleteFeatureFlag(appName, "", "", loggerTrackingIds);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task DeleteFlag_Must_Return_Exception()
        {
            configClientProvider.Setup(_ => _.GetConfigurationClient()).Throws(new Exception("Testing Exception"));
            featureFlagManager = new FeatureFlagManager(config.Object, configClientProvider.Object, mockCacheFactory.Object, logger.Object);
            await featureFlagManager.DeleteFeatureFlag(appName, "", "", loggerTrackingIds);
        }

        [TestMethod]
        public void Equals_Opertor_ReturnTrue()
        {
            string str1 = @"
            {
                'name': 'CheckIfAliasExist',
                'parameters': {
                    'operator': 'MemberOfCustomGroup',
                    'value': 'bhchalas,prkulkar,ancha',
                    'isactive': 'true',
                    'StageId': '0',
                    'stageName' : 'Stage1'
                }
            }";
            string str2 = @"
{
                'name': 'CheckIfAliasExist',
                'parameters': {
                    'operator': 'MemberOfCustomGroup',
                    'value': 'bhchalas,prkulkar,ancha',
                    'isactive': 'true',
                    'StageId': '0',
                    'stageName' : 'Stage1'
                }
            }";
            Filter filter1 = JsonConvert.DeserializeObject<Filter>(str1);
            Filter filter2 = JsonConvert.DeserializeObject<Filter>(str2);
            FilterComparer filterComparer = new FilterComparer();
            Assert.IsTrue(filterComparer.Equals(filter1, filter2));
        }

        [TestMethod]
        public void Equals_Opertor_ReturnsFalse()
        {
            string str1 = @"
            {
                'name': 'CheckIfAliasExist',
                'parameters': {
                    'operator': 'MemberOfCustomGroup',
                    'value': 'bhchalas,prkulkar,ancha',
                    'isactive': 'true',
                    'StageId': '0',
                    'stageName' : 'Stage1'
                }
            }";
            string str2 = @"
            {
                'name': 'CheckIfAliasExist',
                'parameters': {
                    'operator': 'Equals',
                    'value': 'bhchalas',
                    'isactive': 'true',
                    'StageId': '1',
                    'stageName' : 'Stage2'
                }
            }";
            Filter filter1 = JsonConvert.DeserializeObject<Filter>(str1);
            Filter filter2 = JsonConvert.DeserializeObject<Filter>(str2);
            FilterComparer filterComparer = new FilterComparer();
            Assert.IsFalse(filterComparer.Equals(filter1, filter2));
        }

        [TestMethod]
        public void GetHashCode_Gets_Right_Data()
        {
            string str1 = @"
            {
                'name': 'CheckIfAliasExist',
                'parameters': {
                    'operator': 'MemberOfCustomGroup',
                    'value': 'bhchalas,prkulkar,ancha',
                    'isactive': 'true',
                    'StageId': '0',
                    'stageName' : 'Stage1'
                }
            }";
            Filter filter1 = JsonConvert.DeserializeObject<Filter>(str1);
            FilterComparer filterComparer = new FilterComparer();
            Assert.IsNotNull(filterComparer.GetHashCode(filter1));
        }

        [TestMethod]
        public void GetHashCode_Gets_Right_Data_With_No_Input()
        {
            FilterComparer filterComparer = new FilterComparer();
            Assert.AreEqual(filterComparer.GetHashCode(null), 0);
        }
        #endregion
    }
}
