//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Http.Features;
//using AppInsights.EnterpriseTelemetry;
//using AppInsights.EnterpriseTelemetry.Context;
//using Microsoft.FeatureFlighting.Api.Middleware;
//using Microsoft.FeatureFlighting.Common.AppExcpetions;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Moq;
//using Newtonsoft.Json;
//using System;
//using System.Diagnostics.CodeAnalysis;
//using System.Net;
//using System.Threading.Tasks;

//namespace Microsoft.FeatureFlighting.Api.Tests.MiddlewareTests
//{
//    [ExcludeFromCodeCoverage]
//    [TestCategory("ExceptionMiddleware")]
//    [TestClass]
//    public class ExceptionMiddlewareTests
//    {
//        [TestMethod]
//        public async Task ExceptionMiddleware_ShouldBeByPasssed_WhenNoExceptionOccurs()
//        {
//            var defaultHttpContext = new DefaultHttpContext();
//            Task mockDelegate(HttpContext context)
//            {
//                return Task.CompletedTask;
//            };
//            var loggerMock = SetLoggerMock();

//            var exceptionMiddleware = new ExceptionMiddleware(mockDelegate, loggerMock.Object);
//            await exceptionMiddleware.InvokeAsync(defaultHttpContext);
//        }

//        [TestMethod]
//        public async Task ExceptionMiddleware_ShouldLogAppException_AndSetStatusCodeAs500_WhenGeneralAppExceptionIsThrown()
//        {
//            var loggerMock = SetLoggerMock();
//            var defaultHttpContext = new DefaultHttpContext();
//            var mockInnerException = new Exception("Fake Message");
//            var mockCorrelationId = Guid.NewGuid().ToString();
//            var mockTransactionId = Guid.NewGuid().ToString();
//            var mockFailedMethod = "Mock Method";
//            var mockGeneralException = new GeneralException(mockInnerException, mockCorrelationId, mockTransactionId, mockFailedMethod);
//            Task mockDelegate(HttpContext context)
//            {
//                throw mockGeneralException;
//            };

//            var exceptionMiddleware = new ExceptionMiddleware(mockDelegate, loggerMock.Object);
//            await exceptionMiddleware.InvokeAsync(defaultHttpContext);

//            Assert.AreEqual((int)HttpStatusCode.InternalServerError, defaultHttpContext.Response.StatusCode);
//            Assert.IsNotNull(defaultHttpContext.Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase);
//        }

//        [TestMethod]
//        public async Task ExceptionMiddleware_ShouldLogAppException_AndSetStatusCodeAs500_WhenGeneralAppExceptionIsThrown_With_Headers_Set()
//        {
//            var loggerMock = SetLoggerMock();
//            var defaultHttpContext = new DefaultHttpContext();
//            var mockInnerException = new Exception("Fake Message");
//            var mockCorrelationId = Guid.NewGuid().ToString();
//            var mockTransactionId = Guid.NewGuid().ToString();
//            var mockFailedMethod = "Mock Method";
//            var mockGeneralException = new GeneralException(mockInnerException, mockCorrelationId, mockTransactionId, mockFailedMethod);
//            Task mockDelegate(HttpContext context)
//            {
//                throw mockGeneralException;
//            };

//            var exceptionMiddleware = new ExceptionMiddleware(mockDelegate, loggerMock.Object);

//            ExceptionMiddleware.SetCorrelationIdHeaderKey("XCV");
//            ExceptionMiddleware.SetTransactionIdHeaderKey("TCID");

//            defaultHttpContext.Request.Headers.Add("XCV", "TXCV");
//            defaultHttpContext.Request.Headers.Add("TCID", "TTCID");

//            await exceptionMiddleware.InvokeAsync(defaultHttpContext);

//            Assert.AreEqual((int)HttpStatusCode.InternalServerError, defaultHttpContext.Response.StatusCode);
//            Assert.IsNotNull(defaultHttpContext.Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase);
//        }

//        [TestMethod]
//        public async Task ExceptionMiddleware_ShouldLogAppException_AndSetStatusCodeAs400_WhenDomainExceptionIsThrown_With_Headers_Set()
//        {
//            var loggerMock = SetLoggerMock();
//            var defaultHttpContext = new DefaultHttpContext();
//            var mockInnerException = new Exception("Fake Message");
//            var mockCorrelationId = Guid.NewGuid().ToString();
//            var mockTransactionId = Guid.NewGuid().ToString();
//            var mockFailedMethod = "Mock Method";
//            var mockGeneralException = new DomainException("TMessage", "1001", mockCorrelationId, mockTransactionId, mockFailedMethod, mockInnerException);
//            Task mockDelegate(HttpContext context)
//            {
//                throw mockGeneralException;
//            };

//            var exceptionMiddleware = new ExceptionMiddleware(mockDelegate, loggerMock.Object);

//            ExceptionMiddleware.SetCorrelationIdHeaderKey("XCV");
//            ExceptionMiddleware.SetTransactionIdHeaderKey("TCID");

//            defaultHttpContext.Request.Headers.Add("XCV", "TXCV");
//            defaultHttpContext.Request.Headers.Add("TCID", "TTCID");

//            await exceptionMiddleware.InvokeAsync(defaultHttpContext);

//            Assert.AreEqual((int)HttpStatusCode.BadRequest, defaultHttpContext.Response.StatusCode);
//        }

//        [TestMethod]
//        public async Task ExceptionMiddleware_ShouldLogAppException_AndSetStatusCodeAs500_WhenAzureRequestExceptionIsThrown_With_Headers_Set()
//        {
//            var loggerMock = SetLoggerMock();
//            var defaultHttpContext = new DefaultHttpContext();
//            var mockInnerException = new Exception("Fake Message");
//            var mockCorrelationId = Guid.NewGuid().ToString();
//            var mockTransactionId = Guid.NewGuid().ToString();
//            var mockFailedMethod = "Mock Method";
//            var mockGeneralException = new AzureRequestException("TMessage", 500, mockCorrelationId, mockTransactionId, mockFailedMethod, mockInnerException);
//            Task mockDelegate(HttpContext context)
//            {
//                throw mockGeneralException;
//            };

//            var exceptionMiddleware = new ExceptionMiddleware(mockDelegate, loggerMock.Object);

//            ExceptionMiddleware.SetCorrelationIdHeaderKey("XCV");
//            ExceptionMiddleware.SetTransactionIdHeaderKey("TCID");

//            defaultHttpContext.Request.Headers.Add("XCV", "TXCV");
//            defaultHttpContext.Request.Headers.Add("TCID", "TTCID");

//            await exceptionMiddleware.InvokeAsync(defaultHttpContext);

//            Assert.AreEqual((int)HttpStatusCode.InternalServerError, defaultHttpContext.Response.StatusCode);
//            Assert.IsNotNull(defaultHttpContext.Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase);
//        }

//        [TestMethod]
//        public async Task ExceptionMiddleware_ShouldLogAppException_AndSetStatusCodeAs500_WhenGeneralExceptionIsThrown_With_Headers_Set()
//        {
//            var loggerMock = SetLoggerMock();
//            var defaultHttpContext = new DefaultHttpContext();
//            var mockInnerException = new Exception("Fake Message");
//            var mockCorrelationId = Guid.NewGuid().ToString();
//            var mockTransactionId = Guid.NewGuid().ToString();
//            var mockGeneralException = new Exception("TMessage");
//            Task mockDelegate(HttpContext context)
//            {
//                throw mockGeneralException;
//            };

//            var exceptionMiddleware = new ExceptionMiddleware(mockDelegate, loggerMock.Object);

//            ExceptionMiddleware.SetCorrelationIdHeaderKey("XCV");
//            ExceptionMiddleware.SetTransactionIdHeaderKey("TCID");

//            await exceptionMiddleware.InvokeAsync(defaultHttpContext);

//            Assert.AreEqual((int)HttpStatusCode.InternalServerError, defaultHttpContext.Response.StatusCode);
//            Assert.IsNotNull(defaultHttpContext.Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase);
//        }

//        [TestMethod]
//        public void ExceptionMiddleware_Error_Details_Should_Be_Set_And_Returned()
//        {
//            ErrorDetails errorDetails = new ErrorDetails()
//            {
//                StatusCode = 401,
//                Message = "TMessage"
//            };

//            Assert.AreEqual(JsonConvert.SerializeObject(errorDetails), errorDetails.ToString());
//        }

//        public Mock<ILogger> SetLoggerMock()
//        {
//            Mock<ILogger> logger = new Mock<ILogger>();
//            logger.Setup(m => m.Log(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
//            logger.Setup(m => m.Log(It.IsAny<System.Exception>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
//            logger.Setup(m => m.Log(It.IsAny<ExceptionContext>()));
//            logger.Setup(m => m.Log(It.IsAny<MessageContext>()));
//            logger.Setup(m => m.Log(It.IsAny<EventContext>()));
//            logger.Setup(m => m.Log(It.IsAny<MetricContext>()));
//            return logger;
//        }
//    }
//}
