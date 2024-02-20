using AppInsights.EnterpriseTelemetry;
using CQRS.Mediatr.Lite;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.API.Controllers;
using Microsoft.FeatureFlighting.Common.Model;
using Microsoft.FeatureFlighting.Common.Model.AzureAppConfig;
using Microsoft.FeatureFlighting.Core.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Microsoft.FeatureFlighting.API.Tests.ControllerTests
{
    [ExcludeFromCodeCoverage]
    [TestCategory("FeatureFlagsAdminController")]
    [TestClass]
    public class FeatureFlagsAdminControllerTest
    {
        public Mock<IConfiguration> _mockConfiguration;
        public Mock<ICommandBus> _mockCommandBus;
        public Mock<ILogger> _mockogger;
        public Mock<IQueryService> _mockQueryService;

        public Mock<FeatureFlagsAdminController> _mockFeatureFlagsAdminController;

        public FeatureFlagsAdminController featureFlagsAdminController;

        public FeatureFlagsAdminControllerTest()
        {
            _mockQueryService = new Mock<IQueryService>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockCommandBus = new Mock<ICommandBus>();
            _mockogger = new Mock<ILogger>();


            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["x-application"] = "test-Tenant";
            httpContext.Request.Headers["x-environment"] = "preprop";

            var testConfig = new Mock<IConfigurationSection>();
            testConfig.Setup(s => s.Value).Returns("preprop,prod");

            _mockConfiguration.Setup(c => c.GetSection("Env:Supported")).Returns(testConfig.Object);

            Command<IdCommandResult> Command = new UnsubscribeAlertsCommand("testFeature", "tesTenant", "preprop", "123", "1234", "test source");
            _mockCommandBus.Setup(c => c.Send(It.IsAny<Command<IdCommandResult>>()));
            featureFlagsAdminController = new FeatureFlagsAdminController(_mockQueryService.Object, _mockCommandBus.Object, _mockConfiguration.Object, _mockogger.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContext
                }
            };
        }
        
        [TestMethod]
        public async Task GetAzureFeatureFlag_NotFound() 
        {
            _mockQueryService.Setup(a => a.Query(It.IsAny<Query<AzureFeatureFlag>>())).Returns(Task.FromResult<AzureFeatureFlag>(null));
            var result=await featureFlagsAdminController.GetAzureFeatureFlag("testfeature name");

            var azureFeatureFlagResult=result as NotFoundObjectResult;

            Assert.AreEqual(azureFeatureFlagResult.StatusCode,StatusCodes.Status404NotFound);
        }

        [TestMethod]
        public async Task GetAzureFeatureFlag_Success()
        {
            _mockQueryService.Setup(query => query.Query(It.IsAny<Query<AzureFeatureFlag>>())).Returns(Task.FromResult<AzureFeatureFlag>(GetAzureFeatureFlag()));
            var result = await featureFlagsAdminController.GetAzureFeatureFlag("testfeature name");

            var azureFeatureFlagResult = result as OkObjectResult;

            Assert.AreEqual(azureFeatureFlagResult.StatusCode, StatusCodes.Status200OK);
        }

        [TestMethod]
        public async Task GetFeatureFlights_NotFound()
        {
            _mockQueryService.Setup(a => a.Query(It.IsAny<Query<IEnumerable<FeatureFlightDto>>>())).Returns(Task.FromResult<IEnumerable<FeatureFlightDto>>(null));
            var result = await featureFlagsAdminController.GetFeatureFlights();

            var azureFeatureFlagResult = result as NotFoundObjectResult;

            Assert.AreEqual(azureFeatureFlagResult.StatusCode, StatusCodes.Status404NotFound);
        }

        [TestMethod]
        public async Task GetFeatureFlights_Success()
        {
            _mockQueryService.Setup(a => a.Query(It.IsAny<Query<IEnumerable<FeatureFlightDto>>>())).Returns(Task.FromResult<IEnumerable<FeatureFlightDto>>(GetFeatureFlightDtos()));
            var result = await featureFlagsAdminController.GetFeatureFlights();

            var azureFeatureFlagResult = result as OkObjectResult;

            Assert.AreEqual(azureFeatureFlagResult.StatusCode, StatusCodes.Status200OK);
        }

        [TestMethod]
        public async Task CreateFeatureFlag_Success()
        {
            var featureFlag = GetAzureFeatureFlag();
            _mockCommandBus.Setup(a => a.Send(It.IsAny<CreateFeatureFlightCommand>()));
            var result = await featureFlagsAdminController.CreateFeatureFlag(featureFlag);

            var azureFeatureFlagResult = result as CreatedAtActionResult;

            Assert.AreEqual(azureFeatureFlagResult.StatusCode, StatusCodes.Status201Created);
        }

        [TestMethod]
        public async Task UpdateFeatureFlag_Success()
        {
            var featureFlag = GetAzureFeatureFlag();
            _mockCommandBus.Setup(a => a.Send(It.IsAny<UpdateFeatureFlightCommand>()));
            var result = await featureFlagsAdminController.UpdateFeatureFlag(featureFlag);

            var azureFeatureFlagResult = result as NoContentResult;

            Assert.AreEqual(azureFeatureFlagResult.StatusCode, StatusCodes.Status204NoContent);
        }

        [TestMethod]
        public async Task EnableFeatureFlag_Success()
        {
            _mockCommandBus.Setup(a => a.Send(It.IsAny<EnableFeatureFlightCommand>()));
            var result = await featureFlagsAdminController.EnableFeatureFlag("testfeature name");

            var azureFeatureFlagResult = result as NoContentResult;

            Assert.AreEqual(azureFeatureFlagResult.StatusCode, StatusCodes.Status204NoContent);
        }

        [TestMethod]
        public async Task DisableFeatureFlag_Success()
        {
            _mockCommandBus.Setup(a => a.Send(It.IsAny<DisableFeatureFlightCommand>()));
            var result = await featureFlagsAdminController.DisableFeatureFlag("testfeature name");

            var azureFeatureFlagResult = result as NoContentResult;

            Assert.AreEqual(azureFeatureFlagResult.StatusCode, StatusCodes.Status204NoContent);
        }

        [TestMethod]
        public async Task ActivateStage_Success()
        {
            _mockCommandBus.Setup(a => a.Send(It.IsAny<ActivateStageCommand>()));
            var result = await featureFlagsAdminController.ActivateStage("testfeature name","test stage name");

            var azureFeatureFlagResult = result as NoContentResult;

            Assert.AreEqual(azureFeatureFlagResult.StatusCode, StatusCodes.Status204NoContent);
        }

        [TestMethod]
        public async Task DeleteFeatureFlag_Success()
        {
            _mockCommandBus.Setup(a => a.Send(It.IsAny<DeleteFeatureFlightCommand>()));
            var result = await featureFlagsAdminController.DeleteFeatureFlag("testfeature name");

            var azureFeatureFlagResult = result as NoContentResult;

            Assert.AreEqual(azureFeatureFlagResult.StatusCode, StatusCodes.Status204NoContent);
        }


        [TestMethod]
        public async Task RebuildFlags_Success()
        {
            _mockCommandBus.Setup(a => a.Send(It.IsAny<RebuildFlightsCommand>())).Returns(Task.FromResult(GetRebuildCommandResult()));
            var result = await featureFlagsAdminController.RebuildFlags("test reason", new string[] { "feature1", "feature2" });

            var azureFeatureFlagResult = result as OkObjectResult;

            Assert.AreEqual(azureFeatureFlagResult.StatusCode, StatusCodes.Status200OK);
        }

        [TestMethod]
        public async Task SubscribeToAlerts_Success()
        {
            _mockCommandBus.Setup(a => a.Send(It.IsAny<SubscribeAlertsCommand>()));
            var result = await featureFlagsAdminController.SubscribeToAlerts("testfeature name");

            var azureFeatureFlagResult = result as NoContentResult;

            Assert.AreEqual(azureFeatureFlagResult.StatusCode, StatusCodes.Status204NoContent);
        }

        [TestMethod]
        public async Task UnsubscribeFromAlerts_Success()
        {
            _mockCommandBus.Setup(a => a.Send(It.IsAny<UnsubscribeAlertsCommand>()));
            var result = await featureFlagsAdminController.UnsubscribeFromAlerts("testfeature name");

            var azureFeatureFlagResult = result as NoContentResult;

            Assert.AreEqual(azureFeatureFlagResult.StatusCode, StatusCodes.Status204NoContent);
        }

        [TestMethod]
        public async Task UpdateEvaluationMetrics_Success()
        {
            _mockCommandBus.Setup(a => a.Send(It.IsAny<UpdateMetricsCommand>())).Returns(Task.FromResult(GetMetricsCommandResult()));
            var result = await featureFlagsAdminController.UpdateEvaluationMetrics("testfeature name");

            var azureFeatureFlagResult = result as OkObjectResult;

            Assert.AreEqual(azureFeatureFlagResult.StatusCode, StatusCodes.Status200OK);
        }

        private AzureFeatureFlag GetAzureFeatureFlag()
        {
            return new AzureFeatureFlag
            {
                Id = Guid.NewGuid().ToString(),
                Conditions = new AzureFilterCollection()
                {
                    Client_Filters = new AzureFilter[] { new AzureFilter {
                        Name="name",Parameters=new AzureFilterParameters{ StageId="1",StageName="test stage",FlightContextKey="test key",IsActive="true",Operator="equal",Value="Jhon"} } }
                },
                Description = "Description",
                Enabled = true,
                Environment = "preprod",
                IncrementalRingsEnabled = true,
                IsFlagOptimized = true,
                Label = "test label",
                LastModifiedOn = DateTime.UtcNow,
                Name = "name",
                Version = "1.0",
                Optimizations = new List<string> { "test opt" },
                Tenant = "test tenant"
            };
        }


        private IEnumerable<FeatureFlightDto> GetFeatureFlightDtos()
        {
            return new List<FeatureFlightDto>() {
            new FeatureFlightDto {
                Id = Guid.NewGuid().ToString(),
                Description = "Description",
                Enabled = true,
                Environment = "preprod",
                Name = "name",
                Version = "1.0",
                Optimizations = new List<string> { "test opt" },
                Tenant = "test tenant",
                Stages = new List<StageDto> { new StageDto { StageId=1,StageName="test"} },
                Audit =new AuditDto{ EnabledOn=DateTime.UtcNow},
                EvaluationMetrics =new EvaluationMetricsDto{AverageLatency=12345},
                IsAzureFlightOptimized=true,
                IsIncremental=true,
                UsageReport=new FeatureUsageReportDto{ IsNew=true}
            },
            new FeatureFlightDto {
                Id = Guid.NewGuid().ToString(),
                Description = "Description",
                Enabled = true,
                Environment = "preprod",
                Name = "name",
                Version = "1.0",
                Optimizations = new List<string> { "test opt" },
                Tenant = "test tenant",
                Stages = new List<StageDto> { new StageDto { StageId=1,StageName="test"} },
                Audit =new AuditDto{ EnabledOn=DateTime.UtcNow},
                EvaluationMetrics =new EvaluationMetricsDto{AverageLatency=12345},
                IsAzureFlightOptimized=true,
                IsIncremental=true,
                UsageReport=new FeatureUsageReportDto{ IsNew=true}
            }
            };
        }

        private RebuildCommandResult GetRebuildCommandResult()
        {
            return new RebuildCommandResult(new List<string> { "test flight" });
        }

        private MetricsCommandResult GetMetricsCommandResult() {
            return new MetricsCommandResult(new EvaluationMetricsDto { LastEvaluatedBy = "admin" }) { 
            };
        }
    }
}
