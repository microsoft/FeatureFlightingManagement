using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.FeatureFlighting.Tests.Functional.Helper;
using Microsoft.FeatureFlighting.Tests.Functional.Utilities;

namespace Microsoft.FeatureFlighting.Tests.Functional.Functional_Test
{
    [TestClass]
    public class CreateFeatureFlagTest
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
        [Description("Test Case ID - 5676060")]
        public async Task Verify_CreateFeatureFlag_returns_correct_response_for_correct_flagData_for_correct_env_correct_app_to_user()
        {
            //Arrange
            FeatureFlagClient flightingClient = ClientCreator.CreateFeatureFlagClient(_testContext);
            string environment = _testContext.Properties["FunctionalTest:Application:Environment"].ToString();
            string app = _testContext.Properties["FunctionalTest:Application"].ToString();
            string featureName = _testContext.Properties["FunctionalTest:FlagName:Enabled"].ToString();

            FeatureFlag featureFlagData = new()
            {
                Description = "FunctionalTestingflagDescription",
                Enabled = true,
                Label = "Testing",
                Name = featureName,
                Environment = environment,
                Conditions = new Condition()
                {
                    Client_Filters = new Filter[]
                    {
                        new Filter()
                        {
                            Name="Alias",
                            Parameters= new FilterSettings()
                            {
                                Operator = "In",
                                Value = "pratikb,prgolc,hkgupta,rokshi,akaleti",
                                IsActive = "true",
                                StageId = "0",
                                StageName = "stg1",
                                FlightContextKey = "Alias"
                            }
                        }
                    }

                }

            };


            //Act
            var result = await flightingClient.CreateFeatureFlag(featureFlagData, app, environment);
            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.Created.ToString(), result);
        }

        [TestCategory("Functional")]
        [TestMethod]
        [Priority(3)]
        [Description("Test Case ID - 5676062")]
        public async Task Verify_CreateFeatureFlag_returns_unauthorized_response_for_correct_flagData_for_correct_env_incorrect_app_to_user()
        {
            //Arrange
            FeatureFlagClient flightingClient = ClientCreator.CreateFeatureFlagClient(_testContext);
            string environment = _testContext.Properties["FunctionalTest:Application:Environment"].ToString();
            string featureName = _testContext.Properties["FunctionalTest:FlagName:Enabled"].ToString();
            FeatureFlag featureFlagData = new()
            {
                Description = "FunctionalTestingflagDescription",
                Enabled = true,
                Label = "FunctionalTestingflag",
                Name = featureName,
                Environment = environment,
                Conditions = new Condition()
                {
                    Client_Filters = new Filter[]
                   {
                        new Filter()
                        {
                            Name="Generic",
                            Parameters= new FilterSettings()
                            {
                                Operator = "Equals",
                                Value = "1",
                                IsActive = "true",
                                StageId = "0",
                                StageName = "stg1",
                                FlightContextKey = "Enabled"
                            }
                        }
                   }

                }

            };

            //Act
            var result = await flightingClient.CreateFeatureFlag(featureFlagData, "Invalid", environment, useAlternateAccount: true);
            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden.ToString(), result);
        }

        [TestCategory("Functional")]
        [TestMethod]
        [Priority(3)]
        [Description("Test Case ID - 5676063")]
        public async Task Verify_CreateFeatureFlag_returns_badRequest_response_for_incorrect_flagData_for_correct_env_incorrect_app_to_user()
        {
            //Arrange
            FeatureFlagClient flightingClient = ClientCreator.CreateFeatureFlagClient(_testContext);
            string environment = _testContext.Properties["FunctionalTest:Application:Environment"].ToString();
            string featureName = _testContext.Properties["FunctionalTest:FlagName:Enabled"].ToString();
            FeatureFlag featureFlagData = new()
            {
                Id = "field experience (fxp)_dev_FunctionalTestingflagForEnabled",
                Description = "FunctionalTestingflagDescription",
                Enabled = true,
                Label = "FunctionalTestingflag",
                Name = featureName,
                Environment = environment,
                Conditions = new Condition()
                {
                    Client_Filters = new Filter[]
                    {
                        new Filter()
                        {
                            Name="Generic",
                            Parameters= new FilterSettings()
                            {
                                Operator = "Equals",
                                Value = "1",
                                IsActive = "true",
                                StageId = "0",
                                StageName = "stg1",
                                FlightContextKey = "Enabled"
                            }
                        }
                    }

                }

            };

            //Act
            var result = await flightingClient.CreateFeatureFlag(featureFlagData, null, environment);

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest.ToString(), result);
        }
    }
}
