using Moq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.FeatureFlighting.Common.Authorization;
using Microsoft.FeatureFlighting.Common.AppExceptions;
using Microsoft.FeatureFlighting.Infrastructure.Authorization;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.FeatureFlighting.Infrastructure.Tests.AuthorizationTest
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class AuthorizationServiceTests
    {
        private Mock<IHttpContextAccessor> httpAccessorMockWithPermissions;
        private Mock<IHttpContextAccessor> httpAccessorMockWithoutPermissions;
        private Mock<IHttpContextAccessor> httpAccessorWithSuperAdminPermissions;
        private ITenantConfigurationProvider _tenantConfigurationProvider;
        private IConfiguration _mockConfiguration;

        [TestInitialize]
        public void TestStartup()
        {
            SetMockConfig();
            httpAccessorMockWithPermissions = SetupHttpContextAccessorMock(httpAccessorMockWithPermissions, true, false);
            httpAccessorMockWithoutPermissions = SetupHttpContextAccessorMock(httpAccessorMockWithoutPermissions, false, false);
            httpAccessorWithSuperAdminPermissions = SetupHttpContextAccessorMock(httpAccessorMockWithoutPermissions, false, true);
        }


        [TestMethod]
        public void Must_Authorize_When_Claims_Are_Present()
        {
            IAuthorizationService authService = new AuthorizationService(httpAccessorMockWithPermissions.Object, _tenantConfigurationProvider, _mockConfiguration);
            authService.EnsureAuthorized("TestApp", "TestOp", "CorrId");
        }

        [TestMethod]
        public void Must_Authorize_When_Admin_Claims_Are_Present()
        {
            IAuthorizationService authService = new AuthorizationService(httpAccessorWithSuperAdminPermissions.Object, _tenantConfigurationProvider, _mockConfiguration);
            authService.EnsureAuthorized("TestApp", "TestOp", "CorrId");
        }

        [ExpectedException(typeof(AccessForbiddenException))]
        [TestMethod]
        public void Must_Authorize_When_Claims_Are_Not_Present()
        {
            IAuthorizationService authService = new AuthorizationService(httpAccessorMockWithoutPermissions.Object, _tenantConfigurationProvider, _mockConfiguration);
            authService.EnsureAuthorized("TestApp", "TestOp", "CorrId");
        }

        [TestMethod]
        public void Must_Return_Authorized_When_Having_Claims()
        {
            IAuthorizationService authService = new AuthorizationService(httpAccessorMockWithPermissions.Object, _tenantConfigurationProvider, _mockConfiguration);
            bool isAuthorized = authService.IsAuthorized("TestApp");
            Assert.IsTrue(isAuthorized);
        }

        [TestMethod]
        public void Must_Return_UnAuthorized_When_Not_Having_Claims()
        {
            IAuthorizationService authService = new AuthorizationService(httpAccessorMockWithoutPermissions.Object, _tenantConfigurationProvider, _mockConfiguration);
            bool isAuthorized = authService.IsAuthorized("TestApp");
            Assert.IsFalse(isAuthorized);
        }

        [TestMethod]
        public void Must_Augment_Claims_And_Authorize_When_Admin_App_Is_Signed_In()
        {
            var mockHttpContextAccessor = SetupHttpContextAccessorMock(null, false, false, null, null, "6f40053e-5319-40e5-a90b-6f714506d96d");
            var tenant = "GTA IV OPERATIONAL REPORTING";
            IAuthorizationService authService = new AuthorizationService(mockHttpContextAccessor.Object, _tenantConfigurationProvider, _mockConfiguration);
            authService.AugmentAdminClaims(tenant);
            bool isAuthorized = authService.IsAuthorized(tenant);
            Assert.IsTrue(isAuthorized);
        }

        [TestMethod]
        public void Must_Augment_Claims_And_Authorize_When_Admin_User_Is_Signed_In()
        {
            var mockHttpContextAccessor = SetupHttpContextAccessorMock(null, false, false, null, null, "admin@microsoft.com");
            var tenant = "GTA IV OPERATIONAL REPORTING";
            IAuthorizationService authService = new AuthorizationService(mockHttpContextAccessor.Object, _tenantConfigurationProvider, _mockConfiguration);
            authService.AugmentAdminClaims(tenant);
            bool isAuthorized = authService.IsAuthorized(tenant);
            Assert.IsTrue(isAuthorized);
        }

        [TestMethod]
        public void Must_Augment_Claims_And_UnAuthorize_When_NonAdmin_Is_Signed_In()
        {
            var mockHttpContextAccessor = SetupHttpContextAccessorMock(null, false, false, null, null, "NON_ADMIN_ID");
            var tenant = "GTA IV OPERATIONAL REPORTING";
            IAuthorizationService authService = new AuthorizationService(mockHttpContextAccessor.Object, _tenantConfigurationProvider, _mockConfiguration);
            authService.AugmentAdminClaims(tenant);
            bool isAuthorized = authService.IsAuthorized(tenant);
            Assert.IsFalse(isAuthorized);
        }

        private Mock<IHttpContextAccessor> SetupHttpContextAccessorMock(Mock<IHttpContextAccessor> httpContextAccessorMock, bool hasPermissions, bool hasAdminPermission = false, string upn = null, string name = null, string appId = null)
        {
            httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            var httpContext = new DefaultHttpContext();

            var claimsWithPermissions = new List<Claim>()
            {
                new Claim("TestApp", "manageexperimentation")
            };

            var claimsWithoutPermissions = new List<Claim>()
            {
                new Claim("TestApp", "testfeature")
            };

            var claimsWithAdminPermission = new List<Claim>()
            {
                new Claim("Experimentation", "All")
            };


            var identityWithClaims = new ClaimsIdentity(claimsWithPermissions, "TestAuthType");
            var identityWithoutClaims = new ClaimsIdentity(claimsWithoutPermissions, "TestAuthType");
            var identityWithAdminClaims = new ClaimsIdentity(claimsWithAdminPermission, "TestAuthType");

            if (hasPermissions)
            {
                httpContext.User.AddIdentity(identityWithClaims);
            }
            else
            {
                httpContext.User.AddIdentity(identityWithoutClaims);
            }

            if (hasAdminPermission)
            {
                httpContext.User.AddIdentity(identityWithAdminClaims);
            }

            if (!string.IsNullOrWhiteSpace(upn))
                (httpContext.User.Identity as ClaimsIdentity).AddClaim(new Claim("upn", upn));
            if (!string.IsNullOrWhiteSpace(name))
                (httpContext.User.Identity as ClaimsIdentity).AddClaim(new Claim(ClaimTypes.Name, name));
            if (!string.IsNullOrWhiteSpace(appId))
                (httpContext.User.Identity as ClaimsIdentity).AddClaim(new Claim("appid", appId));

            httpContextAccessorMock.Setup(_ => _.HttpContext).Returns(httpContext);

            return httpContextAccessorMock;
        }

        [DeploymentItem(@"appsettings.test.json", @"")]
        private void SetMockConfig()
        {
            TenantConfiguration mockConfiguration = TenantConfiguration.GetDefault();
            mockConfiguration.Authorization = new()
            {
                Type = "Configuration",
                Administrators = "tester-001@microsoft.com,appid-001,6f40053e-5319-40e5-a90b-6f714506d96d,admin@microsoft.com"
            };
            Mock<ITenantConfigurationProvider> mockProvider = new();
            mockProvider.Setup(provider => provider.Get(It.IsAny<string>()))
                .Returns(Task.FromResult(mockConfiguration));
            _tenantConfigurationProvider = mockProvider.Object;
            _mockConfiguration = new ConfigurationBuilder().AddJsonFile(@"appsettings.test.json").Build();
        }
    }
}
