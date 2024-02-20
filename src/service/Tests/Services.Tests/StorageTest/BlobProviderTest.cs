using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Infrastructure.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppInsights.EnterpriseTelemetry;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure;
using Azure;

namespace Microsoft.FeatureFlighting.Infrastructure.Tests.StorageTest
{
    [ExcludeFromCodeCoverage]
    [TestCategory("BlobProvider")]
    [TestClass]
    public class BlobProviderTest
    {
        private readonly Mock<BlobContainerClient> _mockBlobContainerClient;
        private readonly Mock<ILogger> _mockLogger;
        private readonly BlobProvider _blobProvider;

        public BlobProviderTest()
        {
            _mockBlobContainerClient = new Mock<BlobContainerClient>();
            _mockLogger = new Mock<ILogger>();
            _blobProvider = new BlobProvider(_mockBlobContainerClient.Object, _mockLogger.Object);
        }

        [TestMethod]
        public async Task Get_ShouldReturnContent_WhenBlobExists()
        {
            // Arrange
            string blobName = "testBlob";
            string expectedContent = "testContent";
            var mockBlobClient = new Mock<BlobClient>();
            var mockResponse = new Mock<Response<bool>>();
            var mresponseResult = true;
            var mockBlobDownloadResult = BlobsModelFactory.BlobDownloadResult(content: new BinaryData("mock content"),
            details: new BlobDownloadDetails());
            mockResponse.Setup(m => m.Value).Returns(mresponseResult);

            mockBlobClient.Setup(m => m.ExistsAsync(default)).Returns(Task.FromResult<Response<bool>>(mockResponse.Object));
            mockBlobClient.Setup(m => m.DownloadContentAsync()).ReturnsAsync(Response.FromValue(mockBlobDownloadResult, Mock.Of<Response>()));
            _mockBlobContainerClient.Setup(m => m.GetBlobClient(blobName)).Returns(mockBlobClient.Object);

            // Act
            string? content = await _blobProvider.Get(blobName, new LoggerTrackingIds());

            // Assert
            Assert.IsNotNull(content);
        }

        [TestMethod]
        public async Task Get_ShouldReturnNull_WhenBlobDoesNotExist()
        {
            // Arrange
            string blobName = "testBlob";
            string expectedContent = "testContent";
            var mockBlobClient = new Mock<BlobClient>();
            var mockResponse = new Mock<Response<bool>>();
            var mresponseResult = false;
            var mockBlobDownloadResult = BlobsModelFactory.BlobDownloadResult(content: new BinaryData("mock content"),
            details: new BlobDownloadDetails());
            mockResponse.Setup(m => m.Value).Returns(mresponseResult);

            mockBlobClient.Setup(m => m.ExistsAsync(default)).Returns(Task.FromResult<Response<bool>>(mockResponse.Object));
            //mockBlobClient.Setup(m => m.DownloadContentAsync()).ReturnsAsync(Response.FromValue(mockBlobDownloadResult, Mock.Of<Response>()));
            _mockBlobContainerClient.Setup(m => m.GetBlobClient(blobName)).Returns(mockBlobClient.Object);

            // Act
            string? content = await _blobProvider.Get(blobName, new LoggerTrackingIds());

            // Assert
            Assert.IsNull(content);
        }
    }
}
