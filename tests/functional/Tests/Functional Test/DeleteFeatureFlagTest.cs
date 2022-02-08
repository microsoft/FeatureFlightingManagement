using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.FeatureFlighting.Tests.Functional.Helper;
using System;

namespace Microsoft.FeatureFlighting.Tests.Functional.Functional_Test
{
    [TestClass]
    public class DeleteFeatureFlagTest
    {
        private static TestContext _testContext;

        [ClassInitialize]
        public static void Setup(TestContext testContext)
        {
            _testContext = testContext;
        }

        [TestCategory("Functional")]
        [TestMethod]
        [Priority(2)]
        [Description("Test Case ID - 5676070")]
        public async Task Verify_DeleteFlag_returns_204_for_correct_env_correct_app_to_user()
        {
            //Arrange
            string featureName = Guid.NewGuid().ToString();
            FeatureFlagClient flightingClient = ClientCreator.CreateFeatureFlagClient(_testContext);
            await CreateFlagHelper.CreateFlag(_testContext, featureName);
            string environment = _testContext.Properties["FunctionalTest:Application:Environment"].ToString();
            string app = _testContext.Properties["FunctionalTest:Application"].ToString();

            //Act
            var createdFlag = await flightingClient.GetFeatureFlag(featureName, app, environment);
            var result = await flightingClient.DeleteFeatureFlag(app, environment, featureName);

            //Assert
            Assert.IsNotNull(createdFlag);
            Assert.AreEqual(HttpStatusCode.NoContent.ToString(), result);
            var deletedFlag = await flightingClient.GetFeatureFlag(featureName, app, environment);
            Assert.AreEqual(HttpStatusCode.NotFound.ToString(), deletedFlag.Id);
        }

        [TestCategory("Functional")]
        [TestMethod]
        [Priority(3)]
        [Description("Test Case ID - 5676071")]
        public async Task Verify_DeleteFlag_returns_400_for_correct_env_incorrect_app_to_user()
        {
            //Arrange
            FeatureFlagClient flightingClient = ClientCreator.CreateFeatureFlagClient(_testContext);
            await CreateFlagHelper.CreateFlag(_testContext);
            string environment = _testContext.Properties["FunctionalTest:Application:Environment"].ToString();
            string featureName = _testContext.Properties["FunctionalTest:FlagName"].ToString();
            
            //Act
            var result = await flightingClient.DeleteFeatureFlag("INVALID", environment, featureName, useAlternateAccount: true);
            
            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest.ToString(), result);
        }

        [TestCategory("Functional")]
        [TestMethod]
        [Priority(3)]
        [Description("Test Case ID - 5676078")]
        public async Task Verify_DeleteFlag_returns_400_for_correct_env_null_app_to_user()
        {
            //Arrange
            FeatureFlagClient flightingClient = ClientCreator.CreateFeatureFlagClient(_testContext);
            await CreateFlagHelper.CreateFlag(_testContext);
            string environment = _testContext.Properties["FunctionalTest:Application:Environment"].ToString();
            string featureName = _testContext.Properties["FunctionalTest:FlagName"].ToString();
            //Act
            var result = await flightingClient.DeleteFeatureFlag(null, environment, featureName);
            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest.ToString(), result);
        }

        [TestCategory("Functional")]
        [TestMethod]
        [Priority(4)]
        [Description("Test Case ID - 5676080")]
        public async Task Verify_DeleteFlag_returns_400_for_incorrect_flag_correct_env_correct_app_to_user()
        {
            //Arrange
            FeatureFlagClient flightingClient = ClientCreator.CreateFeatureFlagClient(_testContext);
            string environment = _testContext.Properties["FunctionalTest:Application:Environment"].ToString();
            string app = _testContext.Properties["FunctionalTest:Application"].ToString();
            
            //Act
            var result = await flightingClient.DeleteFeatureFlag(app, environment, "InvalidFlag");
            
            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest.ToString(), result);
        }
    }
}
