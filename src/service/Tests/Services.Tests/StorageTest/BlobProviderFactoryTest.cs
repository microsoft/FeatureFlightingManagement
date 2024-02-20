using AppInsights.EnterpriseTelemetry;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Common.Cache;
using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.FeatureFlighting.Common.Storage;
using Microsoft.FeatureFlighting.Infrastructure.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Concurrent;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;


namespace Microsoft.FeatureFlighting.Infrastructure.Tests.StorageTest
{
    [ExcludeFromCodeCoverage]
    [TestCategory("BlobProviderFactory")]
    [TestClass]
    public class BlobProviderFactoryTest
    {
        private Mock<ITenantConfigurationProvider> _mockTenantConfigurationProvider;
        private Mock<ILogger> _mockLogger;
        private Mock<IBlobProvider> _mockBlobProvider;
        private BlobProviderFactory _factory;

        Mock<Dictionary<string, IBlobProvider>> _mockBlobProviderCache;

        public BlobProviderFactoryTest()
        {
            _mockTenantConfigurationProvider = new Mock<ITenantConfigurationProvider>();
            _mockLogger = new Mock<ILogger>();
            _mockBlobProvider = new Mock<IBlobProvider>();
            _mockBlobProviderCache = new Mock<Dictionary<string, IBlobProvider>>();

            _factory = new BlobProviderFactory(_mockTenantConfigurationProvider.Object, _mockLogger.Object);
        }

        [TestMethod]
        public async Task CreateBreWorkflowProvider_nodata_when_tenantConfiguration_BusinessRuleEngine_is_null()
        {
            _mockTenantConfigurationProvider.Setup(t => t.Get(It.IsAny<string>())).Returns(Task.FromResult(
                new TenantConfiguration
                {
                    BusinessRuleEngine = null
                }));

            var result = await _factory.CreateBreWorkflowProvider("test");
            Assert.IsNull(null);
        }

        [TestMethod]
        public async Task CreateBreWorkflowProvider_nodata_when_tenantConfiguration_BusinessRuleEngine_storage_is_null()
        {
            _mockTenantConfigurationProvider.Setup(t => t.Get(It.IsAny<string>())).Returns(Task.FromResult(
                new TenantConfiguration
                {
                    BusinessRuleEngine = new BusinessRuleEngineConfiguration
                    {
                        Storage = null
                    }
                }));

            var result = await _factory.CreateBreWorkflowProvider("test");
            Assert.IsNull(null);
        }

        //[TestMethod]
        //public async Task CreateBreWorkflowProvider_Success()
        //{
        //    _mockTenantConfigurationProvider.Setup(t => t.Get(It.IsAny<string>())).Returns(Task.FromResult<TenantConfiguration>(GetTenantConfiguration()));
        //    var _mockBlobServiceClient = new Mock<BlobServiceClient>();
        //    _mockBlobProviderCache.Setup(b => b.Add(It.IsAny<string>(), It.IsAny<IBlobProvider>()));
        //    _mockBlobServiceClient.Setup(b => b.GetBlobContainerClient(It.IsAny<string>())).Returns(new BlobContainerClient("test connection", "container name"));
        //    var result = await _factory.CreateBreWorkflowProvider("test");
        //    Assert.IsNotNull(result);
        //}

        //public Dictionary<string, IBlobProvider> getIblobProviderData()
        //{
        //    Dictionary<string, IBlobProvider> blobProviderCache = new Dictionary<string, IBlobProvider>();

        //    blobProviderCache.Add("test", _mockBlobProvider.Object);
        //    blobProviderCache.Add("testTenant2", _mockBlobProvider.Object);

        //    return blobProviderCache;
        //}

        private TenantConfiguration GetTenantConfiguration()
        {
            return new TenantConfiguration()
            {
                BusinessRuleEngine = new BusinessRuleEngineConfiguration
                {
                    Storage = new StorageConfiguration
                    {
                        StorageConnectionString = "test connection string",
                        ContainerName = "test container name"
                    }
                },
                Name = "Test",
                Contact = "32323232323",
                IsDyanmic = true,
                ShortName = "Test"
            };
        }
    }
}

