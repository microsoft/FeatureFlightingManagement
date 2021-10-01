using Moq;
using System;
using System.Net;
using AppInsights.EnterpriseTelemetry;
using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;
using AppInsights.EnterpriseTelemetry.Context;
using Microsoft.PS.FlightingService.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.PS.FlightingService.Common.AppExcpetions;
using Microsoft.PS.FlightingService.Api.ExceptionHandler;

namespace Microsoft.PS.FlightingService.Api.Tests.ExceptionHandlerTests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class DomainExceptionHandlerTests
    {
        [TestMethod]
        public void DomainExceptionHandler_ShouldLog_AndChangeStatusCodeTo400()
        {
            #region Arrange
            var mockLogger = new Mock<ILogger>();
            var mockCorrelationId = Guid.NewGuid().ToString();
            var mockException = new DomainException(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            var defaultContext = new DefaultHttpContext();

            mockLogger.Setup(logger => logger.Log(It.IsAny<ExceptionContext>()));
            #endregion Arrange

            #region Act
            var handler = new DomainExceptionHandler(mockLogger.Object);
            handler.Handle(mockException, defaultContext, mockCorrelationId, Guid.NewGuid().ToString());
            #endregion Act

            #region Assert
            Assert.AreEqual((int)HttpStatusCode.BadRequest, defaultContext.Response.StatusCode);
            mockLogger.Verify(logger => logger.Log(It.Is<ExceptionContext>(ec => ec.Exception.Message == mockException.Message)));
            #endregion Assert
        }

        [TestMethod]
        public void DomainExceptionHandler_ShouldLog_AndChangeStatusCodeTo404_WhenFeatureFlagIsNotFound()
        {
            #region Arrange
            var mockLogger = new Mock<ILogger>();
            var mockCorrelationId = Guid.NewGuid().ToString();
            var mockException = new DomainException(Guid.NewGuid().ToString(), exceptionCode: Constants.Exception.DomainException.FlagDoesntExist.ExceptionCode);
            var defaultContext = new DefaultHttpContext();

            mockLogger.Setup(logger => logger.Log(It.IsAny<ExceptionContext>()));
            #endregion Arrange

            #region Act
            var handler = new DomainExceptionHandler(mockLogger.Object);
            handler.Handle(mockException, defaultContext, mockCorrelationId, Guid.NewGuid().ToString());
            #endregion Act

            #region Assert
            Assert.AreEqual((int)HttpStatusCode.NotFound, defaultContext.Response.StatusCode);
            mockLogger.Verify(logger => logger.Log(It.Is<ExceptionContext>(ec => ec.Exception.Message == mockException.Message)));
            #endregion Assert
        }

        [TestMethod]
        public void DomainExceptionHandler_ShouldNotHandle_WhenExceptionTypeIsNotDomain()
        {
            #region Arrange
            var mockLogger = new Mock<ILogger>();
            var mockCorrelationId = Guid.NewGuid().ToString();
            var mockException = new Exception(Guid.NewGuid().ToString());
            var defaultContext = new DefaultHttpContext();

            mockLogger.Setup(logger => logger.Log(It.IsAny<ExceptionContext>()));
            #endregion Arrange

            #region Act
            var handler = new DomainExceptionHandler(mockLogger.Object);
            handler.Handle(mockException, defaultContext, mockCorrelationId, Guid.NewGuid().ToString());
            #endregion Act

            #region Assert
            mockLogger.Verify(logger => logger.Log(It.IsAny<ExceptionContext>()), Times.Never);
            #endregion Assert
        }
    }
}
