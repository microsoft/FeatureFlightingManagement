using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.FeatureFlighting.Tests.Functional.Helper;

namespace Microsoft.FeatureFlighting.Tests.Functional.Functional_Test
{
    [TestClass]
    public class GetFlagByFeatureNameTest
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
        [Description("Test Case ID - 5676104")]
        public async Task Verify_GetFlag_returns_featureFlagData_for_valid_flag_for_correct_env_correct_app_to_user()
        {
            //Arrange
            FeatureFlagClient flightingClient = ClientCreator.CreateFeatureFlagClient(_testContext);
            await CreateFlagHelper.CreateFlag(_testContext);
            string environment = _testContext.Properties["FunctionalTest:Application:Environment"].ToString();
            string app = _testContext.Properties["FunctionalTest:Application"].ToString();
            string featureName = _testContext.Properties["FunctionalTest:FlagName"].ToString();
            
            //Act
            var result = await flightingClient.GetFeatureFlag(featureName,app, environment);
            
            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Id, app.ToLowerInvariant()+"_"+environment.ToLowerInvariant()+"_"+featureName);
        }

        [TestCategory("Functional")]
        [TestMethod]
        [Priority(4)]
        [Description("Test Case ID - 5676105")]
        public async Task Verify_GetFlag_returns_404_for_invalid_flag_correct_env_correct_app_to_user()
        {
            //Arrange
            FeatureFlagClient flightingClient = ClientCreator.CreateFeatureFlagClient(_testContext);
            string environment = _testContext.Properties["FunctionalTest:Application:Environment"].ToString();
            string app = _testContext.Properties["FunctionalTest:Application"].ToString();
          
            //Act
            var result = await flightingClient.GetFeatureFlag("INVALIDFlag", app, environment);
            
            //Assert
            Assert.AreEqual(HttpStatusCode.NotFound.ToString(), result.Id);
        }

        [TestCategory("Functional")]
        [TestMethod]
        [Priority(4)]
        [Description("Test Case ID - 5676106")]
        public async Task Verify__GetFlag_returns_400_for_incorrect_env_correct_app()
        {
            //Arrange
            FeatureFlagClient flightingClient = ClientCreator.CreateFeatureFlagClient(_testContext);
            await CreateFlagHelper.CreateFlag(_testContext);
            string app = _testContext.Properties["FunctionalTest:Application"].ToString();
            string featureName = _testContext.Properties["FunctionalTest:FlagName"].ToString();
            
            //Act
            var result = await flightingClient.GetFeatureFlag(featureName,app, "local");

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest.ToString(), result.Id);
        }

        [TestCategory("Functional")]
        [TestMethod]
        [Priority(4)]
        [Description("Test Case ID - 5676107")]
        public async Task Verify__GetFlag_returns_404_for_correct_env_incorrect_app()
        {
            //Arrange
            FeatureFlagClient flightingClient = ClientCreator.CreateFeatureFlagClient(_testContext);
            await CreateFlagHelper.CreateFlag(_testContext);
            string environment = _testContext.Properties["FunctionalTest:Application:Environment"].ToString();
            string featureName = _testContext.Properties["FunctionalTest:FlagName"].ToString();
            
            //Act
             var result = await flightingClient.GetFeatureFlag(featureName,"INVALID", environment);

            //Assert
            Assert.AreEqual(HttpStatusCode.NotFound.ToString(), result.Id);
        }
    }
}
