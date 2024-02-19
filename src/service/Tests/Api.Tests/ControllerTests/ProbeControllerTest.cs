using CQRS.Mediatr.Lite;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Api.Controllers;
using Microsoft.FeatureFlighting.API.Controllers;
using Microsoft.FeatureFlighting.Core.Commands;
using Microsoft.FeatureFlighting.Core.Spec;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.FeatureFlighting.API.Tests.ControllerTests
{
    [ExcludeFromCodeCoverage]
    [TestCategory("ProbeController")]
    [TestClass]
    public class ProbeControllerTest
    {
        public Mock<IConfiguration> _mockConfiguration;

        public ProbeController probeController;

        public ProbeControllerTest()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            probeController = new ProbeController(_mockConfiguration.Object);
        }

        [TestMethod]
        public void Ping_Success()
        {
            var result=probeController.Ping();

            var pingResult = result as OkObjectResult;

            Assert.AreEqual(pingResult.StatusCode, StatusCodes.Status200OK);
            Assert.AreEqual(pingResult.Value, "Pong");

        }
    }
}
