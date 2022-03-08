using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.FeatureFlighting.Tests.Functional.Helper;

namespace Microsoft.FeatureFlighting.Tests.Functional.Functional_Test
{
    [TestClass]
    public class ActivateStageTest
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
        [Description("Test Case ID - 5676082")]
        public async Task Verify_ActiveStage_returns_204_for_correct_env_correct_app_to_user()
        {
            //Arrange
            var flightingClient = ClientCreator.CreateFeatureFlagClient(_testContext);
            string environment = _testContext.Properties["FunctionalTest:Application:Environment"].ToString();
            string app = _testContext.Properties["FunctionalTest:Application"].ToString();
            string featureName = _testContext.Properties["FunctionalTest:FlagName"].ToString();
            await flightingClient.DeleteFeatureFlag(app, environment, featureName);
            await CreateFlagHelper.CreateFlag(_testContext);

            //Act
            var result = await flightingClient.ActivateStage(app, environment,featureName, "stg2");
            
            //Assert
            Assert.AreEqual(HttpStatusCode.NoContent.ToString(), result);

            // Cleanup
            await flightingClient.DeleteFeatureFlag(app, environment, featureName);
        }

        [TestCategory("Functional")]
        [TestMethod]
        [Priority(4)]
        [Description("Test Case ID - 5676083")]
        public async Task Verify_ActiveStage_returns_400_for_correct_env_incorrect_app_to_user()
        {
            //Arrange
            FeatureFlagClient flightingClient = ClientCreator.CreateFeatureFlagClient(_testContext);
            string app = _testContext.Properties["FunctionalTest:Application"].ToString();
            string environment = _testContext.Properties["FunctionalTest:Application:Environment"].ToString();
            string featureName = _testContext.Properties["FunctionalTest:FlagName"].ToString();
            await flightingClient.DeleteFeatureFlag(app, environment, featureName);
            await CreateFlagHelper.CreateFlag(_testContext);

            //Act
            var result = await flightingClient.ActivateStage("INVALID", environment, featureName, "stg2");
            
            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest.ToString(), result);

            // Cleanup
            await flightingClient.DeleteFeatureFlag(app, environment, featureName);
        }

        [TestCategory("Functional")]
        [TestMethod]
        [Priority(4)]
        [Description("Test Case ID - 5676084")]
        public async Task Verify_ActiveStage_returns_400_for_correct_env_null_app_to_user()
        {
            //Arrange
            FeatureFlagClient flightingClient = ClientCreator.CreateFeatureFlagClient(_testContext);
            string app = _testContext.Properties["FunctionalTest:Application"].ToString();
            string environment = _testContext.Properties["FunctionalTest:Application:Environment"].ToString();
            string featureName = _testContext.Properties["FunctionalTest:FlagName"].ToString();
            await flightingClient.DeleteFeatureFlag(app, environment, featureName);
            await CreateFlagHelper.CreateFlag(_testContext);

            //Act
            var result = await flightingClient.ActivateStage(null, environment, featureName, "stg2");
            
            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest.ToString(), result);

            // Cleanup
            await flightingClient.DeleteFeatureFlag(app, environment, featureName);
        }

        [TestCategory("Functional")]
        [TestMethod]
        [Priority(4)]
        [Description("Test Case ID - 5676085")]
        public async Task Verify_ActiveStage_returns_400_for_incorrect_flag_correct_env_correct_app_to_user()
        {
            //Arrange
            FeatureFlagClient flightingClient = ClientCreator.CreateFeatureFlagClient(_testContext);
            string environment = _testContext.Properties["FunctionalTest:Application:Environment"].ToString();
            string app = _testContext.Properties["FunctionalTest:Application"].ToString();
            string featureName = _testContext.Properties["FunctionalTest:FlagName"].ToString();
            await flightingClient.DeleteFeatureFlag(app, environment, featureName);
            await CreateFlagHelper.CreateFlag(_testContext);

            //Act
            var result = await flightingClient.ActivateStage(app, environment, "Invalid", "stg2");
            
            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest.ToString(), result);

            // Cleanup
            await flightingClient.DeleteFeatureFlag(app, environment, featureName);
        }
    }
}
