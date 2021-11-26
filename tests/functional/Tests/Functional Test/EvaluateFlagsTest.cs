using System.Net;
using System.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.FeatureFlighting.Tests.Functional.Helper;
using Microsoft.FeatureFlighting.Tests.Functional.Utilities;

namespace Microsoft.FeatureFlighting.Tests.Functional.Functional_Test
{
    [TestClass]
    public class EvaluateFlagsTest
    {
        private static TestContext _testContext;

        [ClassInitialize]
        public static void Setup(TestContext testContext)
        {
            _testContext = testContext;
        }

        [TestCategory("Functional")]
        [TestMethod]
        [Priority(1)]
        [Description("Test Case ID - 5646128")]
        public async Task Verify_Evaluate_response_for_correct_Role_correct_env_correct_app()
        {
            //Arrange
            FeatureFlagClient flightingClient = ClientCreator.CreateFeatureFlagClient(_testContext);
            await CreateFlagHelper.CreateVerificationFlag(_testContext);
            string environment = _testContext.Properties["FunctionalTest:Application:Environment"].ToString();
            string app = _testContext.Properties["FunctionalTest:Application"].ToString();
            string featureName = _testContext.Properties["FunctionalTest:FlagName:Verification"].ToString();
            List<string> featureNameList = new();
            Context contextData = new() { Role = "Manager" };
            string context = JsonConvert.SerializeObject(contextData);
            featureNameList.Add(featureName);

            //Act
            var result = await flightingClient.Evaluate(app, environment, featureNameList, context);

            //Assert
            Assert.IsTrue(result[featureName]);
        }

        [TestCategory("Functional")]
        [TestMethod]
        [Priority(1)]
        [Description("Test Case ID - 5646338")]
        public async Task Verify_Evaluate_response_for_incorrect_Role_correct_env_correct_app()
        {
            //Arrange
            FeatureFlagClient flightingClient = ClientCreator.CreateFeatureFlagClient(_testContext);
            await CreateFlagHelper.CreateVerificationFlag(_testContext);
            string environment = _testContext.Properties["FunctionalTest:Application:Environment"].ToString();
            string app = _testContext.Properties["FunctionalTest:Application"].ToString();
            string featureName = _testContext.Properties["FunctionalTest:FlagName:Verification"].ToString();
            List<string> featureNameList = new();
            Context contextData = new() { Role = "Designer" };
            string context = JsonConvert.SerializeObject(contextData);
            featureNameList.Add(featureName);

            //Act
            var result = await flightingClient.Evaluate(app, environment, featureNameList, context.ToString());

            //Assert
            Assert.IsFalse(result[featureName]);
        }

        [TestCategory("Functional")]
        [TestMethod]
        [Priority(1)]
        [Description("Test Case ID - 5695806")]
        public async Task Verify_Evaluate_response_for_incorrect_Date_correct_env_correct_app()
        {
            //Arrange
            FeatureFlagClient flightingClient = ClientCreator.CreateFeatureFlagClient(_testContext);
            await CreateFlagHelper.CreateVerificationFlag(_testContext);
            string environment = _testContext.Properties["FunctionalTest:Application:Environment"].ToString();
            string app = _testContext.Properties["FunctionalTest:Application"].ToString();
            string featureName = _testContext.Properties["FunctionalTest:FlagName:Verification"].ToString();
            List<string> featureNameList = new();
            Context contextData = new() { Date = "1587540496000" };
            string context = JsonConvert.SerializeObject(contextData);
            featureNameList.Add(featureName);

            //Act
            var result = await flightingClient.Evaluate(app, environment, featureNameList, context.ToString());

            //Assert
            Assert.IsFalse(result[featureName]);
        }
        [TestCategory("Functional")]
        [TestMethod]
        [Priority(1)]
        [Description("Test Case ID - 5677782")]
        public async Task Verify_Evaluate_response_for_correct_Date_correct_env_correct_app()
        {
            //Arrange
            FeatureFlagClient flightingClient = ClientCreator.CreateFeatureFlagClient(_testContext);
            await CreateFlagHelper.CreateVerificationFlag(_testContext);
            string environment = _testContext.Properties["FunctionalTest:Application:Environment"].ToString();
            string app = _testContext.Properties["FunctionalTest:Application"].ToString();
            string featureName = _testContext.Properties["FunctionalTest:FlagName:Verification"].ToString();
            List<string> featureNameList = new();
            Context contextData = new() { Date = "1587022096000" };
            string context = JsonConvert.SerializeObject(contextData);
            featureNameList.Add(featureName);

            //Act
            var result = await flightingClient.Evaluate(app, environment, featureNameList, context.ToString());

            //Assert
            Assert.IsTrue(result[featureName]);
        }
        [TestCategory("Functional")]
        [TestMethod]
        [Priority(1)]
        [Description("Test Case ID - 5695861")]
        public async Task Verify_Evaluate_response_for_incorrect_Date_correct_env_correct_app_without_date_context()
        {
            //Arrange
            FeatureFlagClient flightingClient = ClientCreator.CreateFeatureFlagClient(_testContext);
            await CreateFlagHelper.CreateVerificationFlag(_testContext);
            string environment = _testContext.Properties["FunctionalTest:Application:Environment"].ToString();
            string app = _testContext.Properties["FunctionalTest:Application"].ToString();
            string featureName = _testContext.Properties["FunctionalTest:FlagName:Verification"].ToString();
            List<string> featureNameList = new();
            Context contextData = new();
            string context = JsonConvert.SerializeObject(contextData);
            featureNameList.Add(featureName);

            //Act
            var result = await flightingClient.Evaluate(app, environment, featureNameList, context.ToString());

            //Assert
            Assert.IsFalse(result[featureName]);
        }
        [TestCategory("Functional")]
        [TestMethod]
        [Priority(1)]
        [Description("Test Case ID - 5646343")]
        public async Task Verify_Evaluate_response_for_correct_Region_correct_env_correct_app()
        {
            //Arrange
            FeatureFlagClient flightingClient = ClientCreator.CreateFeatureFlagClient(_testContext);
            await CreateFlagHelper.CreateVerificationFlag(_testContext);
            string environment = _testContext.Properties["FunctionalTest:Application:Environment"].ToString();
            string app = _testContext.Properties["FunctionalTest:Application"].ToString();
            string featureName = _testContext.Properties["FunctionalTest:FlagName:Verification"].ToString();
            List<string> featureNameList = new();
            Context contextData = new() { Region = "Local" };
            string context = JsonConvert.SerializeObject(contextData);
            featureNameList.Add(featureName);

            //Act
            var result = await flightingClient.Evaluate(app, environment, featureNameList, context.ToString());

            //Assert
            Assert.IsTrue(result[featureName]);
        }

        [TestCategory("Functional")]
        [TestMethod]
        [Description("Test Case ID - 5646346")]
        [Priority(1)]
        public async Task Verify_Evaluate_response_for_incorrect_Region_correct_env_correct_app()
        {
            //Arrange
            FeatureFlagClient flightingClient = ClientCreator.CreateFeatureFlagClient(_testContext);
            await CreateFlagHelper.CreateVerificationFlag(_testContext);
            string environment = _testContext.Properties["FunctionalTest:Application:Environment"].ToString();
            string app = _testContext.Properties["FunctionalTest:Application"].ToString();
            string featureName = _testContext.Properties["FunctionalTest:FlagName:Verification"].ToString();
            List<string> featureNameList = new();
            Context contextData = new() { Region = "Global" };
            string context = JsonConvert.SerializeObject(contextData);
            featureNameList.Add(featureName);

            //Act
            var result = await flightingClient.Evaluate(app, environment, featureNameList, context.ToString());

            //Assert
            Assert.IsFalse(result[featureName]);
        }

        [TestCategory("Functional")]
        [TestMethod]
        [Description("Test Case ID - 5676044")]
        [Priority(1)]
        public async Task Verify_Evaluate_response_for_correct_RoleGroup_correct_env_correct_app()
        {
            //Arrange
            FeatureFlagClient flightingClient = ClientCreator.CreateFeatureFlagClient(_testContext);
            await CreateFlagHelper.CreateVerificationFlag(_testContext);
            string environment = _testContext.Properties["FunctionalTest:Application:Environment"].ToString();
            string app = _testContext.Properties["FunctionalTest:Application"].ToString();
            string featureName = _testContext.Properties["FunctionalTest:FlagName:Verification"].ToString();
            List<string> featureNameList = new();
            Context contextData = new() { RoleGroup = "3" };
            string context = JsonConvert.SerializeObject(contextData);
            featureNameList.Add(featureName);

            //Act
            var result = await flightingClient.Evaluate(app, environment, featureNameList, context.ToString());

            //Assert
            Assert.IsTrue(result[featureName]);
        }

        [TestCategory("Functional")]
        [TestMethod]
        [Description("Test Case ID - 5676045")]
        [Priority(1)]
        public async Task Verify_Evaluate_response_for_incorrect_RoleGroup_correct_env_correct_app()
        {
            //Arrange
            FeatureFlagClient flightingClient = ClientCreator.CreateFeatureFlagClient(_testContext);
            await CreateFlagHelper.CreateVerificationFlag(_testContext);
            string environment = _testContext.Properties["FunctionalTest:Application:Environment"].ToString();
            string app = _testContext.Properties["FunctionalTest:Application"].ToString();
            string featureName = _testContext.Properties["FunctionalTest:FlagName:Verification"].ToString();
            List<string> featureNameList = new();
            Context contextData = new() { RoleGroup = "1" };
            string context = JsonConvert.SerializeObject(contextData);
            featureNameList.Add(featureName);

            //Act
            var result = await flightingClient.Evaluate(app, environment, featureNameList, context.ToString());

            //Assert
            Assert.IsFalse(result[featureName]);
        }


        [TestCategory("Functional")]
        [TestMethod]
        [Description("Test Case ID - 5676046")]
        [Priority(1)]
        public async Task Verify_Evaluate_response_for_correct_Generic_context_correct_env_correct_app()
        {
            //Arrange
            FeatureFlagClient flightingClient = ClientCreator.CreateFeatureFlagClient(_testContext);
            await CreateFlagHelper.CreateVerificationFlag(_testContext);
            string environment = _testContext.Properties["FunctionalTest:Application:Environment"].ToString();
            string app = _testContext.Properties["FunctionalTest:Application"].ToString();
            string featureName = _testContext.Properties["FunctionalTest:FlagName:Verification"].ToString();
            List<string> featureNameList = new();
            Context contextData = new() { number = "8" };
            string context = JsonConvert.SerializeObject(contextData);
            featureNameList.Add(featureName);

            //Act
            var result = await flightingClient.Evaluate(app, environment, featureNameList, context.ToString());

            //Assert
            Assert.IsTrue(result[featureName]);
        }

        [TestCategory("Functional")]
        [TestMethod]
        [Description("Test Case ID - 5676047")]
        [Priority(1)]
        public async Task Verify_Evaluate_response_for_incorrect_Generic_context_correct_env_correct_app()
        {
            //Arrange
            FeatureFlagClient flightingClient = ClientCreator.CreateFeatureFlagClient(_testContext);
            await CreateFlagHelper.CreateVerificationFlag(_testContext);
            string environment = _testContext.Properties["FunctionalTest:Application:Environment"].ToString();
            string app = _testContext.Properties["FunctionalTest:Application"].ToString();
            string featureName = _testContext.Properties["FunctionalTest:FlagName:Verification"].ToString();
            List<string> featureNameList = new();
            Context contextData = new() { number = "1" };
            string context = JsonConvert.SerializeObject(contextData);
            featureNameList.Add(featureName);

            //Act
            var result = await flightingClient.Evaluate(app, environment, featureNameList, context.ToString());

            //Assert
            Assert.IsFalse(result[featureName]);
        }


        [TestCategory("Functional")]
        [TestMethod]
        [Description("Test Case ID - 5676048")]
        [Priority(1)]
        public async Task Verify_Evaluate_response_for_correct_Country_context_correct_env_correct_app()
        {
            //Arrange
            FeatureFlagClient flightingClient = ClientCreator.CreateFeatureFlagClient(_testContext);
            await CreateFlagHelper.CreateVerificationFlag(_testContext);
            string environment = _testContext.Properties["FunctionalTest:Application:Environment"].ToString();
            string app = _testContext.Properties["FunctionalTest:Application"].ToString();
            string featureName = _testContext.Properties["FunctionalTest:FlagName:Verification"].ToString();
            List<string> featureNameList = new();
            Context contextData = new() { Country = "India" };
            string context = JsonConvert.SerializeObject(contextData);
            featureNameList.Add(featureName);

            //Act
            var result = await flightingClient.Evaluate(app, environment, featureNameList, context.ToString());

            //Assert
            Assert.IsTrue(result[featureName]);
        }

        [TestCategory("Functional")]
        [TestMethod]
        [Description("Test Case ID - 5676049")]
        [Priority(1)]
        public async Task Verify_Evaluate_response_for_incorrect_Country_context_correct_env_correct_app()
        {
            //Arrange
            FeatureFlagClient flightingClient = ClientCreator.CreateFeatureFlagClient(_testContext);
            await CreateFlagHelper.CreateVerificationFlag(_testContext);
            string environment = _testContext.Properties["FunctionalTest:Application:Environment"].ToString();
            string app = _testContext.Properties["FunctionalTest:Application"].ToString();
            string featureName = _testContext.Properties["FunctionalTest:FlagName:Verification"].ToString();
            List<string> featureNameList = new();
            Context contextData = new() { Country = "Italy" };
            string context = JsonConvert.SerializeObject(contextData);
            featureNameList.Add(featureName);
            //Act
            var result = await flightingClient.Evaluate(app, environment, featureNameList, context.ToString());
            //Assert
            Assert.IsFalse(result[featureName]);
        }

        [TestCategory("Functional")]
        [TestMethod]
        [Description("Test Case ID - 5676050")]
        [Priority(1)]
        public async Task Verify_Evaluate_response_for_correct_UserUpn_context_correct_env_correct_app()
        {
            //Arrange
            FeatureFlagClient flightingClient = ClientCreator.CreateFeatureFlagClient(_testContext);
            await CreateFlagHelper.CreateVerificationFlag(_testContext);
            string environment = _testContext.Properties["FunctionalTest:Application:Environment"].ToString();
            string app = _testContext.Properties["FunctionalTest:Application"].ToString();
            string featureName = _testContext.Properties["FunctionalTest:FlagName:Verification"].ToString();
            List<string> featureNameList = new();
            Context contextData = new() { Upn = "pratikb@microsoft.com" };
            string context = JsonConvert.SerializeObject(contextData);
            featureNameList.Add(featureName);
            //Act
            var result = await flightingClient.Evaluate(app, environment, featureNameList, context.ToString());
            //Assert
            Assert.IsTrue(result[featureName]);
        }

        [TestCategory("Functional")]
        [TestMethod]
        [Description("Test Case ID - 5676051")]
        [Priority(1)]
        public async Task Verify_Evaluate_response_for_incorrect_UserUpn_context_correct_env_correct_app()
        {
            //Arrange
            FeatureFlagClient flightingClient = ClientCreator.CreateFeatureFlagClient(_testContext);
            await CreateFlagHelper.CreateVerificationFlag(_testContext);
            string environment = _testContext.Properties["FunctionalTest:Application:Environment"].ToString();
            string app = _testContext.Properties["FunctionalTest:Application"].ToString();
            string featureName = _testContext.Properties["FunctionalTest:FlagName:Verification"].ToString();
            List<string> featureNameList = new();
            Context contextData = new() { Upn = "morat@microsoft.com" };
            string context = JsonConvert.SerializeObject(contextData);
            featureNameList.Add(featureName);

            //Act
            var result = await flightingClient.Evaluate(app, environment, featureNameList, context.ToString());

            //Assert
            Assert.IsFalse(result[featureName]);
        }

        [TestCategory("Functional")]
        [TestMethod]
        [Description("Test Case ID - 5676052")]
        [Priority(1)]
        public async Task Verify_Evaluate_response_for_correct_Alias_context_correct_env_correct_app()
        {
            //Arrange
            FeatureFlagClient flightingClient = ClientCreator.CreateFeatureFlagClient(_testContext);
            await CreateFlagHelper.CreateVerificationFlag(_testContext);
            string environment = _testContext.Properties["FunctionalTest:Application:Environment"].ToString();
            string app = _testContext.Properties["FunctionalTest:Application"].ToString();
            string featureName = _testContext.Properties["FunctionalTest:FlagName:Verification"].ToString();
            List<string> featureNameList = new();
            Context contextData = new() { Alias = "morat" };
            string context = JsonConvert.SerializeObject(contextData);
            featureNameList.Add(featureName);

            //Act
            var result = await flightingClient.Evaluate(app, environment, featureNameList, context.ToString());

            //Assert
            Assert.IsTrue(result[featureName]);
        }

        [TestCategory("Functional")]
        [TestMethod]
        [Description("Test Case ID - 5676053")]
        [Priority(1)]
        public async Task Verify_Evaluate_response_for_incorrect_alias_context_correct_env_correct_app()
        {
            //Arrange
            FeatureFlagClient flightingClient = ClientCreator.CreateFeatureFlagClient(_testContext);
            await CreateFlagHelper.CreateVerificationFlag(_testContext);
            string environment = _testContext.Properties["FunctionalTest:Application:Environment"].ToString();
            string app = _testContext.Properties["FunctionalTest:Application"].ToString();
            string featureName = _testContext.Properties["FunctionalTest:FlagName:Verification"].ToString();
            List<string> featureNameList = new();
            Context contextData = new() { Alias = "pratikb" };
            string context = JsonConvert.SerializeObject(contextData);
            featureNameList.Add(featureName);

            //Act
            var result = await flightingClient.Evaluate(app, environment, featureNameList, context.ToString());

            //Assert
            Assert.IsFalse(result[featureName]);
        }

        [TestCategory("Functional")]
        [TestMethod]
        [Description("Test Case ID - 5676054")]
        [Priority(3)]
        public async Task Verify_Evaluate_returns_false_for_correct_env_correct_app_when_context_not_present()
        {
            //Arrange
            FeatureFlagClient flightingClient = ClientCreator.CreateFeatureFlagClient(_testContext);
            await CreateFlagHelper.CreateVerificationFlag(_testContext);
            string environment = _testContext.Properties["FunctionalTest:Application:Environment"].ToString();
            string app = _testContext.Properties["FunctionalTest:Application"].ToString();
            string featureName = _testContext.Properties["FunctionalTest:FlagName:Verification"].ToString();
            List<string> featureNameList = new();
            Context contextData = new();
            string context = JsonConvert.SerializeObject(contextData);
            featureNameList.Add(featureName);

            //Act
            var result = await flightingClient.Evaluate(app, environment, featureNameList, context.ToString());

            //Assert
            Assert.IsFalse(result[featureName]);
        }

        [TestCategory("Functional")]
        [TestMethod]
        [Description("Test Case ID - 5676055")]
        [Priority(3)]
        public async Task Verify_Evaluate_response_for_correct_env_incorrect_app_correct_context_to_user()
        {
            //Arrange
            FeatureFlagClient flightingClient = ClientCreator.CreateFeatureFlagClient(_testContext);
            await CreateFlagHelper.CreateVerificationFlag(_testContext);
            string environment = _testContext.Properties["FunctionalTest:Application:Environment"].ToString();
            string featureName = _testContext.Properties["FunctionalTest:FlagName:Verification"].ToString();
            List<string> featureNameList = new();
            Context contextData = new() { Alias = "twsharma" };
            string context = JsonConvert.SerializeObject(contextData);
            featureNameList.Add(featureName);

            //Act
            var result = await flightingClient.Evaluate("InvalidApp", environment, featureNameList, context.ToString());

            //Assert
            Assert.IsFalse(result[featureName]);
        }

        [TestCategory("Functional")]
        [TestMethod]
        [Description("Test Case ID - 5676056")]
        [Priority(3)]
        public async Task Verify_Evaluate_response_for_nonExisting_flag_correct_env_correct_app_correct_context_to_user()
        {
            //Arrange
            FeatureFlagClient flightingClient = ClientCreator.CreateFeatureFlagClient(_testContext);
            await CreateFlagHelper.CreateVerificationFlag(_testContext);
            string environment = _testContext.Properties["FunctionalTest:Application:Environment"].ToString();
            string app = _testContext.Properties["FunctionalTest:Application"].ToString();
            string featureName = "InvalidFlag";
            List<string> featureNameList = new();
            Context contextData = new() { Alias = "twsharma" };
            string context = JsonConvert.SerializeObject(contextData);
            featureNameList.Add(featureName);

            //Act
            var result = await flightingClient.Evaluate(app, environment, featureNameList, context.ToString());

            //Assert
            Assert.IsFalse(result[featureName]);
        }

        [TestCategory("Functional")]
        [TestMethod]
        [Description("Test Case ID - 5676057")]
        [Priority(3)]
        public async Task Verify_All_Feature_Flags_Are_Evaluated_for_empty_flagList_correct_env_correct_app_correct_context_to_user()
        {
            //Arrange
            FeatureFlagClient flightingClient = ClientCreator.CreateFeatureFlagClient(_testContext);
            await CreateFlagHelper.CreateVerificationFlag(_testContext);
            string environment = _testContext.Properties["FunctionalTest:Application:Environment"].ToString();
            string app = _testContext.Properties["FunctionalTest:Application"].ToString();
            List<string> featureNameList = new();
            Context contextData = new() { Alias = "twsharma" };
            string context = JsonConvert.SerializeObject(contextData);

            //Act
            var result = await flightingClient.Evaluate(app, environment, featureNameList, context.ToString());

            //Assert
            Assert.IsNotNull(result);
            Assert.AreNotEqual(HttpStatusCode.BadRequest.ToString(), result.First().Key);

        }

        [TestCategory("Functional")]
        [TestMethod]
        [Priority(1)]
        [Description("Test Case ID - 5646128")]
        public async Task Verify_Evaluate_response_for_Rules_Engine()
        {
            //Arrange
            FeatureFlagClient flightingClient = ClientCreator.CreateFeatureFlagClient(_testContext);
            await CreateFlagHelper.CreateFlagWithBusinessRuleEngine(_testContext);
            string environment = _testContext.Properties["FunctionalTest:Application:Environment"].ToString();
            string app = _testContext.Properties["FunctionalTest:Application"].ToString();
            string featureName = _testContext.Properties["FunctionalTest:FlagName:BRE"].ToString();
            List<string> featureNameList = new();
            Context contextData = new() { EOU = "United States - Healthcare", Country = "UK", UserPrincipalName = "pratikb@microsoft.com", RoleGroup = "4" };
            string context = JsonConvert.SerializeObject(contextData);
            featureNameList.Add(featureName);

            //Act
            var result = await flightingClient.Evaluate(app, environment, featureNameList, context);

            //Assert
            Assert.IsTrue(result[featureName]);
        }

        [TestCategory("Functional")]
        [TestMethod]
        [Priority(1)]
        [Description("Test Case ID - 5646128")]
        public async Task Verify_Evaluate_response_for_Completementary_Rules_Engine()
        {
            //Arrange
            FeatureFlagClient flightingClient = ClientCreator.CreateFeatureFlagClient(_testContext);
            await CreateFlagHelper.CreateFlagWithBusinessRuleEngine(_testContext, complementary: true);
            string environment = _testContext.Properties["FunctionalTest:Application:Environment"].ToString();
            string app = _testContext.Properties["FunctionalTest:Application"].ToString();
            string featureName = _testContext.Properties["FunctionalTest:FlagName:BRE:Complementary"].ToString();
            List<string> featureNameList = new();
            Context contextData = new() { EOU = "United States - Healthcare", Country = "UK", UserPrincipalName = "pratikb@microsoft.com", RoleGroup = "4" };
            string context = JsonConvert.SerializeObject(contextData);
            featureNameList.Add(featureName);

            //Act
            var result = await flightingClient.Evaluate(app, environment, featureNameList, context);

            //Assert
            Assert.IsFalse(result[featureName]);
        }

    }
}
