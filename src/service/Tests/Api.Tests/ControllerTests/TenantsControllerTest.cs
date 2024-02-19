using AppInsights.EnterpriseTelemetry;
using CQRS.Mediatr.Lite;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.API.Controllers;
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

namespace Microsoft.FeatureFlighting.API.Tests.ControllerTests
{
    [ExcludeFromCodeCoverage]
    [TestCategory("TenantsController")]
    [TestClass]
    public class TenantsControllerTest
    {
        public Mock<IConfiguration> _mockConfiguration;
        public Mock<ILogger> _mockogger;
        public Mock<IQueryService> _mockQueryService;

        public TenantsController tenantsController;

        public TenantsControllerTest()
        {
            _mockQueryService = new Mock<IQueryService>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockogger = new Mock<ILogger>();


            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["x-application"] = "test-Tenant";
            httpContext.Request.Headers["x-environment"] = "preprop";

            var testConfig = new Mock<IConfigurationSection>();
            testConfig.Setup(s => s.Value).Returns("preprop,prod");

            _mockConfiguration.Setup(c => c.GetSection("Env:Supported")).Returns(testConfig.Object);

            tenantsController = new TenantsController(_mockQueryService.Object, _mockConfiguration.Object, _mockogger.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContext
                }
            };
        }


        [TestMethod]
        public async Task GetTenants_NotFound_when_tenants_is_null()
        {
            _mockQueryService.Setup(q => q.Query(It.IsAny<Query<IEnumerable<TenantConfiguration>>>())).Returns(Task.FromResult<IEnumerable<TenantConfiguration>>(null));
            var result = await tenantsController.GetTenants();
            var tenantsResult = result as NotFoundObjectResult;
            Assert.AreEqual(tenantsResult.StatusCode, StatusCodes.Status404NotFound);
        }

        [TestMethod]
        public async Task GetTenants_NotFound_when_tenants_is_empty()
        {
            _mockQueryService.Setup(q => q.Query(It.IsAny<Query<IEnumerable<TenantConfiguration>>>())).Returns(Task.FromResult<IEnumerable<TenantConfiguration>>(new List<TenantConfiguration>() { }));
            var result = await tenantsController.GetTenants();
            var tenantsResult = result as NotFoundObjectResult;
            Assert.AreEqual(tenantsResult.StatusCode, StatusCodes.Status404NotFound);
        }

        [TestMethod]
        public async Task GetTenants_Success()
        {
            _mockQueryService.Setup(q => q.Query(It.IsAny<Query<IEnumerable<TenantConfiguration>>>())).Returns(Task.FromResult<IEnumerable<TenantConfiguration>>(GetTenantConfigurations()));
            var result = await tenantsController.GetTenants();
            var tenantsResult = result as OkObjectResult;
            Assert.AreEqual(tenantsResult.StatusCode, StatusCodes.Status200OK);
        }

        private IEnumerable<TenantConfiguration> GetTenantConfigurations()
        {
            return new List<TenantConfiguration>
            {
                new TenantConfiguration()
                {
                    Name = "Test",
                    Contact="32323232323",
                    IsDyanmic=true,
                    ShortName="Test",
                }
            };
        }
    }
}
