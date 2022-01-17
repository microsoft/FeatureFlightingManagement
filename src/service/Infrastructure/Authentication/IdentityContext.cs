using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Common.Authentication;

namespace Microsoft.FeatureFlighting.Infrastructure.Authentication
{
    internal class IdentityContext: IIdentityContext
    {
        private readonly bool _isUserImpersonationEnabled;
        private readonly string _impersonationHeader;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public IdentityContext(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _isUserImpersonationEnabled = bool.Parse(configuration["UserImpersonation:Enabled"]);
            _impersonationHeader =  configuration["UserImpersonation:ImpersonationHeader"];
            _httpContextAccessor = httpContextAccessor;
        }

        // <inheritdoc />
        public string? GetCurrentUserPrincipalName()
        {
            HttpContext httpContext = GetHttpContext();
            if (_isUserImpersonationEnabled
                && !string.IsNullOrWhiteSpace(_impersonationHeader)
                && httpContext?.Request?.Headers != null
                && httpContext.Request.Headers.ContainsKey(_impersonationHeader))
            {
                return httpContext.Request.Headers[_impersonationHeader].ToString();
            }

            return GetSignedInUserPrincipalName() ?? GetSignedInAppId();
        }

        // <inheritdoc />
        public string? GetSignedInUserPrincipalName()
        {
            HttpContext httpContext = GetHttpContext();
            string? upn = httpContext?.User?.Claims?.FirstOrDefault(claim => claim.Type.Contains("upn"))?.Value;
            if (string.IsNullOrWhiteSpace(upn))
                upn = httpContext?.User?.Claims?.FirstOrDefault(claim => claim.Type == ClaimTypes.Email)?.Value;
            if (string.IsNullOrWhiteSpace(upn))
                upn = httpContext?.User?.Claims?.FirstOrDefault(claim => claim.Type == ClaimTypes.Name)?.Value;
            return upn;
        }

        // <inheritdoc />
        public string? GetSignedInAppId()
        {
            HttpContext httpContext = GetHttpContext();
            return httpContext?.User?.Claims?.FirstOrDefault(claim => claim.Type.Contains("appid"))?.Value;
        }

        private HttpContext GetHttpContext() => _httpContextAccessor.HttpContext;
    }
}
