using System;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.FeatureFlighting.Tests.Functional.Helper;
using Microsoft.FeatureFlighting.Tests.Functional.Utilities;

namespace Microsoft.FeatureFlighting.Tests.Functional
{
    public class FeatureFlagClient
    {
        private static readonly HttpClient httpClient = new();
        private static TestContext _testContext;
        private static Dictionary<string, string> _tokenCache = new();

        public FeatureFlagClient(TestContext testContext)
        {
            _testContext = testContext;
        }

        public async Task<FeatureFlag> GetFeatureFlag(string featureName, string appName, string environment)
        {
            string route = FeatureControlConstants.FeatureFlagsRoute + "/" + featureName;
            HttpResponseMessage response = await SendRequestToFlightingService<FeatureFlag>(HttpMethod.Get, route, environment, appName, false, null);
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<FeatureFlag>(await response.Content.ReadAsStringAsync());
            }
            return new FeatureFlag
            {
                Id = response.StatusCode.ToString()
            };
        }

        public async Task<string> CreateFeatureFlag(FeatureFlag featureFlagPayload, string appName, string environment, bool useAlternateAccount = false)
        {
            string route = FeatureControlConstants.FeatureFlagsRoute;
            HttpResponseMessage response = await SendRequestToFlightingService(HttpMethod.Post, route, environment, appName, true, featureFlagPayload, null, useAlternateAccount);
            if (response.IsSuccessStatusCode)
            {
                if (response.StatusCode.ToString() == System.Net.HttpStatusCode.NoContent.ToString() || response.StatusCode.ToString() == System.Net.HttpStatusCode.Created.ToString())
                    return response.StatusCode.ToString();
                return await response.Content.ReadAsStringAsync();
            }
            return response.StatusCode.ToString();
        }

        public async Task<string> UpdateFeatureFlag(FeatureFlag featureFlagPayload, string appName, string environment, bool useAlternateAccount = false)
        {
            string route = FeatureControlConstants.FeatureFlagsRoute;
            HttpResponseMessage response = await SendRequestToFlightingService(HttpMethod.Put, route, environment, appName, true, featureFlagPayload, null, useAlternateAccount);
            return response.StatusCode.ToString();
        }

        public async Task<IList<FeatureFlagDTO>> GetFeatureFlags(string appName, string environment)
        {
            string route = FeatureControlConstants.FeatureFlagsRoute;
            HttpResponseMessage response = await SendRequestToFlightingService<FeatureFlag>(HttpMethod.Get, route, environment, appName, false, null);
            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<IList<FeatureFlagDTO>>(responseString);
            }
            return new List<FeatureFlagDTO>
            {
                new FeatureFlagDTO
                {
                    Id = response.StatusCode.ToString()
                }
            };
        }

        public async Task<string> ActivateStage(string appName, string environment, string featureName, string stageName)
        {
            string route = FeatureControlConstants.FeatureFlagsRoute + "/" + string.Format(FeatureControlConstants.ActivateStageRoute, featureName, stageName);
            HttpResponseMessage response = await SendRequestToFlightingService<FeatureFlag>(new HttpMethod("PATCH"), route, environment, appName, false, null);
            return response.StatusCode.ToString();
        }

        public async Task<string> EnableFeatureFlag(string appName, string environment, string featureName, bool useAlternateAccount = false)
        {
            string route = FeatureControlConstants.FeatureFlagsRoute + "/" + string.Format(FeatureControlConstants.EnableFeatureFlagRoute, featureName);
            HttpResponseMessage response = await SendRequestToFlightingService<FeatureFlag>(new HttpMethod("PATCH"), route, environment, appName, false, null, null, useAlternateAccount);
            return response.StatusCode.ToString();
        }

        public async Task<string> DisableFeatureFlag(string appName, string environment, string featureName, bool useAlternateAccount = false)
        {
            string route = FeatureControlConstants.FeatureFlagsRoute + "/" + string.Format(FeatureControlConstants.DisableFeatureFlagRoute, featureName);
            HttpResponseMessage response = await SendRequestToFlightingService<FeatureFlag>(new HttpMethod("PATCH"), route, environment, appName, false, null, null, useAlternateAccount);
            return response.StatusCode.ToString();
        }

        public async Task<string> DeleteFeatureFlag(string appName, string environment, string featureName, bool useAlternateAccount = false)
        {
            string route = FeatureControlConstants.FeatureFlagsRoute + "/" + string.Format(FeatureControlConstants.DeleteFeatureFlagRoute, featureName);
            HttpResponseMessage response = await SendRequestToFlightingService<FeatureFlag>(HttpMethod.Delete, route, environment, appName, false, null, null, useAlternateAccount);
            return response.StatusCode.ToString();
        }
        public async Task<Dictionary<string, bool>> Evaluate(string appName, string environment, List<string> featureFlags, string context)
        {
            string featureNames = string.Join(",", featureFlags);
            string route = FeatureControlConstants.FeatureFlagsRoute + "/" + string.Format(FeatureControlConstants.Evaluate, appName, environment) + featureNames;

            HttpResponseMessage response = await SendRequestToFlightingService<FeatureFlag>(HttpMethod.Get, route, environment, appName, false, null, context);
            if (response.IsSuccessStatusCode)
            {
                string result = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Dictionary<string, bool>>(result);
            }
            return new Dictionary<string, bool>
            {
                { response.StatusCode.ToString(), true }
            };
        }
        public async Task<Dictionary<string, bool>> EvaluateBackwards(string appName, string environment, List<string> featureFlags, string context)
        {
            string featureNames = string.Join(",", featureFlags);
            string route = FeatureControlConstants.FeatureFlagsRoute + "/" + FeatureControlConstants.Evaluate_Backwards + featureNames;
            HttpResponseMessage response = await SendRequestToFlightingService<FeatureFlag>(HttpMethod.Get, route, environment, appName, false, null, context);
            if (response.IsSuccessStatusCode)
            {
                string result = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Dictionary<string, bool>>(result);
            }
            return null;
        }

        public async Task<IList<string>> GetApplications()
        {
            string route = FeatureControlConstants.FeatureConfigurationRoute + FeatureControlConstants.GetAllApplicationsRoute;
            HttpResponseMessage response = await SendRequestToFlightingService<FeatureFlag>(HttpMethod.Get, route, null, null, false, null);
            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<IList<string>>(responseString);
            }
            return null;
        }
        public async Task<string[]> GetOperators()
        {
            string route = FeatureControlConstants.FeatureConfigurationRoute + FeatureControlConstants.GetOperatorsRoute;
            HttpResponseMessage response = await SendRequestToFlightingService<FeatureFlag>(HttpMethod.Get, route, null, null, false, null);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<string[]>(responseString);
            }

            return new string[0];
        }

        public async Task<string[]> GetFilters()
        {
            string route = FeatureControlConstants.FeatureConfigurationRoute + FeatureControlConstants.GetFiltersRoute;
            HttpResponseMessage response = await SendRequestToFlightingService<FeatureFlag>(HttpMethod.Get, route, null, null, false, null);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<string[]>(responseString);
            }

            return new string[0];
        }

        public async Task<Dictionary<string, List<string>>> GetFilterOperatorMapping()
        {
            string route = FeatureControlConstants.FeatureConfigurationRoute + FeatureControlConstants.FilterOperatorsMappingRoute;
            HttpResponseMessage response = await SendRequestToFlightingService<FeatureFlag>(HttpMethod.Get, route, null, null, false, null);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(responseString);
            }

            return null;
        }

        private async Task<HttpResponseMessage> SendRequestToFlightingService<T>(HttpMethod httpMethod, string route, string environment, string appName, bool hasPayload, T payload, string context = null, bool useAlternateAccount = false)
        {
            var flightingServiceUri = _testContext.Properties["FunctionalTest:FxpFlighting:Endpoint"].ToString();
            var authToken = useAlternateAccount ? await GetAccessTokenAsyncFromAlternateAccount(): await GetAccessTokenAsync();

            HttpRequestMessage request = new()
            {
                Method = httpMethod,
                RequestUri = new Uri(flightingServiceUri + route)
            };

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            request.Headers.Add(FeatureControlConstants.ApplicationHeader, appName);
            request.Headers.Add(FeatureControlConstants.EnvironmentHeader, environment);
            request.Headers.Add(FeatureControlConstants.FLIGHT_CONTEXT_HEADER, context);

            if (hasPayload)
            {
                string requestPayload = JsonConvert.SerializeObject(payload);
                request.Content = new StringContent(requestPayload);
                request.Content.Headers.Remove(FeatureControlConstants.ContentTypeHeader);
                request.Content.Headers.Add(FeatureControlConstants.ContentTypeHeader, FeatureControlConstants.JsonContentHeaderValue);
            }
            return await httpClient.SendAsync(request);
        }

        private static async Task<string> GetClientSecretForAuthentication()
        {
            if (_testContext.Properties["FunctionalTest:Environment"].ToString().ToLowerInvariant() != "Local".ToLowerInvariant())
            {
                return _testContext.Properties["FunctionalTest:AAD:ClientSecret"].ToString();
            }
            return await KeyVaultHelper.Instance.GetSecret(_testContext.Properties["FunctionalTest:KeyVault:Endpoint"].ToString(), _testContext.Properties["FunctionalTest:AAD:ClientSecret"].ToString());
        }

        private static async Task<string> GetAlternateClientSecretForAuthentication()
        {
            if (_testContext.Properties["FunctionalTest:Environment"].ToString().ToLowerInvariant() != "Local".ToLowerInvariant())
            {
                return _testContext.Properties["FunctionalTest:InvalidAAD:ClientSecret"].ToString();
            }
            return await KeyVaultHelper.Instance.GetSecret(_testContext.Properties["FunctionalTest:KeyVault:Endpoint"].ToString(), _testContext.Properties["FunctionalTest:InvalidAAD:ClientSecret"].ToString());
        }

        private static async Task<string> GetAccessTokenAsync()
        {
            string clientId = _testContext.Properties["FunctionalTest:AAD:ClientId"].ToString();
            string flightingKey = await GetClientSecretForAuthentication();
            string flightingResource = _testContext.Properties["FunctionalTest:FxpFlighting:AAD:ResourceId"].ToString();
            string authority = _testContext.Properties["FunctionalTest:AAD:Authority"].ToString();

            string cachedToken = _tokenCache.GetValueOrDefault(clientId, null);
            if (!string.IsNullOrWhiteSpace(cachedToken))
                return cachedToken;

            var authContext = new AuthenticationContext(authority);
            var client = clientId;
            var key = flightingKey;
            var credential = new ClientCredential(client, key);
            var authResult = await authContext.AcquireTokenAsync(flightingResource, credential);
            return authResult.AccessToken;
        }

        private static async Task<string> GetAccessTokenAsyncFromAlternateAccount()
        {
            string clientId = _testContext.Properties["FunctionalTest:InvalidAAD:ClientId"].ToString();
            string flightingKey = await GetAlternateClientSecretForAuthentication();
            string flightingResource = _testContext.Properties["FunctionalTest:FxpFlighting:AAD:ResourceId"].ToString();
            string authority = _testContext.Properties["FunctionalTest:AAD:Authority"].ToString();

            var authContext = new AuthenticationContext(authority);
            var client = clientId;
            var key = flightingKey;
            var credential = new ClientCredential(client, key);
            var authResult = await authContext.AcquireTokenAsync(flightingResource, credential);
            return authResult.AccessToken;
        }
    }
}
