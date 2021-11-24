using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.FeatureFlighting.Tests.Functional.Utilities;

namespace Microsoft.FeatureFlighting.Tests.Functional.Helper
{
    public static class CreateFlagHelper
    {
        public static async Task CreateFlag(TestContext _testContext, string flagName = null)
        {
            FeatureFlagClient flightingClient = ClientCreator.CreateFeatureFlagClient(_testContext);
            string featureFlagName = _testContext.Properties["FunctionalTest:FlagName"].ToString();
            string environment = _testContext.Properties["FunctionalTest:Application:Environment"].ToString();
            string app = _testContext.Properties["FunctionalTest:Application"].ToString();
            string featureFlagData = @"
                    {     
                          'description': 'FunctionalTestingflagDescription',
                          'enabled': true,
                          'label': 'FunctionalTesting',
                          'name': '',
                          'environment': '',
                          'conditions': {
                            'client_filters': [
                              {
                                'name': 'Alias',
                                'parameters': {
                                  'operator': 'NotMemberOfSecurityGroup',
                                  'value': 'fxpswe',
                                  'isActive': 'true',
                                  'stageId': '0',
                                  'stageName': 'stg1',
                                  'flightContextKey': 'alias'
                                  }
                             },
                             {
                                'name': 'Country',
                                'parameters': {
                                  'operator': 'In',
                                  'value': 'India,USA',
                                  'isActive': 'true',
                                  'stageId': '0',
                                  'stageName': 'stg1',
                                  'flightContextKey': 'Country'
                                }
                             },
                             {
                                'name': 'UserUpn',
                                'parameters': {
                                  'operator': 'MemberOfSecurityGroup',
                                  'value': 'fxpswe',
                                  'isActive': 'true',
                                  'stageId': '0',
                                  'stageName': 'stg1',
                                  'flightContextKey': 'upn'
                                }
                             },
                             {
                                'name': 'Generic',
                                'parameters': {
                                  'operator': 'GreaterThan',
                                  'value': '5',
                                  'isActive': 'true',
                                  'stageId': '0',
                                  'stageName': 'stg1',
                                  'flightContextKey': 'number'
                                }
                             },
                             {
                                'name': 'RoleGroup',
                                'parameters': {
                                  'operator': 'NotIn',
                                  'value': '1,2',
                                  'isActive': 'true',
                                  'stageId': '0',
                                  'stageName': 'stg1',
                                  'flightContextKey': 'RoleGroup'
                                }
                             }
,
                             {
                                'name': 'Region',
                                'parameters': {
                                  'operator': 'NotEquals',
                                  'value': 'global',
                                  'isActive': 'true',
                                  'stageId': '0',
                                  'stageName': 'stg1',
                                  'flightContextKey': 'Region'
                                }
                             },
                             {
                                'name': 'Role',
                                'parameters': {
                                  'operator': 'Equals',
                                  'value': 'Manager',
                                  'isActive': 'true',
                                  'stageId': '0',
                                  'stageName': 'stg1',
                                  'flightContextKey': 'Role'
                                }
                             },
                             {
                                'name': 'Date',
                                'parameters': {
                                  'operator': 'LessThan',
                                  'value': '1587367696000',
                                  'isActive': 'true',
                                  'stageId': '0',
                                  'stageName': 'stg1',
                                  'flightContextKey': 'Date'
                                }
                             },
                            ]
                          }
                    }";

            FeatureFlag featureFlagPayLoad = JsonConvert.DeserializeObject<FeatureFlag>(featureFlagData);
            featureFlagPayLoad.Name = featureFlagName;
            featureFlagPayLoad.Environment = environment;
            //update filter value for sg:fxpswe
            if (!string.IsNullOrWhiteSpace(flagName))
                featureFlagPayLoad.Name = flagName;
            var filters = featureFlagPayLoad.Conditions.Client_Filters;
            string filterValue = "[{\"Name\":\"fxpswe\",\"ObjectId\":\"6525439e-8512-4859-bb13-5a97ba5c0ff3\"}]";
            filters[0].Parameters.Value = filterValue;
            filters[2].Parameters.Value = filterValue;
            await flightingClient.CreateFeatureFlag(featureFlagPayLoad, app, environment);
        }


        public static async Task CreateFlagWithEnabledFilterKey(TestContext _testContext)
        {
            FeatureFlagClient flightingClient = ClientCreator.CreateFeatureFlagClient(_testContext);
            string featureFlagName = _testContext.Properties["FunctionalTest:FlagName:Enabled"].ToString();
            string environment = _testContext.Properties["FunctionalTest:Application:Environment"].ToString();
            string app = _testContext.Properties["FunctionalTest:Application"].ToString();
            string featureFlagData = @"
                    {     
                          'description': 'FunctionalTestingflagDescription',
                          'enabled': true,
                          'label': 'FunctionalTesting',
                          'name': '',
                          'environment': '',
                          'conditions': {
                            'client_filters': [
                              
                             {
                                'name': 'Generic',
                                'parameters': {
                                  'operator': 'Equals',
                                  'value': '1',
                                  'isActive': 'true',
                                  'stageId': '0',
                                  'stageName': 'stg1',
                                  'flightContextKey': 'Enabled'
                                }
                             }
                            ]
                          }
                    }";

            FeatureFlag featureFlagPayLoad = JsonConvert.DeserializeObject<FeatureFlag>(featureFlagData);
            featureFlagPayLoad.Name = featureFlagName;
            featureFlagPayLoad.Environment = environment;
            await flightingClient.CreateFeatureFlag(featureFlagPayLoad, app, environment);
        }

        public static async Task CreateFlagWithBusinessRuleEngine(TestContext _testContext, bool complementary = false)
        {
            FeatureFlagClient flightingClient = ClientCreator.CreateFeatureFlagClient(_testContext);
            string breFlagName = complementary 
                ? _testContext.Properties["FunctionalTest:FlagName:BRE:Complementary"].ToString()
                : _testContext.Properties["FunctionalTest:FlagName:BRE"].ToString();
            string environment = _testContext.Properties["FunctionalTest:Application:Environment"].ToString();
            string app = _testContext.Properties["FunctionalTest:Application"].ToString();
            string breWorkflowName = _testContext.Properties["FunctionalTest:BRE:Name"].ToString();
            string featureFlagData = @"
                    {   
                        'description': 'FunctionalTestingflagDescription',
                        'enabled': true,
                        'label': null,
                        'name': '',
                        'environment': '',
                        'conditions': {
                                    'client_Filters': [
                                        {
                                        'name': 'RulesEngine',
                                        'parameters': {
                                            'operator': 'Evaluates',
                                            'value': '',
                                            'isActive': 'true',
                                            'stageId': '1',
                                            'stageName': 'stg1',
                                            'flightContextKey': 'RulesEngine'
                                        }
                                    }
                            ]
                        }
            }";

            
            FeatureFlag featureFlagPayLoad = JsonConvert.DeserializeObject<FeatureFlag>(featureFlagData);
            featureFlagPayLoad.Name = breFlagName;
            featureFlagPayLoad.Environment = environment;
            featureFlagPayLoad.Conditions.Client_Filters[0].Parameters.Value = breWorkflowName;
            if (complementary)
                featureFlagPayLoad.Conditions.Client_Filters[0].Parameters.Operator = "NotEvaluates";
            await flightingClient.CreateFeatureFlag(featureFlagPayLoad, app, environment);
        }
    }
}
