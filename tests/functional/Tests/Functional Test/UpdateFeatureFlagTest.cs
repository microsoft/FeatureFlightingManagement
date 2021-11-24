using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.FeatureFlighting.Tests.Functional.Helper;
using Microsoft.FeatureFlighting.Tests.Functional.Utilities;

namespace Microsoft.FeatureFlighting.Tests.Functional.Functional_Test
{
    [TestClass]
    public class UpdateFeatureFlagTest
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
        [Description("Test Case ID - 5676065")]
        public async Task Verify_UpdateFeatureFlag_returns_correct_response_for_correct_flagData_for_correct_env_correct_app_to_user()
        {
            //Arrange
            FeatureFlagClient flightingClient = ClientCreator.CreateFeatureFlagClient(_testContext);
            await CreateFlagHelper.CreateFlagWithEnabledFilterKey(_testContext);
            string environment = _testContext.Properties["FunctionalTest:Application:Environment"].ToString();
            string app = _testContext.Properties["FunctionalTest:Application"].ToString();
            string featureName = _testContext.Properties["FunctionalTest:FlagName:Enabled"].ToString();
            FeatureFlag featureFlagData = new ()
            {
                Id = $"{app.ToLowerInvariant()}_{environment.ToLowerInvariant()}_{featureName.ToLowerInvariant()}",
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
            var result = await flightingClient.UpdateFeatureFlag(featureFlagData, app, environment);
            
            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.NoContent.ToString(), result);
        }

        [TestCategory("Functional")]
        [TestMethod]
        [Priority(4)]
        [Description("Test Case ID - 5676066")]
        public async Task Verify_UpdateFeatureFlag_returns_403_response_for_correct_flagData_for_correct_env_incorrect_app_to_user()
        {
            //Arrange
            FeatureFlagClient flightingClient = ClientCreator.CreateFeatureFlagClient(_testContext);
            await CreateFlagHelper.CreateFlagWithEnabledFilterKey(_testContext);
            string environment = _testContext.Properties["FunctionalTest:Application:Environment"].ToString();
            string app = _testContext.Properties["FunctionalTest:Application"].ToString();
            string featureName = _testContext.Properties["FunctionalTest:FlagName:Enabled"].ToString();
            FeatureFlag featureFlagData = new()
            {
                Id = $"{app.ToLowerInvariant()}_{environment.ToLowerInvariant()}_{featureName.ToLowerInvariant()}",
                Description = "FunctionalTestingflagDescription",
                Enabled = true,
                Label = "FunctionalTestingflag",
                Name = featureName,
                Environment = environment.ToLowerInvariant(),
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
            var result = await flightingClient.UpdateFeatureFlag(featureFlagData, "Invalid", environment, useAlternateAccount: true);
            
            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden.ToString(), result);
        }

        [TestCategory("Functional")]
        [TestMethod]
        [Priority(4)]
        [Description("Test Case ID - 5676067")]
        public async Task Verify_UpdateFeatureFlag_returns_bad_request_for_correct_flagData_for_correct_env_null_app_to_user()
        {
            //Arrange
            FeatureFlagClient flightingClient = ClientCreator.CreateFeatureFlagClient(_testContext);
            await CreateFlagHelper.CreateFlagWithEnabledFilterKey(_testContext);
            string environment = _testContext.Properties["FunctionalTest:Application:Environment"].ToString();
            string app = _testContext.Properties["FunctionalTest:Application"].ToString();
            string featureName = _testContext.Properties["FunctionalTest:FlagName:Enabled"].ToString();
            FeatureFlag featureFlagData = new FeatureFlag()
            {
                Id = $"{app.ToLowerInvariant()}_{environment.ToLowerInvariant()}_{featureName.ToLowerInvariant()}",
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
            var result = await flightingClient.UpdateFeatureFlag(featureFlagData, null, environment);
            
            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest.ToString(), result);
        }
    }
}
