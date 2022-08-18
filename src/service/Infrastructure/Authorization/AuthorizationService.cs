using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Identity.Client;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.FeatureFlighting.Common.AppExceptions;
using Microsoft.FeatureFlighting.Common.Authorization;

[assembly: InternalsVisibleTo("Microsoft.FeatureFlighting.Infrastructure.Tests")]

namespace Microsoft.FeatureFlighting.Infrastructure.Authorization
{   
    /// <inheritdoc/>
    internal class AuthorizationService : IAuthorizationService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITenantConfigurationProvider _tenantConfigurationProvider;
        private readonly IConfiguration _configuration;
        private readonly ConcurrentDictionary<string, IConfidentialClientApplication> _confidentialApps;
        private readonly string _adminClaimType;
        private readonly string _adminClaimValue;
        private readonly string _tenantAdminClaimValue;

        public AuthorizationService(IHttpContextAccessor httpContextAccessor, ITenantConfigurationProvider tenantConfigurationProvider, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _tenantConfigurationProvider = tenantConfigurationProvider;
            _configuration = configuration;

            _confidentialApps = new ConcurrentDictionary<string, IConfidentialClientApplication>();
            _adminClaimType = _configuration["Authorization:AdminClaimType"]?.ToLowerInvariant() ?? "Experimentation";
            _adminClaimValue = _configuration["Authorization:AdminClaimValue"]?.ToLowerInvariant() ?? "All";
            _tenantAdminClaimValue = _configuration["Authorization:TenantAdminClaimValue"]?.ToLowerInvariant() ?? "manageexperimentation";
        }

        /// <inheritdoc/>
        public void EnsureAuthorized(string appName, string operation, string correlationId)
        {
            var isAuthorized = IsAuthorized(appName);
            if (!isAuthorized)
                throw new AccessForbiddenException(appName, operation, correlationId);
        }

        /// <inheritdoc/>
        public bool IsAuthorized(string appName)
        {
            var adminClaims = _httpContextAccessor.HttpContext.User.Claims
                .Where(claim =>
                    claim.Type.ToLowerInvariant().Equals(_adminClaimType) &&
                    claim.Value.ToLowerInvariant().Equals(_adminClaimValue))
                .Select(claim => claim.Value.ToLowerInvariant())
                .ToList();

            if (adminClaims.Any())
                return true;

            var appAuthClaims = _httpContextAccessor.HttpContext.User.Claims
                .Where(claim => claim.Type.ToLowerInvariant().Equals(appName.ToLowerInvariant()))
                .Select(claim => claim.Value.ToLowerInvariant())
                .ToList();

            if (appAuthClaims.Contains(_tenantAdminClaimValue))
                return true;

            return false;
        }

        public async Task<string> GetAuthenticationToken(string authority, string clientId, string clientSecret, string resourceId)
        {
            AuthenticationResult authenticationResult;
            const string MsalScopeSuffix = "/.default";
            string bearerToken = null;
            try
            {
                IConfidentialClientApplication app = GetOrCreateConfidentialApp(authority, clientId, clientSecret);
                if (app != null)
                {
                    var scopes = new[] { resourceId + MsalScopeSuffix };
                    authenticationResult = await app.AcquireTokenForClient(scopes).ExecuteAsync();
                    bearerToken = authenticationResult.AccessToken;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return bearerToken;
        }

        private IConfidentialClientApplication GetOrCreateConfidentialApp(string authority, string clientId, string clientSecret)
        {
            string confidentialAppCacheKey = $"{authority}-{clientId}";
            if (_confidentialApps.ContainsKey(confidentialAppCacheKey))
            {
                return _confidentialApps[confidentialAppCacheKey];
            }
            IConfidentialClientApplication app =
                ConfidentialClientApplicationBuilder
                    .Create(clientId)
                    .WithClientSecret(clientSecret)
                    .WithAuthority(new Uri(authority))
                    .Build();
            _confidentialApps.TryAdd(confidentialAppCacheKey, app);
            return app;
        }

        public void AugmentAdminClaims(string tenant)
        {
            TenantConfiguration tenantConfiguration = _tenantConfigurationProvider.Get(tenant).ConfigureAwait(false).GetAwaiter().GetResult();
            List<string> administrators = tenantConfiguration.Authorization?.GetAdministrators()?.ToList() ?? new List<string>();
            
            var signedInIdentity = GetSignedInServicePrincipalIdentity();
            if (administrators.Contains(signedInIdentity, StringComparer.InvariantCultureIgnoreCase))
            {
                var adminClaim = new Claim(tenant.ToLowerInvariant(), _tenantAdminClaimValue);
                if (_httpContextAccessor.HttpContext.User.Identity is ClaimsIdentity identity)
                    identity.AddClaim(adminClaim);
            }
        }

        private string GetSignedInServicePrincipalIdentity()
        {
            var signedInUpn = GetSignedInUserPrincipalName();
            if (!string.IsNullOrWhiteSpace(signedInUpn))
                return signedInUpn;
            return GetSignedInAppId();
        }

        private string GetSignedInUserPrincipalName()
        {
            var upn = _httpContextAccessor.HttpContext?.User?.Claims.FirstOrDefault(claim => claim.Type.Contains("upn"))?.Value;
            if (string.IsNullOrWhiteSpace(upn))
                upn = _httpContextAccessor.HttpContext?.User?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name)?.Value;
            return upn;
        }

        private string GetSignedInAppId()
        {
            var appId = _httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(claim => claim.Type.Contains("appid"))?.Value;
            return appId;
        }
    }
}
