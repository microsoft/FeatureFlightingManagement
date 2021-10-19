using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;
using AppInsights.EnterpriseTelemetry;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using AppInsights.EnterpriseTelemetry.Context;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Services.Interfaces;
using System.Web;

namespace Microsoft.FeatureFlighting.Services
{
    public class CarbonFlightingService: IBackwardCompatibleFeatureManager
    {
        private readonly IAuthorizationService _authService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        public CarbonFlightingService(IAuthorizationService authService, IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger logger)
        {
            _authService = authService;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public bool IsBackwardCompatibityRequired(string componentName, string environment, string featureFlag)
        {   
            bool.TryParse(_configuration.GetValue<string>("BackwardCompatibleFlags:Enabled"), out bool isBackwardCompatibeEnabled);
            if (!isBackwardCompatibeEnabled)
                return false;
            if (featureFlag != HttpUtility.UrlEncode(featureFlag))
            {
                _logger.Log($"Feature Flag Name-{featureFlag} of application-{componentName} for Environment-{environment} is invaild for URL");
                return false;
            }
                
            var backwardCompatibleFlagsConfigKey = $"BackwardCompatibleFlags:{Regex.Replace(componentName.ToUpperInvariant(), @"[\W\s]+", "_")}:{environment.ToUpperInvariant()}";
            var backwardCompatibleFlags = _configuration.GetValue<string>(backwardCompatibleFlagsConfigKey)?.Split(',');
            return backwardCompatibleFlags != null 
                && backwardCompatibleFlags.Any(
                    flag => flag.ToLowerInvariant() == Constants.Flighting.ALL || 
                    flag.ToLowerInvariant() == featureFlag.ToLowerInvariant());
        }

        public async Task<Dictionary<string, bool>> IsEnabledAsync(string componentName, string environment, List<string> featureFlags, string flightContext)
        {
            var performanceContext = new PerformanceContext("Carbon Flighting API Time");
            componentName = componentName.ToUpperInvariant();
            environment = environment.ToUpperInvariant();
            var httpClientName = _configuration.GetValue<string>("CarbonFlightingService:Name");
            var httpClient = _httpClientFactory.CreateClient(httpClientName);
            var authorizationToken = await GetAuthToken();
            var url = CreateUrl(componentName, environment, featureFlags);
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Authorization", $"Bearer {authorizationToken}");
            request.Headers.Add(Constants.Flighting.FLIGHT_CONTEXT_HEADER, flightContext);
            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var serializedResponseResult = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<Dictionary<string, bool>>(serializedResponseResult);
            performanceContext.Stop();
            _logger.Log(performanceContext);
            return result;
        }

        private async Task<string> GetAuthToken()
        {
            return await _authService.GetAuthenticationToken(
                authority: _configuration.GetValue<string>("Authentication:Authority"),
                clientId: _configuration.GetValue<string>("Authentication:Audience"),
                clientSecret: _configuration.GetValue<string>("AuthenticationSecret"),
                resourceId: _configuration.GetValue<string>("CarbonFlightingService:AadResourceId"));
        }

        private string CreateUrl(string componentName, string environment, List<string> featureFlags)
        {
            var tenantName = GetBackwardCompaibleTenant(componentName);
            var url = _configuration.GetValue<string>("CarbonFlightingService:RelativeUrl")
                .Replace("{Tenant}", tenantName)
                .Replace("{Env}", environment)
                .Replace("{featureNames}", string.Join(",", featureFlags.Distinct()));        
            return url;
        }

        private string GetBackwardCompaibleTenant(string componentName)
        {
            var backwardCompatibleTenantKey = $"BackwardCompatibleFlags:TenantMapping:{Regex.Replace(componentName.ToUpperInvariant(), @"[\W\s]+", "_")}";
            return _configuration.GetValue<string>(backwardCompatibleTenantKey);
        }
    }
}
