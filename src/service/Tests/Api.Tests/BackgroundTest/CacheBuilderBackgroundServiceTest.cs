using AppInsights.EnterpriseTelemetry;
using CQRS.Mediatr.Lite;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.FeatureFlighting.API.Background;
using Microsoft.FeatureFlighting.API.Controllers;
using Microsoft.FeatureFlighting.Common.Cache;
using Microsoft.FeatureFlighting.Core.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.FeatureFlighting.API.Tests.BackgroundTest
{
    [TestCategory("CacheBuilderBackgroundService")]
    [TestClass]
    public class CacheBuilderBackgroundServiceTest
    {
        public Mock<IConfiguration> _mockConfiguration;
        public Mock<IBackgroundCacheManager> _mockBackgroundCacheManager;
        public Mock<ILogger> _mockogger;

        public Mock<IHostedService> _mockHostedService;

        public CacheBuilderBackgroundService CacheBuilderBackgroundService;

        public CacheBuilderBackgroundServiceTest() {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockogger = new Mock<ILogger>();
            _mockBackgroundCacheManager = new Mock<IBackgroundCacheManager>();

            var testConfig = new Mock<IConfigurationSection>();
            testConfig.Setup(s => s.Value).Returns("preprop,prod");

            _mockConfiguration.Setup(c => c.GetSection("Env:Supported")).Returns(testConfig.Object);

            var testConfigCache = new Mock<IConfigurationSection>();
            testConfigCache.Setup(s => s.Value).Returns("true");

            _mockConfiguration.Setup(c => c.GetSection("BackgroundCache:Enabled")).Returns(testConfigCache.Object);

            var testConfigPeriod = new Mock<IConfigurationSection>();
            testConfigPeriod.Setup(s => s.Value).Returns("10");

            _mockConfiguration.Setup(c => c.GetSection("BackgroundCache:Period")).Returns(testConfigPeriod.Object);

            Command<IdCommandResult> Command = new UnsubscribeAlertsCommand("testFeature", "tesTenant", "preprop", "123", "1234", "test source");
            CacheBuilderBackgroundService = new CacheBuilderBackgroundService(_mockBackgroundCacheManager.Object, _mockogger.Object, _mockConfiguration.Object);
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task StartAsync_Success(bool _isBackgroundRefreshEnabled)
        {
            var result = CacheBuilderBackgroundService.StartAsync(default).IsCompleted;
            Assert.IsTrue(result);
        }

        [DataTestMethod]
        public async Task StopAsync_Success()
        {
            var result = CacheBuilderBackgroundService.StopAsync(default).IsCompleted;
            Assert.IsTrue(result);
        }
    }
}
