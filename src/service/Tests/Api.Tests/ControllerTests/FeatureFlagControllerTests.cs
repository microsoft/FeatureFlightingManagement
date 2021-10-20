using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using AppInsights.EnterpriseTelemetry;
using AppInsights.EnterpriseTelemetry.Context;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Common.AppExcpetions;
using Microsoft.PS.Services.FlightingService.Api.ActionFilters;
using Microsoft.PS.Services.FlightingService.Api.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.FeatureFlighting.Common.Model;
using Microsoft.FeatureFlighting.Core;
using Microsoft.FeatureFlighting.Core.Spec;
using Microsoft.FeatureFlighting.Common.Authorization;

namespace Microsoft.FeatureFlighting.Api.Tests.ControllerTests
{
    [ExcludeFromCodeCoverage]
    [TestCategory("FeatureFlagController")]
    [TestClass]
    public class FeatureFlagControllerTests
    {
        [ExpectedException(typeof(DomainException))]
        [TestMethod]
        public async Task Evaluate_Feature_Flag_Must_Validation_Failed_On_Invalid_FeatureNames()
        {
            var configMock = SetConfigurationMock("XCV", "TransactionId");
            var featureFlagManagerMock = SetFeatureFlagManagerMock(true, false);
            var featureFlagEvaluator = SetFeatureFlagEvaluatorMock();
            var authMock = SetAuthServiceMock(true);
            var httpContextMock = SetHttpAccessorMock(true, true);
            FeatureFlagsController controller = new FeatureFlagsController(featureFlagEvaluator.Object, featureFlagManagerMock.Object, authMock.Object, httpContextMock.Object, configMock);

            await controller.EvaluateFeatureFlag("TApp", "TEnv", "");
        }

        [ExpectedException(typeof(DomainException))]
        [TestMethod]
        public async Task Evaluate_Feature_Flag_Must_Validation_Failed_On_Invalid_Headers()
        {
            var configMock = SetConfigurationMock("XCV", "TransactionId");
            var featureFlagManagerMock = SetFeatureFlagManagerMock(true, false);
            var featureFlagEvaluator = SetFeatureFlagEvaluatorMock();
            var authMock = SetAuthServiceMock(true);
            var httpContextMock = SetHttpAccessorMock(true, true);
            FeatureFlagsController controller = new FeatureFlagsController(featureFlagEvaluator.Object, featureFlagManagerMock.Object, authMock.Object, httpContextMock.Object, configMock);

            await controller.EvaluateFeatureFlag("", "", "Features");
        }

        [ExpectedException(typeof(DomainException))]
        [TestMethod]
        public async Task Evaluate_Feature_Flag_Must_Validation_Failed_On_Empty_Header_Values()
        {
            var configMock = SetConfigurationMock("", "");
            var featureFlagManagerMock = SetFeatureFlagManagerMock(true, false);
            var featureFlagEvaluator = SetFeatureFlagEvaluatorMock();
            var authMock = SetAuthServiceMock(true);
            var httpContextMock = SetHttpAccessorMock(false, true);
            FeatureFlagsController controller = new FeatureFlagsController(featureFlagEvaluator.Object, featureFlagManagerMock.Object, authMock.Object, httpContextMock.Object, configMock);

            await controller.EvaluateFeatureFlag("", "", "Features");
        }

        [ExpectedException(typeof(DomainException))]
        [TestMethod]
        public async Task Evaluate_Feature_Flag_Must_Validation_Failed_On_AnyOne_Missing_Headers()
        {
            var configMock = SetConfigurationMock("XCV", "TransactionId");
            var featureFlagManagerMock = SetFeatureFlagManagerMock(true, true);
            var featureFlagEvaluator = SetFeatureFlagEvaluatorMock();
            var authMock = SetAuthServiceMock(true);
            var httpContextMock = SetHttpAccessorMock(false, true);
            var loggerMock = SetLoggerMock();
            FeatureFlagsController controller = new FeatureFlagsController(featureFlagEvaluator.Object, featureFlagManagerMock.Object, authMock.Object, httpContextMock.Object, configMock);

            var result = (await controller.EvaluateFeatureFlag("", "", "Features")) as ValidationFailedResult;
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task Evaluate_Feature_Flag_Must_Give_Feature_Flag_Status_On_Correct_Data()
        {
            var configMock = SetConfigurationMock("XCV", "TransactionId");
            var featureFlagManagerMock = SetFeatureFlagManagerMock(true, false);
            var featureFlagEvaluator = SetFeatureFlagEvaluatorMock();
            var authMock = SetAuthServiceMock(true);
            var httpContextMock = SetHttpAccessorMock(true, true);
            var loggerMock = SetLoggerMock();
            FeatureFlagsController controller = new FeatureFlagsController(featureFlagEvaluator.Object, featureFlagManagerMock.Object, authMock.Object, httpContextMock.Object, configMock);

            var result = (await controller.EvaluateFeatureFlag("TApp", "UAT", "Features")) as OkObjectResult;
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task Evaluate_All_Feature_Flag_Must_Give_All_Feature_Flag_Status_On_EmptyFeature_Data()
        {
            var configMock = SetConfigurationMock("XCV", "TransactionId");
            var featureFlagManagerMock = SetFeatureFlagManagerMock(true, true);
            var featureFlagEvaluator = SetFeatureFlagEvaluatorMock();
            var authMock = SetAuthServiceMock(true);
            var httpContextMock = SetHttpAccessorMock(true, true);
            FeatureFlagsController controller = new FeatureFlagsController(featureFlagEvaluator.Object, featureFlagManagerMock.Object, authMock.Object, httpContextMock.Object, configMock);
            var result = (await controller.EvaluateFeatureFlag("TApp", "UAT", string.Empty)) as OkObjectResult;
            Assert.IsNotNull(result);
            var dict = result.Value as Dictionary<string, bool>;
            Assert.IsNotNull(dict);
            Assert.AreEqual(dict.Count, 1);
        }

        [TestMethod]
        public async Task Evaluate_All_Feature_Flag_Must_Give_Feature_Flag_Status_On_No_Available_Data()
        {
            var configMock = SetConfigurationMock("XCV", "TransactionId");
            var featureFlagManagerMock = SetFeatureFlagManagerMock(false, false);
            var featureFlagEvaluator = SetFeatureFlagEvaluatorMock();
            var authMock = SetAuthServiceMock(true);
            var httpContextMock = SetHttpAccessorMock(true, true);
            var loggerMock = SetLoggerMock();
            FeatureFlagsController controller = new FeatureFlagsController(featureFlagEvaluator.Object, featureFlagManagerMock.Object, authMock.Object, httpContextMock.Object, configMock);
            var result = (await controller.EvaluateFeatureFlag("TApp", "UAT", string.Empty)) as OkObjectResult;
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Value, typeof(Dictionary<string, bool>));
            Assert.AreEqual((result.Value as Dictionary<string, bool>).Count, 0);
        }


        [TestMethod]
        public async Task EvaluateFeatureFlag_Backward_Must_Give_Feature_Flag_Status_On_Correct_Data()
        {
            var configMock = SetConfigurationMock("XCV", "TransactionId");
            var featureFlagManagerMock = SetFeatureFlagManagerMock(true, false);
            var featureFlagEvaluator = SetFeatureFlagEvaluatorMock();
            var authMock = SetAuthServiceMock(true);
            var httpContextMock = SetHttpAccessorMock(true, true);
            var loggerMock = SetLoggerMock();
            FeatureFlagsController controller = new FeatureFlagsController(featureFlagEvaluator.Object, featureFlagManagerMock.Object, authMock.Object, httpContextMock.Object, configMock);

            var result = (await controller.EvaluateFeatureFlag_Backward("TApp", "UAT", "Features")) as OkObjectResult;
            Assert.IsNotNull(result);
        }

        [ExpectedException(typeof(DomainException))]
        [TestMethod]
        public async Task Create_Flag_Must_Validation_Failed_On_Invalid_Headers()
        {
            var configMock = SetConfigurationMock("XCV", "TransactionId");
            var featureFlagManagerMock = SetFeatureFlagManagerMock(true, false);
            var featureFlagEvaluator = SetFeatureFlagEvaluatorMock();
            var authMock = SetAuthServiceMock(true);
            var httpContextMock = SetHttpAccessorMock(true, true);
            FeatureFlagsController controller = new FeatureFlagsController(featureFlagEvaluator.Object, featureFlagManagerMock.Object, authMock.Object, httpContextMock.Object, configMock);

            FeatureFlag flag = new FeatureFlag()
            {
                Id = "TId",
                Label = "TLabel",
                Description = "TDescription",
                Enabled = true,
                Conditions = new Condition()
            };
            await controller.CreateFeatureFlag("", "", flag);
        }

        [ExpectedException(typeof(AccessForbiddenException))]
        [TestMethod]
        public async Task Create_Flag_Must_Be_UnAuthorized_On_AppName_Mismatch()
        {
            var configMock = SetConfigurationMock("XCV", "TransactionId");
            var featureFlagManagerMock = SetFeatureFlagManagerMock(true, false);
            var featureFlagEvaluator = SetFeatureFlagEvaluatorMock();
            var authMock = SetAuthServiceMock(false);
            var httpContextMock = SetHttpAccessorMock(true, true);
            FeatureFlagsController controller = new FeatureFlagsController(featureFlagEvaluator.Object, featureFlagManagerMock.Object, authMock.Object, httpContextMock.Object, configMock);

            FeatureFlag flag = new FeatureFlag()
            {
                Id = "TId",
                Label = "TLabel",
                Description = "TDescription",
                Enabled = true,
                Conditions = new Condition()
            };
            await controller.CreateFeatureFlag("TApp", "UAT", flag);
        }

        [ExpectedException(typeof(Exception))]
        [TestMethod]
        public async Task Create_Flag_Must_Be_Failed_On_Invalid_FeatureFlag_Payload()
        {
            var configMock = SetConfigurationMock("XCV", "TransactionId");
            var featureFlagManagerMock = SetFeatureFlagManagerMock(false, false);
            var featureFlagEvaluator = SetFeatureFlagEvaluatorMock();
            var authMock = SetAuthServiceMock(true);
            var httpContextMock = SetHttpAccessorMock(true, true);
            FeatureFlagsController controller = new FeatureFlagsController(featureFlagEvaluator.Object, featureFlagManagerMock.Object, authMock.Object, httpContextMock.Object, configMock);

            FeatureFlag flag = new FeatureFlag()
            {
                Id = "TId",
                Label = "TLabel",
                Description = "TDescription",
                Enabled = true,
                Conditions = new Condition()
            };
            await controller.CreateFeatureFlag("TApp", "UAT", flag);
        }

        [TestMethod]
        public void Create_Flag_Must_Be_Success_On_Successful_Validations()
        {
            var configMock = SetConfigurationMock("XCV", "TransactionId");
            var featureFlagManagerMock = SetFeatureFlagManagerMock(true, false);
            var featureFlagEvaluator = SetFeatureFlagEvaluatorMock();
            var authMock = SetAuthServiceMock(true);
            var httpContextMock = SetHttpAccessorMock(true, true);
            var loggerMock = SetLoggerMock();
            FeatureFlagsController controller = new FeatureFlagsController(featureFlagEvaluator.Object, featureFlagManagerMock.Object, authMock.Object, httpContextMock.Object, configMock);

            FeatureFlag flag = new FeatureFlag()
            {
                Id = "TId",
                Label = "TLabel",
                Description = "TDescription",
                Enabled = true,
                Conditions = new Condition()
            };
            var result = controller.CreateFeatureFlag("TApp", "UAT", flag).Result as CreatedAtActionResult;
            Assert.IsNotNull(result);
        }

        [ExpectedException(typeof(DomainException))]
        [TestMethod]
        public async Task Update_Flag_Must_Validation_Failed_On_Invalid_Headers()
        {
            var configMock = SetConfigurationMock("XCV", "TransactionId");
            var featureFlagManagerMock = SetFeatureFlagManagerMock(true, false);
            var featureFlagEvaluator = SetFeatureFlagEvaluatorMock();
            var authMock = SetAuthServiceMock(true);
            var httpContextMock = SetHttpAccessorMock(true, true);
            var loggerMock = SetLoggerMock();
            FeatureFlagsController controller = new FeatureFlagsController(featureFlagEvaluator.Object, featureFlagManagerMock.Object, authMock.Object, httpContextMock.Object, configMock);

            FeatureFlag flag = new FeatureFlag()
            {
                Id = "TId",
                Label = "TLabel",
                Description = "TDescription",
                Enabled = true,
                Conditions = new Condition()
            };
            await controller.UpdateFeatureFlag("", "", flag);
        }

        [ExpectedException(typeof(AccessForbiddenException))]
        [TestMethod]
        public async Task Update_Flag_Must_Be_UnAuthorized_On_AppName_Mismatch()
        {
            var configMock = SetConfigurationMock("XCV", "TransactionId");
            var featureFlagManagerMock = SetFeatureFlagManagerMock(true, false);
            var featureFlagEvaluator = SetFeatureFlagEvaluatorMock();
            var authMock = SetAuthServiceMock(false);
            var httpContextMock = SetHttpAccessorMock(true, true);
            var controller = new FeatureFlagsController(featureFlagEvaluator.Object, featureFlagManagerMock.Object, authMock.Object, httpContextMock.Object, configMock);

            FeatureFlag flag = new FeatureFlag()
            {
                Id = "TId",
                Label = "TLabel",
                Description = "TDescription",
                Enabled = true,
                Conditions = new Condition()
            };
            await controller.UpdateFeatureFlag("TApp", "UAT", flag);
        }

        [ExpectedException(typeof(DomainException))]
        [TestMethod]
        public async Task Update_Flag_Must_Be_Throw_Exception_When_Feature_Flag_Is_Not_Foubd()
        {
            var configMock = SetConfigurationMock("XCV", "TransactionId");
            var featureFlagManagerMock = SetFeatureFlagManagerMock(true, false);
            var featureFlagEvaluator = SetFeatureFlagEvaluatorMock();
            var authMock = SetAuthServiceMock(true);
            var httpContextMock = SetHttpAccessorMock(true, true);
            FeatureFlagsController controller = new FeatureFlagsController(featureFlagEvaluator.Object, featureFlagManagerMock.Object, authMock.Object, httpContextMock.Object, configMock);

            FeatureFlag flag = new FeatureFlag()
            {
                Id = "TId",
                Label = "TLabel",
                Description = "TDescription",
                Enabled = true,
                Conditions = new Condition()
            };
            await controller.UpdateFeatureFlag("TApp", "UAT", flag);
        }

        [ExpectedException(typeof(DomainException))]
        [TestMethod]
        public async Task Activate_Stage_Must_Fail_On_Invalid_Headers()
        {
            var configMock = SetConfigurationMock("XCV", "TransactionId");
            var featureFlagManagerMock = SetFeatureFlagManagerMock(true, false);
            var featureFlagEvaluator = SetFeatureFlagEvaluatorMock();
            var authMock = SetAuthServiceMock(true);
            var httpContextMock = SetHttpAccessorMock(true, true);
            FeatureFlagsController controller = new FeatureFlagsController(featureFlagEvaluator.Object, featureFlagManagerMock.Object, authMock.Object, httpContextMock.Object, configMock);

            var result = await controller.ActivateStage("", "", "Flag","stage1") as ValidationFailedResult;
            Assert.IsNotNull(result);
        }

        [ExpectedException(typeof(AccessForbiddenException))]
        [TestMethod]
        public async Task Activate_Stage_Flag_Must_Be_UnAuthorized_On_AppName_Mismatch()
        {
            var configMock = SetConfigurationMock("XCV", "TransactionId");
            var featureFlagManagerMock = SetFeatureFlagManagerMock(true, false);
            var featureFlagEvaluator = SetFeatureFlagEvaluatorMock();
            var authMock = SetAuthServiceMock(false);
            var httpContextMock = SetHttpAccessorMock(true, true);
            var loggerMock = SetLoggerMock();
            FeatureFlagsController controller = new FeatureFlagsController(featureFlagEvaluator.Object, featureFlagManagerMock.Object, authMock.Object, httpContextMock.Object, configMock);

            await controller.ActivateStage("TApp", "UAT", "Flag", "stage1");
        }

        [ExpectedException(typeof(DomainException))]
        [TestMethod]
        public async Task Activate_Stage_Flag_Must_return_Null_when_flag_not_found()
        {
            var configMock = SetConfigurationMock("XCV", "TransactionId");
            var featureFlagManagerMock = SetFeatureFlagManagerMock(true, false);
            var featureFlagEvaluator = SetFeatureFlagEvaluatorMock();
            var authMock = SetAuthServiceMock(true);
            var httpContextMock = SetHttpAccessorMock(true, true);
            FeatureFlagsController controller = new FeatureFlagsController(featureFlagEvaluator.Object, featureFlagManagerMock.Object, authMock.Object, httpContextMock.Object, configMock);

            await controller.ActivateStage("TApp", "UAT", "TFlag", "stage1");
        }
        [TestMethod]
        public async Task Activate_Stage_Flag_Must_return_Success_when_flag_is_correct()
        {
            var configMock = SetConfigurationMock("XCV", "TransactionId");
            var featureFlagManagerMock = SetFeatureFlagManagerMock(true, true);
            var featureFlagEvaluator = SetFeatureFlagEvaluatorMock();
            var authMock = SetAuthServiceMock(true);
            var httpContextMock = SetHttpAccessorMock(true, true);
            var loggerMock = SetLoggerMock();
            FeatureFlagsController controller = new FeatureFlagsController(featureFlagEvaluator.Object, featureFlagManagerMock.Object, authMock.Object, httpContextMock.Object, configMock);

            var result = await controller.ActivateStage("TApp", "UAT", "Flag", "stage1") as OkObjectResult;
            Assert.IsNull(result);
        }

        [ExpectedException(typeof(DomainException))]
        [TestMethod]
        public async Task  Delete_Flag_Must_Fail_On_Invalid_Headers()
        {
            var configMock = SetConfigurationMock("XCV", "TransactionId");
            var featureFlagManagerMock = SetFeatureFlagManagerMock(true, false);
            var featureFlagEvaluator = SetFeatureFlagEvaluatorMock();
            var authMock = SetAuthServiceMock(true);
            var httpContextMock = SetHttpAccessorMock(true, true);
            FeatureFlagsController controller = new FeatureFlagsController(featureFlagEvaluator.Object, featureFlagManagerMock.Object, authMock.Object, httpContextMock.Object, configMock);

            await controller.DeleteFeatureFlag("", "", "Flag");
        }

        [ExpectedException(typeof(AccessForbiddenException))]
        [TestMethod]
        public async Task Delete_Flag_Must_Be_UnAuthorized_On_AppName_Mismatch()
        {
            var configMock = SetConfigurationMock("XCV", "TransactionId");
            var featureFlagManagerMock = SetFeatureFlagManagerMock(true, false);
            var featureFlagEvaluator = SetFeatureFlagEvaluatorMock();
            var authMock = SetAuthServiceMock(false);
            var httpContextMock = SetHttpAccessorMock(true, true);
            FeatureFlagsController controller = new FeatureFlagsController(featureFlagEvaluator.Object, featureFlagManagerMock.Object, authMock.Object, httpContextMock.Object, configMock);

             await controller.DeleteFeatureFlag("TApp", "UAT", "Flag");
        }

        [TestMethod]
        public async Task Delete_Flag_Must_return_Success_when_flag_is_correct()
        {
            var configMock = SetConfigurationMock("XCV", "TransactionId");
            var featureFlagManagerMock = SetFeatureFlagManagerMock(true, true);
            var featureFlagEvaluator = SetFeatureFlagEvaluatorMock();
            var authMock = SetAuthServiceMock(true);
            var httpContextMock = SetHttpAccessorMock(true, true);
            FeatureFlagsController controller = new FeatureFlagsController(featureFlagEvaluator.Object, featureFlagManagerMock.Object, authMock.Object, httpContextMock.Object, configMock);

            var result = await controller.DeleteFeatureFlag("TApp", "UAT", "Flag") as OkObjectResult;
            Assert.IsNull(result);
        }

        [ExpectedException(typeof(DomainException))]
        [TestMethod]
        public async Task Enable_Flag_Must_Validation_Failed_On_Invalid_Headers()
        {
            var configMock = SetConfigurationMock("XCV", "TransactionId");
            var featureFlagManagerMock = SetFeatureFlagManagerMock(true, false);
            var featureFlagEvaluator = SetFeatureFlagEvaluatorMock();
            var authMock = SetAuthServiceMock(true);
            var httpContextMock = SetHttpAccessorMock(true, true);
            FeatureFlagsController controller = new FeatureFlagsController(featureFlagEvaluator.Object, featureFlagManagerMock.Object, authMock.Object, httpContextMock.Object, configMock);

            await controller.EnableFeatureFlag("", "", "TFlag");
        }

        [ExpectedException(typeof(AccessForbiddenException))]
        [TestMethod]
        public async Task Enable_Flag_Must_Be_UnAuthorized_On_AppName_Mismatch()
        {
            var configMock = SetConfigurationMock("XCV", "TransactionId");
            var featureFlagManagerMock = SetFeatureFlagManagerMock(true, false);
            var featureFlagEvaluator = SetFeatureFlagEvaluatorMock();
            var authMock = SetAuthServiceMock(false);
            var httpContextMock = SetHttpAccessorMock(true, true);
            FeatureFlagsController controller = new FeatureFlagsController(featureFlagEvaluator.Object, featureFlagManagerMock.Object, authMock.Object, httpContextMock.Object, configMock);

            await controller.EnableFeatureFlag("TApp", "UAT", "TFlag");
        }

        [ExpectedException(typeof(DomainException))]
        [TestMethod]
        public async Task Enable_Flag_Must_Be_Failed_On_Invalid_FeatureName()
        {
            var configMock = SetConfigurationMock("XCV", "TransactionId");
            var featureFlagManagerMock = SetFeatureFlagManagerMock(true, false);
            var featureFlagEvaluator = SetFeatureFlagEvaluatorMock();
            var authMock = SetAuthServiceMock(true);
            var httpContextMock = SetHttpAccessorMock(true, true);
            FeatureFlagsController controller = new FeatureFlagsController(featureFlagEvaluator.Object, featureFlagManagerMock.Object, authMock.Object, httpContextMock.Object, configMock);

            await controller.EnableFeatureFlag("TApp", "UAT", "TFlag");
        }

        [TestMethod]
        public void Enable_Flag_Must_Be_Success_On_Successful_Validations()
        {
            var configMock = SetConfigurationMock("XCV", "TransactionId");
            var featureFlagManagerMock = SetFeatureFlagManagerMock(true, true);
            var featureFlagEvaluator = SetFeatureFlagEvaluatorMock();
            var authMock = SetAuthServiceMock(true);
            var httpContextMock = SetHttpAccessorMock(true, true);
            FeatureFlagsController controller = new FeatureFlagsController(featureFlagEvaluator.Object, featureFlagManagerMock.Object, authMock.Object, httpContextMock.Object, configMock);

            var result = controller.EnableFeatureFlag("TApp", "UAT", "TFlag").Result as NoContentResult;
            Assert.IsNotNull(result);
        }

        [ExpectedException(typeof(DomainException))]
        [TestMethod]
        public async Task Disable_Flag_Must_Validation_Failed_On_Invalid_Headers()
        {
            var configMock = SetConfigurationMock("XCV", "TransactionId");
            var featureFlagManagerMock = SetFeatureFlagManagerMock(true, false);
            var featureFlagEvaluator = SetFeatureFlagEvaluatorMock();
            var authMock = SetAuthServiceMock(true);
            var httpContextMock = SetHttpAccessorMock(true, true);
            FeatureFlagsController controller = new FeatureFlagsController(featureFlagEvaluator.Object, featureFlagManagerMock.Object, authMock.Object, httpContextMock.Object, configMock);

            await controller.DisableFeatureFlag("", "", "TFlag");
        }

        [ExpectedException(typeof(AccessForbiddenException))]
        [TestMethod]
        public async Task Disable_Flag_Must_Be_UnAuthorized_On_AppName_Mismatch()
        {
            var configMock = SetConfigurationMock("XCV", "TransactionId");
            var featureFlagManagerMock = SetFeatureFlagManagerMock(true, false);
            var featureFlagEvaluator = SetFeatureFlagEvaluatorMock();
            var authMock = SetAuthServiceMock(false);
            var httpContextMock = SetHttpAccessorMock(true, true);
            FeatureFlagsController controller = new FeatureFlagsController(featureFlagEvaluator.Object, featureFlagManagerMock.Object, authMock.Object, httpContextMock.Object, configMock);

            await controller.DisableFeatureFlag("TApp", "UAT", "TFlag");
        }

        [TestMethod]
        public async Task Disable_Flag_Must_Be_Success_On_Successful_Validations()
        {
            var configMock = SetConfigurationMock("XCV", "TransactionId");
            var featureFlagManagerMock = SetFeatureFlagManagerMock(true, true);
            var featureFlagEvaluator = SetFeatureFlagEvaluatorMock();
            var authMock = SetAuthServiceMock(true);
            var httpContextMock = SetHttpAccessorMock(true, true);
            FeatureFlagsController controller = new FeatureFlagsController(featureFlagEvaluator.Object, featureFlagManagerMock.Object, authMock.Object, httpContextMock.Object, configMock);

            var result = (await controller.DisableFeatureFlag("TApp", "UAT", "TFlag")) as NoContentResult;
            Assert.IsNotNull(result);
        }

        [ExpectedException(typeof(DomainException))]
        [TestMethod]
        public async Task Get_Feature_Flag_Must_Validation_Failed_On_Invalid_Headers()
        {
            var configMock = SetConfigurationMock("XCV", "TransactionId");
            var featureFlagManagerMock = SetFeatureFlagManagerMock(true, false);
            var featureFlagEvaluator = SetFeatureFlagEvaluatorMock();
            var authMock = SetAuthServiceMock(true);
            var httpContextMock = SetHttpAccessorMock(true, true);
            FeatureFlagsController controller = new FeatureFlagsController(featureFlagEvaluator.Object, featureFlagManagerMock.Object, authMock.Object, httpContextMock.Object, configMock);

            await controller.GetFeatureFlag("", "", "TFlag");
        }

        [TestMethod]
        public void Get_Feature_Flag_Must_Be_Failed_On_Invalid_FeatureName()
        {
            var configMock = SetConfigurationMock("XCV", "TransactionId");
            var featureFlagManagerMock = SetFeatureFlagManagerMock(true, false);
            var featureFlagEvaluator = SetFeatureFlagEvaluatorMock();
            var authMock = SetAuthServiceMock(true);
            var httpContextMock = SetHttpAccessorMock(true, true);
            var loggerMock = SetLoggerMock();
            FeatureFlagsController controller = new FeatureFlagsController(featureFlagEvaluator.Object, featureFlagManagerMock.Object, authMock.Object, httpContextMock.Object, configMock);

            var result = controller.GetFeatureFlag("TApp", "UAT", "TFlag").Result as NotFoundObjectResult;
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Get_Feature_Flag_Must_Be_Success_On_Successful_Validations()
        {
            var configMock = SetConfigurationMock("XCV", "TransactionId");
            var featureFlagManagerMock = SetFeatureFlagManagerMock(true, true);
            var featureFlagEvaluator = SetFeatureFlagEvaluatorMock();
            var authMock = SetAuthServiceMock(true);
            var httpContextMock = SetHttpAccessorMock(true, true);
            var loggerMock = SetLoggerMock();
            FeatureFlagsController controller = new FeatureFlagsController(featureFlagEvaluator.Object, featureFlagManagerMock.Object, authMock.Object, httpContextMock.Object, configMock);

            var result = controller.GetFeatureFlag("TApp", "UAT", "TFlag").Result as OkObjectResult;
            Assert.IsNotNull(result);
        }

        [ExpectedException(typeof(DomainException))]
        [TestMethod]
        public async Task Get_Feature_Flags_Must_Validation_Failed_On_Invalid_Headers()
        {
            var configMock = SetConfigurationMock("XCV", "TransactionId");
            var featureFlagManagerMock = SetFeatureFlagManagerMock(true, false);
            var featureFlagEvaluator = SetFeatureFlagEvaluatorMock();
            var authMock = SetAuthServiceMock(true);
            var httpContextMock = SetHttpAccessorMock(true, true);
            var loggerMock = SetLoggerMock();
            FeatureFlagsController controller = new FeatureFlagsController(featureFlagEvaluator.Object, featureFlagManagerMock.Object, authMock.Object, httpContextMock.Object, configMock);

            await controller.GetFeatureFlags("", "UAT");
        }

        [TestMethod]
        public void Get_Feature_Flags_Must_Be_Failed_On_No_Features()
        {
            var configMock = SetConfigurationMock("XCV", "TransactionId");
            var featureFlagManagerMock = SetFeatureFlagManagerMock(true, false);
            var featureFlagEvaluator = SetFeatureFlagEvaluatorMock();
            var authMock = SetAuthServiceMock(true);
            var httpContextMock = SetHttpAccessorMock(true, true);
            var loggerMock = SetLoggerMock();
            FeatureFlagsController controller = new FeatureFlagsController(featureFlagEvaluator.Object, featureFlagManagerMock.Object, authMock.Object, httpContextMock.Object, configMock);

            var result = controller.GetFeatureFlags("TApp", "UAT").Result as NotFoundObjectResult;
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Get_Feature_Flags_Must_Be_Success_On_Successful_Validations()
        {
            var configMock = SetConfigurationMock("XCV", "TransactionId");
            var featureFlagManagerMock = SetFeatureFlagManagerMock(true, true);
            var featureFlagEvaluator = SetFeatureFlagEvaluatorMock();
            var authMock = SetAuthServiceMock(true);
            var httpContextMock = SetHttpAccessorMock(true, true);
            var loggerMock = SetLoggerMock();
            FeatureFlagsController controller = new FeatureFlagsController(featureFlagEvaluator.Object, featureFlagManagerMock.Object, authMock.Object, httpContextMock.Object, configMock);

            var result = controller.GetFeatureFlags("TApp", "UAT").Result as OkObjectResult;
            Assert.IsNotNull(result);
        }

        public IConfiguration SetConfigurationMock(string corrHeaderKey, string transHeaderKey)
        {
            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
            keyValuePairs.Add("Env:Supported", "UAT");
            keyValuePairs.Add("Application:x-correlationid", corrHeaderKey);
            keyValuePairs.Add("Application:x-messageid", transHeaderKey);

            IConfiguration configuration = new ConfigurationBuilder()
               .AddInMemoryCollection(keyValuePairs)
               .Build();

            return configuration;
        }

        public Mock<IHttpContextAccessor> SetHttpAccessorMock(bool setAllHeaders, bool setOne)
        {
            var httpContext = new DefaultHttpContext();
            //httpContext.Items.Add(Constants.Flighting.FEATURE_NAME_PARAM, "TFeatureName");
            httpContext.Items.Add(Constants.Flighting.FEATURE_ENV_PARAM, "TEnvironment");
            httpContext.Items.Add(Constants.Flighting.FEATURE_APP_PARAM, "TAppName");
            if (setAllHeaders)
            {
                httpContext.Request.Headers.Add("XCV", "TXCV");
                httpContext.Request.Headers.Add("TransactionId", "TTId");
            }
            else if (setOne)
            {
                httpContext.Request.Headers.Add("XCV", "TXCV");
            }
            Mock<IHttpContextAccessor> httpAccessorMock = new Mock<IHttpContextAccessor>();
            httpAccessorMock.Setup(m => m.HttpContext).Returns(httpContext);
            return httpAccessorMock;
        }

        public Mock<IFeatureFlagEvaluator> SetFeatureFlagEvaluatorMock()
        {
            var mockfeatureflagEvaluator = new Mock<IFeatureFlagEvaluator>();
            IDictionary<string, bool> mockResults = new Dictionary<string, bool>()
            {
                { "Flag", true }
            };
            mockfeatureflagEvaluator.Setup(m => m.Evaluate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
                .Returns(Task.FromResult(mockResults));

            return mockfeatureflagEvaluator;
        }

        public Mock<IFeatureFlagManager> SetFeatureFlagManagerMock(bool isCreateSuccess, bool isFeatureFlagFound)
        {
            FeatureFlag flag = new FeatureFlag()
            {
                Id = "TId",
                Label = "TLabel",
                Description = "TDescription",
                Name = "Flag",
                Enabled = true,
                Conditions = new Condition()
            };

            Mock<IFeatureFlagManager> featureflagManagerMock = new Mock<IFeatureFlagManager>();
            if (isCreateSuccess)
            {
                featureflagManagerMock.Setup(m => m.CreateFeatureFlag(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<FeatureFlag>(), It.IsAny<LoggerTrackingIds>())).Returns(Task.CompletedTask);
            } else
            {
                featureflagManagerMock.Setup(m => m.CreateFeatureFlag(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<FeatureFlag>(), It.IsAny<LoggerTrackingIds>())).Throws(new Exception());
            }
            
            if (isFeatureFlagFound)
            {   
                featureflagManagerMock.Setup(m => m.UpdateFeatureFlag(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<FeatureFlag>(), It.IsAny<LoggerTrackingIds>()));
                featureflagManagerMock.Setup(m => m.UpdateFeatureFlagStatus(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<FeatureFlag>(), It.IsAny<bool>(), It.IsAny<LoggerTrackingIds>()));
                featureflagManagerMock.Setup(m => m.ActivateStage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<FeatureFlag>(), It.IsAny<string>(), It.IsAny<LoggerTrackingIds>()));
                featureflagManagerMock.Setup(m => m.DeleteFeatureFlag(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<LoggerTrackingIds>()));
                IList<FeatureFlagDto> flags = new List<FeatureFlagDto>()
                {
                    new FeatureFlagDto()
                };
                featureflagManagerMock.Setup(m => m.GetFeatureFlag(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<LoggerTrackingIds>())).Returns(Task.FromResult(flag));
                featureflagManagerMock.Setup(m => m.GetFeatures(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<LoggerTrackingIds>(), It.IsAny<bool>())).Returns(Task.FromResult(flags.Select(flag => flag.Name).ToList() as IList<string>));
                featureflagManagerMock.Setup(m => m.GetFeatureFlags(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<LoggerTrackingIds>())).Returns(Task.FromResult(flags));
            }
            else
            {
                featureflagManagerMock.Setup(m => m.UpdateFeatureFlag(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<FeatureFlag>(), It.IsAny<LoggerTrackingIds>())).Throws(new DomainException("", ""));
                featureflagManagerMock.Setup(m => m.UpdateFeatureFlagStatus(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<FeatureFlag>(), It.IsAny<bool>(), It.IsAny<LoggerTrackingIds>())).Throws(new DomainException("", ""));
                featureflagManagerMock.Setup(m => m.ActivateStage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<FeatureFlag>(), It.IsAny<string>(), It.IsAny<LoggerTrackingIds>())).Throws(new DomainException("", ""));
                featureflagManagerMock.Setup(m => m.DeleteFeatureFlag(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<LoggerTrackingIds>())).Throws(new DomainException("", ""));

                featureflagManagerMock.Setup(m => m.GetFeatureFlag(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<LoggerTrackingIds>())).Returns(Task.FromResult<FeatureFlag>(null));
                featureflagManagerMock.Setup(m => m.GetFeatureFlags(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<LoggerTrackingIds>())).Returns(Task.FromResult<IList<FeatureFlagDto>>(null));
            }

            return featureflagManagerMock;
        }

        public Mock<IAuthorizationService> SetAuthServiceMock(bool isAuthorized)
        {
            Mock<IAuthorizationService> authServiceMock = new Mock<IAuthorizationService>();
            authServiceMock.Setup(m => m.IsAuthorized(It.IsAny<string>())).Returns(isAuthorized);
            if (isAuthorized)
            {
                authServiceMock.Setup(m => m.EnsureAuthorized(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
            }
            else
            {
                authServiceMock.Setup(m => m.EnsureAuthorized(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Throws(new AccessForbiddenException("", "", ""));
            }
            
            return authServiceMock;
        }

        public Mock<ILogger> SetLoggerMock()
        {
            Mock<ILogger> logger = new Mock<ILogger>();
            logger.Setup(m => m.Log(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
            logger.Setup(m => m.Log(It.IsAny<System.Exception>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
            logger.Setup(m => m.Log(It.IsAny<ExceptionContext>()));
            logger.Setup(m => m.Log(It.IsAny<MessageContext>()));
            logger.Setup(m => m.Log(It.IsAny<EventContext>()));
            logger.Setup(m => m.Log(It.IsAny<MetricContext>()));
            return logger;
        }
    }
}
