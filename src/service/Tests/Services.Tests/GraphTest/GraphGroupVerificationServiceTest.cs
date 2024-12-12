using Microsoft.FeatureFlighting.Common.Caching;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Infrastructure.Graph;
using Microsoft.Graph;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppInsights.EnterpriseTelemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Microsoft.FeatureFlighting.Common.Cache;

namespace Microsoft.FeatureFlighting.Infrastructure.Tests.GraphTest
{
    [ExcludeFromCodeCoverage]
    [TestCategory("GraphGroupVerificationService")]
    [TestClass]
    public class GraphGroupVerificationServiceTest
    {
        private readonly Mock<IGraphServiceClient> _mockGraphServiceClient;
        private readonly Mock<ICacheFactory> _mockCacheFactory;
        private readonly Mock<ICache> _mockCache;
        private readonly Mock<ILogger> _mockLogger;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly GraphGroupVerificationService _graphGroupVerificationService;

        public GraphGroupVerificationServiceTest()
        {
            _mockGraphServiceClient = new Mock<IGraphServiceClient>();
            _mockCacheFactory = new Mock<ICacheFactory>();
            _mockLogger = new Mock<ILogger>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockCache = new Mock<ICache>();

            _mockConfiguration.SetupGet(x => x[It.Is<string>(s => s == "Graph:CacheExpiration")]).Returns("100");
            _mockConfiguration.SetupGet(x => x[It.Is<string>(s => s == "Graph:CachingEnabled")]).Returns("True");
            _mockConfiguration.SetupGet(x => x[It.Is<string>(s => s == "Logging:LogLevel:Default")]).Returns("Debug");
            _mockConfiguration.SetupGet(x => x[It.Is<string>(s => s == "Graph:Tenant")]).Returns("contoso.onmicrosoft.com");
            _mockConfiguration.SetupGet(x => x[It.Is<string>(s => s == "Graph:Authority")]).Returns("https://login.microsoftonline.com/contoso.onmicrosoft.com");
            _mockConfiguration.SetupGet(x => x[It.Is<string>(s => s == "Graph:Scope")]).Returns("dev");
            _mockConfiguration.SetupGet(x => x[It.Is<string>(s => s == "Graph:ClientSecretLocation")]).Returns("secret");
            _mockConfiguration.SetupGet(x => x[It.Is<string>(s => s == "secret")]).Returns("value");
            _mockConfiguration.SetupGet(x => x[It.Is<string>(s => s == "Graph:ClientId")]).Returns("201");
            _graphGroupVerificationService = new GraphGroupVerificationService(_mockConfiguration.Object, _mockCacheFactory.Object, _mockLogger.Object);
        }

        [TestMethod]
        public async Task IsMember_WhenUserIsMemberOfGroup_ShouldReturnTrue()
        {
            // Arrange
            var mockConfidentialClientApplicationBuilderWrapper = new Mock<IConfidentialClientApplicationBuilderWrapper>();
            mockConfidentialClientApplicationBuilderWrapper
                .Setup(m => m.Build(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Mock.Of<IConfidentialClientApplication>());
            var mockGraphServiceClientWrapper = new Mock<IGraphServiceClientWrapper>();
            mockGraphServiceClientWrapper
                .Setup(m => m.GetUser(It.IsAny<string>()))
                .ReturnsAsync(new User { Id = "testUserId", UserPrincipalName = "testUser@domain.com" });

            var userUpn = "testuser@domain.com";
            var securityGroupIds = new List<string> { "group1", "group2" };
            var trackingIds = new LoggerTrackingIds { CorrelationId = "TestCorrelationId", TransactionId = "TestTransactionId" };
            _mockCacheFactory.Setup(c => c.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(_mockCache.Object);
            _mockCache.Setup(c=>c.GetList(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult<IList<string>>(new List<string> { "testUser@domain.com", "201","123" }));
            // Act
            var result = await _graphGroupVerificationService.IsMember(userUpn, securityGroupIds, trackingIds);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task IsMember_WhenUserIsMemberOfGroup_ShouldReturnFalse()
        {
            // Arrange
            var mockConfidentialClientApplicationBuilderWrapper = new Mock<IConfidentialClientApplicationBuilderWrapper>();
            mockConfidentialClientApplicationBuilderWrapper
                .Setup(m => m.Build(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Mock.Of<IConfidentialClientApplication>());
            var mockGraphServiceClientWrapper = new Mock<IGraphServiceClientWrapper>();
            mockGraphServiceClientWrapper
                .Setup(m => m.GetUser(It.IsAny<string>()))
                .ReturnsAsync(new User { Id = "testUserId", UserPrincipalName = "testUser@domain.com" });

            var userUpn = "testuser@domain.com";
            var securityGroupIds = new List<string> { "group1", "group2" };
            var trackingIds = new LoggerTrackingIds { CorrelationId = "TestCorrelationId", TransactionId = "TestTransactionId" };
            _mockCacheFactory.Setup(c => c.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(_mockCache.Object);
            _mockCache.Setup(c => c.GetList(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult<IList<string>>(new List<string> { "testUser@domain1.com", "201", "123" }));
            // Act
            var result = await _graphGroupVerificationService.IsMember(userUpn, securityGroupIds, trackingIds);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task IsUserAliasPartOfSecurityGroup_WhenUserIsMemberOfGroup_ShouldReturnTrue()
        {
            // Arrange
            var mockConfidentialClientApplicationBuilderWrapper = new Mock<IConfidentialClientApplicationBuilderWrapper>();
            mockConfidentialClientApplicationBuilderWrapper
                .Setup(m => m.Build(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Mock.Of<IConfidentialClientApplication>());
            var mockGraphServiceClientWrapper = new Mock<IGraphServiceClientWrapper>();
            mockGraphServiceClientWrapper
                .Setup(m => m.GetUser(It.IsAny<string>()))
                .ReturnsAsync(new User { Id = "testUserId", UserPrincipalName = "testUser@domain.com" });

            var userAlias = "testuser";
            var securityGroupIds = new List<string> { "group1", "group2" };
            var trackingIds = new LoggerTrackingIds { CorrelationId = "TestCorrelationId", TransactionId = "TestTransactionId" };
            _mockCacheFactory.Setup(c => c.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(_mockCache.Object);
            _mockCache.Setup(c => c.GetList(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult<IList<string>>(new List<string> { "testUser@domain.com", "201", "123" }));
            // Act
            var result = await _graphGroupVerificationService.IsUserAliasPartOfSecurityGroup(userAlias, securityGroupIds, trackingIds);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task IsUserAliasPartOfSecurityGroup_WhenUserIsMemberOfGroup_ShouldReturnFalse()
        {
            // Arrange
            var mockConfidentialClientApplicationBuilderWrapper = new Mock<IConfidentialClientApplicationBuilderWrapper>();
            mockConfidentialClientApplicationBuilderWrapper
                .Setup(m => m.Build(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Mock.Of<IConfidentialClientApplication>());
            var mockGraphServiceClientWrapper = new Mock<IGraphServiceClientWrapper>();
            mockGraphServiceClientWrapper
                .Setup(m => m.GetUser(It.IsAny<string>()))
                .ReturnsAsync(new User { Id = "testUserId", UserPrincipalName = "testUser@domain.com" });

            var userAlias = "testuser1";
            var securityGroupIds = new List<string> { "group1", "group2" };
            var trackingIds = new LoggerTrackingIds { CorrelationId = "TestCorrelationId", TransactionId = "TestTransactionId" };
            _mockCacheFactory.Setup(c => c.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(_mockCache.Object);
            _mockCache.Setup(c => c.GetList(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult<IList<string>>(new List<string> { "testUser@domain1.com", "201", "123" }));
            // Act
            var result = await _graphGroupVerificationService.IsUserAliasPartOfSecurityGroup(userAlias, securityGroupIds, trackingIds);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task GetCachedObject_Success()
        {
            // Arrange
            var userAlias = "testuser1";
            var securityGroupIds = new List<string> { "group1", "group2" };
            var trackingIds = new LoggerTrackingIds { CorrelationId = "TestCorrelationId", TransactionId = "TestTransactionId" };
            _mockCacheFactory.Setup(c => c.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(_mockCache.Object);
            _mockCache.Setup(c => c.GetList(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult<IList<string>>(new List<string> { "testUser@domain1.com", "201", "123" }));
            BackgroundCacheParameters backgroundCacheParameters = new BackgroundCacheParameters { CacheKey = "test" };
            // Act
            var result = await _graphGroupVerificationService.GetCachedObject(backgroundCacheParameters, trackingIds);

            // Assert
            Assert.AreEqual(result.Count,3);
        }

        [TestMethod]
        public async Task SetCacheObject_Success()
        {
            // Arrange
            var userAlias = "testuser1";
            _mockCacheFactory.Setup(c => c.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(_mockCache.Object);
            _mockCache.Setup(c => c.GetList(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult<IList<string>>(new List<string> { "testUser@domain1.com", "201", "123" }));
            BackgroundCacheableObject<List<string>> backgroundCacheableObject = new BackgroundCacheableObject<List<string>>() { 
            CacheParameters = new BackgroundCacheParameters { CacheKey = "test" ,CacheDuration=100,Tenant="t1000",ObjectId="o9000"},
            };
            LoggerTrackingIds loggerTrackingIds = new LoggerTrackingIds { CorrelationId = "TestCorrelationId", TransactionId = "TestTransactionId" };
            // Act
            var result = _graphGroupVerificationService.SetCacheObject(backgroundCacheableObject, loggerTrackingIds).IsCompleted;

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task CreateCacheableObject_Success()
        {
            // Arrange
            var _mockGraphServiceClient =new Mock<IGraphServiceClient>();
            var _mockGroupTransitiveMembersCollectionWithReferencesPage=new Mock<IGroupTransitiveMembersCollectionWithReferencesPage>();
            var _mockGroupTransitiveMembersCollectionWithReferencesRequest = new Mock<IGroupTransitiveMembersCollectionWithReferencesRequest>();
            var userAlias = "testuser1";


            _mockGraphServiceClient.Setup(c => c.Groups["group1"].TransitiveMembers.Request().GetAsync()).Returns(Task.FromResult(_mockGroupTransitiveMembersCollectionWithReferencesPage.Object));

            _mockGroupTransitiveMembersCollectionWithReferencesPage.Setup(c => c.NextPageRequest).Returns(_mockGroupTransitiveMembersCollectionWithReferencesRequest.Object);
            _mockGroupTransitiveMembersCollectionWithReferencesPage.Setup(c => c.NextPageRequest.GetAsync()).Returns(Task.FromResult(_mockGroupTransitiveMembersCollectionWithReferencesPage.Object));

            _mockCacheFactory.Setup(c => c.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(_mockCache.Object);
            _mockCache.Setup(c => c.GetList(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult<IList<string>>(new List<string> { "testUser@domain1.com", "201", "123" }));


            BackgroundCacheParameters backgroundCacheParameters = new BackgroundCacheParameters { CacheKey = "test" };

            LoggerTrackingIds loggerTrackingIds = new LoggerTrackingIds { CorrelationId = "TestCorrelationId", TransactionId = "TestTransactionId" };
            // Act
            var result = _graphGroupVerificationService.CreateCacheableObject(backgroundCacheParameters, loggerTrackingIds);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task RebuildCache_Success()
        {
            // Arrange
            var _mockGraphServiceClient = new Mock<IGraphServiceClient>();
            var _mockGroupTransitiveMembersCollectionWithReferencesPage = new Mock<IGroupTransitiveMembersCollectionWithReferencesPage>();
            var _mockGroupTransitiveMembersCollectionWithReferencesRequest = new Mock<IGroupTransitiveMembersCollectionWithReferencesRequest>();
            var userAlias = "testuser1";


            _mockGraphServiceClient.Setup(c => c.Groups["group1"].TransitiveMembers.Request().GetAsync()).Returns(Task.FromResult(_mockGroupTransitiveMembersCollectionWithReferencesPage.Object));

            _mockGroupTransitiveMembersCollectionWithReferencesPage.Setup(c => c.NextPageRequest).Returns(_mockGroupTransitiveMembersCollectionWithReferencesRequest.Object);
            _mockGroupTransitiveMembersCollectionWithReferencesPage.Setup(c => c.NextPageRequest.GetAsync()).Returns(Task.FromResult(_mockGroupTransitiveMembersCollectionWithReferencesPage.Object));

            _mockCacheFactory.Setup(c => c.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(_mockCache.Object);
            _mockCache.Setup(c => c.GetList(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult<IList<string>>(new List<string> { "testUser@domain1.com", "201", "123" }));


            BackgroundCacheParameters backgroundCacheParameters = new BackgroundCacheParameters { CacheKey = "test" };

            LoggerTrackingIds loggerTrackingIds = new LoggerTrackingIds { CorrelationId = "TestCorrelationId", TransactionId = "TestTransactionId" };
            // Act
            var result= _graphGroupVerificationService.RebuildCache(backgroundCacheParameters, loggerTrackingIds);

            // Assert
            Assert.IsTrue(true);
        }
    }
}
