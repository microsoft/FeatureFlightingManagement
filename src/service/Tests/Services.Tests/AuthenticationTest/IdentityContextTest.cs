using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Common.Cache;
using Microsoft.FeatureFlighting.Infrastructure.Authentication;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.FeatureFlighting.Infrastructure.Tests.AuthenticationTest
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class IdentityContextTest
    {
        private IdentityContext Setup()
        {
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            var iConfiguration = new Mock<IConfiguration>();
            var repositoryMock = new Mock<IList<IBackgroundCacheable>>();
            return new IdentityContext(iConfiguration.Object, httpContextAccessorMock.Object);
        }

        [TestMethod]
        public void GetCurrentUserPrincipalName()
        {
            Assert.IsNull(Setup().GetCurrentUserPrincipalName());
        }

        [TestMethod]
        public void GetSignedInUserPrincipalName()
        {
            Assert.IsNull(Setup().GetSignedInUserPrincipalName());
        }
        [TestMethod]
        public void GetSignedInAppId()
        {
            Assert.IsNull(Setup().GetSignedInAppId());
        }

    }
}
