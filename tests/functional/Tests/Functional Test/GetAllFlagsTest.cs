using System;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.FeatureFlighting.Tests.Functional.Helper;

namespace Microsoft.FeatureFlighting.Tests.Functional.Functional_Test
{
    [TestClass]
    public class GetAllFlagsTest
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
        [Description("Test Case ID - 5676099")]
        public async Task Verify_GetAllFlags_returns_List_of_flags_for_correct_env_correct_app_to_user()
        {
            //Arrange
            FeatureFlagClient flightingClient = ClientCreator.CreateFeatureFlagClient(_testContext);
            string environment = _testContext.Properties["FunctionalTest:Application:Environment"].ToString();
            string app = _testContext.Properties["FunctionalTest:Application"].ToString();
            
            //Act
            var result = await flightingClient.GetFeatureFlags(app,environment);
            
            //Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
        }

        [TestCategory("Functional")]
        [TestMethod]
        [Priority(4)]
        [Description("Test Case ID - 5676102")]
        public async Task Verify__GetAllFlags_returns_400_for_incorrect_env_correct_app_where_no_flags_present()
        {
            //Arrange
            FeatureFlagClient flightingClient = ClientCreator.CreateFeatureFlagClient(_testContext);
            string app = _testContext.Properties["FunctionalTest:Application"].ToString();
            //Act
            var result = await flightingClient.GetFeatureFlags(app, "local");
            
            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest.ToString(), result.First().Id);
        }

        [TestCategory("Functional")]
        [TestMethod]
        [Priority(4)]
        [Description("Test Case ID - 5676101")]
        public async Task Verify__GetAllFlags_returns_404_for_correct_env_incorrect_app_where_no_flags_present()
        {
            //Arrange
            FeatureFlagClient flightingClient = ClientCreator.CreateFeatureFlagClient(_testContext);
            string environment = _testContext.Properties["FunctionalTest:Application:Environment"].ToString();
            //Act
            
            var result = await flightingClient.GetFeatureFlags(Guid.NewGuid().ToString() ,environment );

            //Assert
            Assert.AreEqual(HttpStatusCode.NotFound.ToString(), result.First().Id);
        }
    }
}
