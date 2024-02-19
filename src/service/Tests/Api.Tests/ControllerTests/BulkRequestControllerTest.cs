using Microsoft.Extensions.Configuration;
using AppInsights.EnterpriseTelemetry;
using CQRS.Mediatr.Lite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Microsoft.FeatureFlighting.API.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;
using Microsoft.FeatureFlighting.Core.Commands;

namespace Microsoft.FeatureFlighting.API.Tests.ControllerTests
{
    [ExcludeFromCodeCoverage]
    [TestCategory("BulkRequestController")]
    [TestClass]
    public class BulkRequestControllerTest
    {
        public Mock<IConfiguration> _mockConfiguration;
        public Mock<ICommandBus> _mockCommandBus;
        public Mock<ILogger> _mockogger;

        public BulkRequestController bulkRequestController;

        public BulkRequestControllerTest() 
        { 
            _mockConfiguration = new Mock<IConfiguration>();
            _mockCommandBus = new Mock<ICommandBus>();
            _mockogger = new Mock<ILogger>();

            bulkRequestController = new BulkRequestController(_mockConfiguration.Object, _mockCommandBus.Object, _mockogger.Object);
        }

        #region BulkDelete_test_case
        [TestMethod]
        public async Task BulkDelete_BadRequest_when_featureFlightSelection_is_null()
        {
           var result= await bulkRequestController.BulkDelete(null);

            var bulkDeleteResult = result as BadRequestObjectResult;

            Assert.AreEqual(bulkDeleteResult.StatusCode, StatusCodes.Status400BadRequest);
            Assert.AreEqual("No flights selected for deletion", bulkDeleteResult.Value);
        }

        [TestMethod]
        public async Task BulkDelete_BadRequest__when_featureFlightSelection_is_empty()
        {
            var result = await bulkRequestController.BulkDelete(new Dictionary<string, string> { });

            var bulkDeleteResult = result as BadRequestObjectResult;

            Assert.AreEqual(bulkDeleteResult.StatusCode, StatusCodes.Status400BadRequest);
            Assert.AreEqual("No flights selected for deletion", bulkDeleteResult.Value);
        }

        [TestMethod]
        public async Task BulkDelete_BadRequest_when_selectedFeatureNames_is_empty()
        {
            Dictionary<string, string> featureFlightSelection=getFeatureFlightSelectionData_for_badrequest();

            var result = await bulkRequestController.BulkDelete(featureFlightSelection);

            var bulkDeleteResult = result as BadRequestObjectResult;

            Assert.AreEqual(bulkDeleteResult.StatusCode, StatusCodes.Status400BadRequest);
            Assert.AreEqual("No flights selected for deletion", bulkDeleteResult.Value);
        }

        [TestMethod]
        public async Task BulkDelete_Success()
        {
            Dictionary<string, string> featureFlightSelection = getFeatureFlightSelectionData_for_success();

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["x-application"] = "test-Tenant";
            httpContext.Request.Headers["x-environment"] = "preprod";

            var testConfig = new Mock<IConfigurationSection>();
            testConfig.Setup(s => s.Value).Returns("preprod,prod");

            _mockConfiguration.Setup(c => c.GetSection("Env:Supported")).Returns(testConfig.Object);

            Command < IdCommandResult > Command = new DeleteFeatureFlightCommand("testFeature", "tesTenant", "preprod", "123", "1234", "test source");
            _mockCommandBus.Setup(c => c.Send(It.IsAny<Command<IdCommandResult>>()));
            bulkRequestController = new BulkRequestController(_mockConfiguration.Object, _mockCommandBus.Object, _mockogger.Object) {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContext
                }
            };

            var result = await bulkRequestController.BulkDelete(featureFlightSelection);
            var bulkDeleteResult = result as OkObjectResult;

            Assert.AreEqual(bulkDeleteResult.StatusCode, StatusCodes.Status200OK);
            //Assert.IsTrue(bulkDeleteResult.Value.ToString().Contains("You won't receive alers for the following flights"));
        }
        #endregion

        #region BulkDisable_test_case
        [TestMethod]
        public async Task BulkDisable_BadRequest_when_featureFlightSelection_is_null()
        {
            var result = await bulkRequestController.BulkDisable(null);

            var bulkDisableResult = result as BadRequestObjectResult;

            Assert.AreEqual(bulkDisableResult.StatusCode, StatusCodes.Status400BadRequest);
            Assert.AreEqual("No flights selected for disablement", bulkDisableResult.Value);
        }

        [TestMethod]
        public async Task BulkDisable_BadRequest__when_featureFlightSelection_is_empty()
        {
            var result = await bulkRequestController.BulkDisable(new Dictionary<string, string> { });

            var bulkDisableResult = result as BadRequestObjectResult;

            Assert.AreEqual(bulkDisableResult.StatusCode, StatusCodes.Status400BadRequest);
            Assert.AreEqual("No flights selected for disablement", bulkDisableResult.Value);
        }

        [TestMethod]
        public async Task BulkDisable_BadRequest_when_selectedFeatureNames_is_empty()
        {
            Dictionary<string, string> featureFlightSelection = getFeatureFlightSelectionData_for_badrequest();

            var result = await bulkRequestController.BulkDisable(featureFlightSelection);

            var bulkDisableResult = result as BadRequestObjectResult;

            Assert.AreEqual(bulkDisableResult.StatusCode, StatusCodes.Status400BadRequest);
            Assert.AreEqual("No flights selected for disablement", bulkDisableResult.Value);
        }

        [TestMethod]
        public async Task BulkDisable_Success()
        {
            Dictionary<string, string> featureFlightSelection = getFeatureFlightSelectionData_for_success();

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["x-application"] = "test-Tenant";
            httpContext.Request.Headers["x-environment"] = "preprod";

            var testConfig = new Mock<IConfigurationSection>();
            testConfig.Setup(s => s.Value).Returns("preprod,prod");

            _mockConfiguration.Setup(c => c.GetSection("Env:Supported")).Returns(testConfig.Object);

            Command<IdCommandResult> Command = new DisableFeatureFlightCommand("testFeature", "tesTenant", "preprod", "123", "1234", "test source");
            _mockCommandBus.Setup(c => c.Send(It.IsAny<Command<IdCommandResult>>()));
            bulkRequestController = new BulkRequestController(_mockConfiguration.Object, _mockCommandBus.Object, _mockogger.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContext
                }
            };

            var result = await bulkRequestController.BulkDisable(featureFlightSelection);
            var bulkDisableResult = result as OkObjectResult;

            Assert.AreEqual(bulkDisableResult.StatusCode, StatusCodes.Status200OK);
            //Assert.IsTrue(bulkDisableResult.Value.ToString().Contains("You won't receive alers for the following flights"));
        }
        #endregion

        #region BulkUnsubscribe_test_case
        [TestMethod]
        public async Task BulkUnsubscribe_BadRequest_when_featureFlightSelection_is_null()
        {
            var result = await bulkRequestController.BulkUnsubscribe(null);

            var bulkUnsubscribeResult = result as BadRequestObjectResult;

            Assert.AreEqual(bulkUnsubscribeResult.StatusCode, StatusCodes.Status400BadRequest);
            Assert.AreEqual("No flights selected for disablement", bulkUnsubscribeResult.Value);
        }

        [TestMethod]
        public async Task BulkUnsubscribe_BadRequest__when_featureFlightSelection_is_empty()
        {
            var result = await bulkRequestController.BulkUnsubscribe(new Dictionary<string, string> { });

            var bulkUnsubscribeResult = result as BadRequestObjectResult;

            Assert.AreEqual(bulkUnsubscribeResult.StatusCode, StatusCodes.Status400BadRequest);
            Assert.AreEqual("No flights selected for disablement", bulkUnsubscribeResult.Value);
        }

        [TestMethod]
        public async Task BulkUnsubscribe_BadRequest_when_selectedFeatureNames_is_empty()
        {
            Dictionary<string, string> featureFlightSelection = getFeatureFlightSelectionData_for_badrequest();

            var result = await bulkRequestController.BulkUnsubscribe(featureFlightSelection);

            var bulkUnsubscribeResult = result as BadRequestObjectResult;

            Assert.AreEqual(bulkUnsubscribeResult.StatusCode, StatusCodes.Status400BadRequest);
            Assert.AreEqual("No flights selected for disablement", bulkUnsubscribeResult.Value);
        }

        [TestMethod]
        public async Task BulkUnsubscribe_Success()
        {
            Dictionary<string, string> featureFlightSelection = getFeatureFlightSelectionData_for_success();

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["x-application"] = "test-Tenant";
            httpContext.Request.Headers["x-environment"] = "preprod";

            var testConfig = new Mock<IConfigurationSection>();
            testConfig.Setup(s => s.Value).Returns("preprod,prod");

            _mockConfiguration.Setup(c => c.GetSection("Env:Supported")).Returns(testConfig.Object);

            Command<IdCommandResult> Command = new UnsubscribeAlertsCommand("testFeature", "tesTenant", "preprod", "123", "1234", "test source");
            _mockCommandBus.Setup(c => c.Send(It.IsAny<Command<IdCommandResult>>()));
            bulkRequestController = new BulkRequestController(_mockConfiguration.Object, _mockCommandBus.Object, _mockogger.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContext
                }
            };

            var result = await bulkRequestController.BulkDisable(featureFlightSelection);
            var bulkUnsubscribeResult = result as OkObjectResult;

            Assert.AreEqual(bulkUnsubscribeResult.StatusCode, StatusCodes.Status200OK);
            //Assert.IsTrue(bulkUnsubscribeResult.Value.ToString().Contains("You won't receive alers for the following flights"));
        }
        #endregion


        private Dictionary<string, string> getFeatureFlightSelectionData_for_badrequest() 
        {
            Dictionary<string, string> featureFlightSelection = new Dictionary<string, string>();
            featureFlightSelection.Add("test1", "false");
            featureFlightSelection.Add("test2", "false");
            featureFlightSelection.Add("test3", "false");
            return featureFlightSelection;
        }

        private Dictionary<string, string> getFeatureFlightSelectionData_for_success()
        {
            Dictionary<string, string> featureFlightSelection = new Dictionary<string, string>();
            featureFlightSelection.Add("test1", "true");
            featureFlightSelection.Add("test2", "true");
            featureFlightSelection.Add("test3", "true");
            return featureFlightSelection;
        }
    }
}
