using CQRS.Mediatr.Lite;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Api.Middlewares;
using Microsoft.FeatureFlighting.API.Controllers;
using Microsoft.FeatureFlighting.Common.Authorization;
using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.FeatureFlighting.Core.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.FeatureFlighting.API.Tests.MiddlewareTests
{
    [ExcludeFromCodeCoverage]
    [TestCategory("ClaimsAugmentationMiddleware")]
    [TestClass]
    public class ClaimsAugmentationMiddlewareTest
    {
        private Mock<ITenantConfigurationProvider> _mockTenantConfigurationProvider;
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<IAuthorizationService> _mockAuthorizationService;

        private ClaimsAugmentationMiddleware claimsAugmentationMiddleware;

        public ClaimsAugmentationMiddlewareTest() 
        {
            _mockTenantConfigurationProvider = new Mock<ITenantConfigurationProvider>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockAuthorizationService=new Mock<IAuthorizationService>();

           
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["x-application"] = "test-Tenant";
            httpContext.Request.Headers["x-environment"] = "preprop";

           var requestDelegate = Mock.Of<RequestDelegate>();

            var testConfig = new Mock<IConfigurationSection>();
            testConfig.Setup(s => s.Value).Returns("preprop,prod");

            _mockConfiguration.Setup(c => c.GetSection("Env:Supported")).Returns(testConfig.Object);

            claimsAugmentationMiddleware = new ClaimsAugmentationMiddleware(requestDelegate);
        }

        [TestMethod]
        public async Task Invoke_Success()
        {
            _mockTenantConfigurationProvider.Setup(t => t.Get(It.IsAny<string>())).Returns(Task.FromResult(GetTenantConfiguration()));
            _mockTenantConfigurationProvider.Setup(t => t.Get(It.IsAny<string>())).Returns(Task.FromResult(GetTenantConfiguration()));
            _mockAuthorizationService.Setup(a => a.AugmentAdminClaims(It.IsAny<string>()));
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["x-application"] = "test-Tenant";
            httpContext.Request.Headers["x-environment"] = "preprop";
            var result = claimsAugmentationMiddleware.Invoke(httpContext, _mockAuthorizationService.Object, _mockTenantConfigurationProvider.Object).IsCompleted;

            Assert.IsTrue(result);
        }



        private TenantConfiguration GetTenantConfiguration()
        {
            return new TenantConfiguration
            {
                Contact = "test contact",
                IsDyanmic = true,
                FlightsDatabase = null,
                Authorization=new AuthorizationConfiguration { Type="1",Administrators="test,test1,test3"},

            };
        }
    }
}
