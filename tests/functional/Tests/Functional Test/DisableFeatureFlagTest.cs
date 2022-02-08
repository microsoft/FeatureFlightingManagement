using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.FeatureFlighting.Tests.Functional.Helper;

namespace Microsoft.FeatureFlighting.Tests.Functional.Functional_Test
{
    [TestClass]
    public class DisableFeatureFlagTest
    {
        private static TestContext _testContext;

        [ClassInitialize]
        public static void Setup(TestContext testContext)
        {
            _testContext = testContext;
        }

        [TestCategory("Functional")]
        [TestMethod]
        [Priority(3)]
        [Description("Test Case ID - 5676087")]
        public async Task Verify_DisableFlag_returns_NoContent_for_correct_env_correct_app_to_user()
        {
            //Arrange
            FeatureFlagClient flightingClient = ClientCreator.CreateFeatureFlagClient(_testContext);
            await CreateFlagHelper.CreateFlag(_testContext);
            string environment = _testContext.Properties["FunctionalTest:Application:Environment"].ToString();
            string app = _testContext.Properties["FunctionalTest:Application"].ToString();
            string featureName = _testContext.Properties["FunctionalTest:FlagName"].ToString();
            
            //Act
            var result = await flightingClient.DisableFeatureFlag(app, environment, featureName);
            
            //Assert
            Assert.AreEqual(HttpStatusCode.NoContent.ToString(), result);
        }

        [TestCategory("Functional")]
        [TestMethod]
        [Priority(4)]
        [Description("Test Case ID - 5676088")]
        public async Task Verify_DisableFlag_returns_400_for_correct_env_incorrect_app_to_user()
        {
            //Arrange
            FeatureFlagClient flightingClient = ClientCreator.CreateFeatureFlagClient(_testContext);
            await CreateFlagHelper.CreateFlag(_testContext);
            string environment = _testContext.Properties["FunctionalTest:Application:Environment"].ToString();
            string featureName = _testContext.Properties["FunctionalTest:FlagName"].ToString();
            //Act
            var result = await flightingClient.DisableFeatureFlag("INVALID", environment, featureName, useAlternateAccount: true);
            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest.ToString(), result);
        }

        [TestCategory("Functional")]
        [TestMethod]
        [Priority(4)]
        [Description("Test Case ID - 5676089")]
        public async Task Verify_DisableFlag_returns_400_for_correct_env_null_app_to_user()
        {
            //Arrange
            FeatureFlagClient flightingClient = ClientCreator.CreateFeatureFlagClient(_testContext);
            await CreateFlagHelper.CreateFlag(_testContext);
            string environment = _testContext.Properties["FunctionalTest:Application:Environment"].ToString();
            string featureName = _testContext.Properties["FunctionalTest:FlagName"].ToString();
            
            //Act
            var result = await flightingClient.DisableFeatureFlag(null, environment, featureName);
            
            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest.ToString(), result);
        }

        [TestCategory("Functional")]
        [TestMethod]
        [Priority(4)]
        [Description("Test Case ID - 5676090")]
        public async Task Verify_DisableFlag_returns_400_for_incorrect_flag_correct_env_correct_app_to_user()
        {
            //Arrange
            FeatureFlagClient flightingClient = ClientCreator.CreateFeatureFlagClient(_testContext);
            string environment = _testContext.Properties["FunctionalTest:Application:Environment"].ToString();
            string app = _testContext.Properties["FunctionalTest:Application"].ToString();

            //Act
            var result = await flightingClient.DisableFeatureFlag(app, environment, "Invalid");
            
            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest.ToString(), result);
        }
    }
}
