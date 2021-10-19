using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Common;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.FeatureFlighting.Services.Interfaces;
using Microsoft.FeatureFlighting.Common.AppExcpetions;

namespace Microsoft.FeatureFlighting.Services
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly string _adminClaimType;
        private readonly string _adminClaimValue;
        private readonly string _tenantAdminClaimValue;

        public AuthorizationService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _configuration = configuration;

            _adminClaimType = _configuration.GetValue<string>("Authorization:AdminClaimType").ToLowerInvariant();
            _adminClaimValue = _configuration.GetValue<string>("Authorization:AdminClaimValue").ToLowerInvariant();
            _tenantAdminClaimValue = _configuration.GetValue<string>("Authorization:TenantAdminClaimValue").ToLowerInvariant();
        }

        public void EnsureAuthorized(string appName, string operation, string correlationId)
        {
            var isAuthorized = IsAuthorized(appName);
            if (!isAuthorized)
                throw new AccessForbiddenException(appName, operation, correlationId);
        }

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
            var authContext = new AuthenticationContext(authority);
            var credentials = new ClientCredential(clientId, clientSecret);
            var authResult = await authContext.AcquireTokenAsync(resourceId, clientCredential: credentials);
            return authResult.AccessToken;
        }

        public void AugmentAdminClaims(string tenant)
        {
            var administrators = _configuration.GetValue<string>($"Tenants:{Utility.GetFormattedTenantName(tenant)}:Authorization:Administrators")?.Split(',');
            if (administrators == null || !administrators.Any())
            {
                administrators = _configuration.GetValue<string>($"Tenants:Default:Authorization:Administrators")?.Split(',');
            }
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
