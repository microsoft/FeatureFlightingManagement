using Moq;
using System;
using System.Net;
using AppInsights.EnterpriseTelemetry;
using Microsoft.AspNetCore.Http;
using AppInsights.EnterpriseTelemetry.Context;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.FeatureFlighting.Common.AppExceptions;
using Microsoft.FeatureFlighting.Api.ExceptionHandler;

namespace Microsoft.FeatureFlighting.Api.Tests.ExceptionHandlerTests
{
    [TestClass]
    public class AccessForbiddenExceptionHandlerTests
    {
        [TestMethod]
        public void AccessForbiddenExceptionHandler_ShouldLogAndUpdateResponseCodeTo403()
        {
            #region Arrange
            var mockLogger = new Mock<ILogger>();
            var mockPartner = Guid.NewGuid().ToString();
            var mockOperation = Guid.NewGuid().ToString();
            var mockCorrelationId = Guid.NewGuid().ToString();
            var mockException = new AccessForbiddenException(mockPartner, mockOperation, mockCorrelationId);
            var defaultContext = new DefaultHttpContext();

            mockLogger.Setup(logger => logger.Log(It.IsAny<ExceptionContext>()));
            mockLogger.Setup(logger => logger.Log(It.IsAny<MetricContext>()));
            #endregion Arrange

            #region Act
            var handler = new AccessForbiddenExceptionHandler(mockLogger.Object);
            handler.Handle(mockException, defaultContext, mockCorrelationId, Guid.NewGuid().ToString());
            #endregion Act

            #region Assert
            Assert.AreEqual((int)HttpStatusCode.Forbidden, defaultContext.Response.StatusCode);
            mockLogger.Verify(logger => logger.Log(It.Is<ExceptionContext>(ec => ec.Exception.Message == mockException.Message)));
            mockLogger.Verify(logger => logger.Log(It.Is<MetricContext>(mc => mc.MetricName == "AccessForbidden")));
            #endregion Assert
        }

        [TestMethod]
        public void AccessForbiddenExceptionHandler_ShouldNotHandle_WhenExceptionIsNotAccessForbidden()
        {
            #region Arrange
            var mockLogger = new Mock<ILogger>();
            var mockPartner = Guid.NewGuid().ToString();
            var mockOperation = Guid.NewGuid().ToString();
            var mockCorrelationId = Guid.NewGuid().ToString();
            var mockException = new DomainException("", "");
            var defaultContext = new DefaultHttpContext();

            mockLogger.Setup(logger => logger.Log(It.IsAny<ExceptionContext>()));
            mockLogger.Setup(logger => logger.Log(It.IsAny<MetricContext>()));
            #endregion Arrange

            #region Act
            var handler = new AccessForbiddenExceptionHandler(mockLogger.Object);
            handler.Handle(mockException, defaultContext, mockCorrelationId, Guid.NewGuid().ToString());
            #endregion Act

            #region Assert
            mockLogger.Verify(logger => logger.Log(It.IsAny<ExceptionContext>()), Times.Never);
            mockLogger.Verify(logger => logger.Log(It.Is<MetricContext>(mc => mc.MetricName == "AccessForbidden")), Times.Never);
            #endregion Assert
        }
    }
}
