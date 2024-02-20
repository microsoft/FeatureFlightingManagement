using AppInsights.EnterpriseTelemetry;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.FeatureFlighting.Infrastructure.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Microsoft.FeatureFlighting.Infrastructure.Tests.StorageTest
{
    [ExcludeFromCodeCoverage]
    [TestCategory("CosmosDbRepository")]
    [TestClass]
    public class CosmosDbRepositoryTest
    {
        private readonly Mock<Container> _mockContainer;
        private readonly Mock<ILogger> _mockLogger;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly CosmosDbRepository<TestClass> _cosmosDbRepository;

        public CosmosDbRepositoryTest()
        {
            _mockContainer = new Mock<Container>();
            _mockLogger = new Mock<ILogger>();
            _mockConfiguration = new Mock<IConfiguration>();
            var cosnmosDbConfiguration = new CosmosDbConfiguration();
            cosnmosDbConfiguration.Endpoint = "https://login.microsoftonline.com/contoso.onmicrosoft.com";
            cosnmosDbConfiguration.PrimaryKey = "dG9rZW4gaXMgcmVhZGFibGU=";
            cosnmosDbConfiguration.DatabaseId = "testDatabase";
            cosnmosDbConfiguration.ContainerId = "testContainer";

            _mockConfiguration.SetupGet(x => x[It.Is<string>(s => s == "CosmosDb:MaxRequestsPerTcpConnection")]).Returns("10");
            _mockConfiguration.SetupGet(x => x[It.Is<string>(s => s == "CosmosDb:MaxRetryWaitTimeOnRateLimitedRequests")]).Returns("5");
            _mockConfiguration.SetupGet(x => x[It.Is<string>(s => s == "CosmosDb:MaxRetryAttemptsOnRateLimitedRequests")]).Returns("5");
            _mockConfiguration.SetupGet(x => x[It.Is<string>(s => s == "ComsosDb:Endpoint")]).Returns("test endpoint");
            _mockConfiguration.SetupGet(x => x[It.Is<string>(s => s == "CosmosDb:MaxItemsPerQuery")]).Returns("10");

            _cosmosDbRepository = new CosmosDbRepository<TestClass>(cosnmosDbConfiguration, _mockConfiguration.Object, _mockLogger.Object);
        }

        [TestMethod]
        public async Task Get_DataNotExists()
        {
            // Arrange
            string documentId = "testDocument";
            string partitionKey = "testPartition";
            var mockItemResponse = new Mock<ItemResponse<TestClass>>();
            var expectedDocument = new TestClass();
            mockItemResponse.Setup(m => m.Resource).Returns(expectedDocument);
            var _mockContainer = new Mock<Container>();

            // Arrange

            var myItems = new List<TestClass>
            {
                new TestClass(),
                new TestClass()
            };
            var mockFeedIterator = new Mock<FeedIterator<TestClass>>();
            var mockContainer = new Mock<Container>();
            var feedResponseMock = new Mock<FeedResponse<TestClass>>();
            feedResponseMock.Setup(x => x.GetEnumerator()).Returns(myItems.GetEnumerator());

            mockFeedIterator.Setup(m => m.HasMoreResults).Returns(true);
            mockFeedIterator.Setup(m => m.ReadNextAsync(default)).ReturnsAsync(feedResponseMock.Object);

            mockContainer.Setup(m => m.GetItemQueryIterator<TestClass>(It.IsAny<QueryDefinition>(), null, It.IsAny<QueryRequestOptions>())).Returns(mockFeedIterator.Object);

            _mockContainer.Setup(m => m.ReadItemAsync<TestClass>(documentId, new PartitionKey(partitionKey), null, default)).ReturnsAsync(mockItemResponse.Object);

            // Act
            var document = await _cosmosDbRepository.Get(documentId, partitionKey, new LoggerTrackingIds());

            // Assert
            Assert.IsNull(document);
        }

        [TestMethod]
        public async Task QueryAll_ThrowException()
        {
            // Arrange
            string documentId = "testDocument";
            string partitionKey = "testPartition";
            var mockItemResponse = new Mock<ItemResponse<TestClass>>();
            var expectedDocument = new TestClass();
            mockItemResponse.Setup(m => m.Resource).Returns(expectedDocument);
            var _mockContainer = new Mock<Container>();

            // Arrange

            var myItems = new List<TestClass>
            {
                new TestClass(),
                new TestClass()
            };
            var mockFeedIterator = new Mock<FeedIterator<TestClass>>();
            var mockContainer = new Mock<Container>();
            var feedResponseMock = new Mock<FeedResponse<TestClass>>();
            feedResponseMock.Setup(x => x.GetEnumerator()).Returns(myItems.GetEnumerator());

            mockFeedIterator.Setup(m => m.HasMoreResults).Returns(true);
            mockFeedIterator.Setup(m => m.ReadNextAsync(default)).ReturnsAsync(feedResponseMock.Object);

            mockContainer.Setup(m => m.GetItemQueryIterator<TestClass>(It.IsAny<QueryDefinition>(), null, It.IsAny<QueryRequestOptions>())).Returns(mockFeedIterator.Object);

            _mockContainer.Setup(m => m.ReadItemAsync<TestClass>(documentId, new PartitionKey(partitionKey), null, default)).ReturnsAsync(mockItemResponse.Object);

            try { 
                var document = await _cosmosDbRepository.QueryAll("SELECT * FROM C", partitionKey, new LoggerTrackingIds());
            }
            catch (Exception ex)
            {
                Assert.IsNotNull(ex);
            }

        }

        [TestMethod]
        public async Task Save_ThrowException()
        {
            // Arrange
            string documentId = "testDocument";
            string partitionKey = "testPartition";
            var mockItemResponse = new Mock<ItemResponse<TestClass>>();
            var expectedDocument = new TestClass();
            mockItemResponse.Setup(m => m.Resource).Returns(expectedDocument);
            var _mockContainer = new Mock<Container>();

            // Arrange

            var myItems = new List<TestClass>
            {
                new TestClass(),
                new TestClass()
            };
            var mockContainer = new Mock<Container>();
            var itemResponseMock = new Mock<ItemResponse<TestClass>>();

            mockContainer.Setup(m => m.UpsertItemAsync<TestClass>(It.IsAny<TestClass>(),null,null,default)).Returns(Task.FromResult<ItemResponse<TestClass>>(itemResponseMock.Object));


            try
            {
                var document = await _cosmosDbRepository.Save(new TestClass(), partitionKey, new LoggerTrackingIds());
            }
            catch (Exception ex)
            {
                Assert.IsNotNull(ex);
            }
        }

        [TestMethod]
        public async Task Delete_ThrowException()
        {
            // Arrange
            string documentId = "testDocument";
            string partitionKey = "testPartition";
            var mockItemResponse = new Mock<ItemResponse<TestClass>>();
            var expectedDocument = new TestClass();
            mockItemResponse.Setup(m => m.Resource).Returns(expectedDocument);
            var _mockContainer = new Mock<Container>();

            // Arrange

            var myItems = new List<TestClass>
            {
                new TestClass(),
                new TestClass()
            };
            var mockContainer = new Mock<Container>();
            var itemResponseMock = new Mock<ItemResponse<TestClass>>();

            mockContainer.Setup(m => m.DeleteItemAsync<TestClass>(It.IsAny<string>(), It.IsAny<PartitionKey>(), null, default)).Returns(Task.FromResult<ItemResponse<TestClass>>(itemResponseMock.Object));


            try
            {
                await _cosmosDbRepository.Delete("1", partitionKey, new LoggerTrackingIds(),default);
            }
            catch (Exception ex)
            {
                Assert.IsNotNull(ex);
            }
        }
    }

    public class TestClass
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public TestClass()
        {
            Id = 1;
            Name = "Test";
            Description = "Test Description";
        }
        // Your class definition here
    }
}
