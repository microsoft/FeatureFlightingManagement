using Moq;
using System;
using System.Net;
using AppInsights.EnterpriseTelemetry;
using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;
using AppInsights.EnterpriseTelemetry.Context;
using Microsoft.FeatureFlighting.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.FeatureFlighting.Common.AppExcpetions;
using Microsoft.FeatureFlighting.Api.ExceptionHandler;

namespace Microsoft.FeatureFlighting.Api.Tests.ExceptionHandlerTests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class GenericExceptionHandlerTests
    {
        [TestMethod]
        public void GenericExeptionHandler_ShouldLogAppException_AndUpdateResponseStatusTo500()
        {
            #region Arrange
            var mockLogger = new Mock<ILogger>();
            var mockCorrelationId = Guid.NewGuid().ToString();
            var mockException = new AzureRequestException(Guid.NewGuid().ToString(), 500, mockCorrelationId, "", "Test");
            var defaultContext = new DefaultHttpContext();

            mockLogger.Setup(logger => logger.Log(It.IsAny<ExceptionContext>()));
            #endregion Arrange

            #region Act
            var handler = new GenericExceptionHandler(mockLogger.Object);
            handler.Handle(mockException, defaultContext, mockCorrelationId, Guid.NewGuid().ToString());
            #endregion Act

            #region Assert
            Assert.AreEqual((int)HttpStatusCode.InternalServerError, defaultContext.Response.StatusCode);
            mockLogger.Verify(logger => logger.Log(It.Is<ExceptionContext>(ec => ec.Exception.Message == mockException.Message)));
            #endregion Assert
        }

        [TestMethod]
        public void GenericExeptionHandler_ShouldLogAppException_AndUpdateResponseStatusTo500_WhenSystemExceptionIsThrown()
        {
            #region Arrange
            var mockLogger = new Mock<ILogger>();
            var mockCorrelationId = Guid.NewGuid().ToString();
            var mockException = new Exception(Guid.NewGuid().ToString());
            var defaultContext = new DefaultHttpContext();

            mockLogger.Setup(logger => logger.Log(It.IsAny<ExceptionContext>()));
            #endregion Arrange

            #region Act
            var handler = new GenericExceptionHandler(mockLogger.Object);
            handler.Handle(mockException, defaultContext, mockCorrelationId, Guid.NewGuid().ToString());
            #endregion Act

            #region Assert
            Assert.AreEqual((int)HttpStatusCode.InternalServerError, defaultContext.Response.StatusCode);
            mockLogger.Verify(logger => logger.Log(It.Is<ExceptionContext>(ec => ec.Exception.Message == Constants.Exception.GeneralException.ExceptionMessage)));
            #endregion Assert
        }

        [TestMethod]
        public void GenericExeptionHandler_ShouldNotHandle_WhenDomainExceptionIsThrown()
        {
            #region Arrange
            var mockLogger = new Mock<ILogger>();
            var mockCorrelationId = Guid.NewGuid().ToString();
            var mockException = new DomainException(Guid.NewGuid().ToString(), "Test");
            var defaultContext = new DefaultHttpContext();

            mockLogger.Setup(logger => logger.Log(It.IsAny<ExceptionContext>()));
            #endregion Arrange

            #region Act
            var handler = new GenericExceptionHandler(mockLogger.Object);
            handler.Handle(mockException, defaultContext, mockCorrelationId, Guid.NewGuid().ToString());
            #endregion Act

            #region Assert
            mockLogger.Verify(logger => logger.Log(It.IsAny<ExceptionContext>()), Times.Never);
            #endregion Assert
        }

        [TestMethod]
        public void GenericExeptionHandler_ShouldNotHandle_WhenAccessForbiddenExceptionIsThrown()
        {
            #region Arrange
            var mockLogger = new Mock<ILogger>();
            var mockCorrelationId = Guid.NewGuid().ToString();
            var mockException = new AccessForbiddenException(Guid.NewGuid().ToString(), "Test", mockCorrelationId);
            var defaultContext = new DefaultHttpContext();

            mockLogger.Setup(logger => logger.Log(It.IsAny<ExceptionContext>()));
            #endregion Arrange

            #region Act
            var handler = new GenericExceptionHandler(mockLogger.Object);
            handler.Handle(mockException, defaultContext, mockCorrelationId, Guid.NewGuid().ToString());
            #endregion Act

            #region Assert
            mockLogger.Verify(logger => logger.Log(It.IsAny<ExceptionContext>()), Times.Never);
            #endregion Assert
        }
    }
}
