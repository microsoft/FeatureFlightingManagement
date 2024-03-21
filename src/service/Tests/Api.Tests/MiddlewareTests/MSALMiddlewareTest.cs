using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Api.Middlewares;
using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.FeatureFlighting.API.Tests.MiddlewareTests
{
    [ExcludeFromCodeCoverage]
    [TestCategory("MSALMiddleware")]
    [TestClass]
    public class MSALMiddlewareTest
    {
        private Mock<IHttpContextAccessor> _mockHttpContextAccessor;

        private MSALMiddleware middleware;
        public MSALMiddlewareTest() 
        {
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["x-application"] = "test-Tenant";
            httpContext.Request.Headers["x-environment"] = "preprop";

            _mockHttpContextAccessor.Setup(h=>h.HttpContext).Returns(httpContext);

            var authProps = new AuthenticationProperties();
            authProps.StoreTokens(new List<AuthenticationToken>
{
    new AuthenticationToken{ Name = "access_token", Value = "test-jwt"}
});
            _mockHttpContextAccessor.Setup(h => h.HttpContext.AuthenticateAsync(It.IsAny<string>())).Returns(Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(), authProps, "Bearer"))));

            var requestDelegate = Mock.Of<RequestDelegate>();

            var testConfig = new Mock<IConfigurationSection>();
            testConfig.Setup(s => s.Value).Returns("preprop,prod");

            middleware = new MSALMiddleware(requestDelegate);

        }

//        [TestMethod]
//        public async Task Invoke_Success() 
//        {
//            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://test123.com");

//            var authProps = new AuthenticationProperties();
//            authProps.StoreTokens(new List<AuthenticationToken>
//{
//    new AuthenticationToken{ Name = "access_token", Value = "test-jwt"}
//});

//            var authenticationServiceMock = new Mock<IAuthenticationService>();
//            authenticationServiceMock.Setup(x => x.AuthenticateAsync(It.IsAny<HttpContext>(), It.IsAny<string>()))
//                .ReturnsAsync(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(), authProps, "Bearer")));

//            var serviceProviderMock = new Mock<IServiceProvider>();
//            serviceProviderMock
//                .Setup(s => s.GetService(typeof(IAuthenticationService)))
//                .Returns(authenticationServiceMock.Object);

//            var context = new DefaultHttpContext()
//            {
//                RequestServices = serviceProviderMock.Object
//            };

//            var result = middleware.Invoke(_mockHttpContextAccessor.Object.HttpContext).IsCompleted;
//            Assert.IsTrue(result);
//        }
    }
}
