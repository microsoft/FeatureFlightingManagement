using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.FeatureFlighting.Common.Authorization;
using static Microsoft.FeatureFlighting.Common.Constants.Authorization;

namespace Microsoft.FeatureFlighting.Api.Middlewares
{
    public class ClaimsAugmentationMiddleware
    {
        private readonly RequestDelegate _next;

        public ClaimsAugmentationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IAuthorizationService authorizationService, ITenantConfigurationProvider tenantConfigurationProvider)
        {
            string tenant = context.Request.Headers.GetOrDefault("X-Application", "Default");
            AuthorizationTypes authorizationType = GetAuthorizationType(tenant, tenantConfigurationProvider);
            if (authorizationType == AuthorizationTypes.Configuration)
            {
                authorizationService.AugmentAdminClaims(tenant);
            }
            await _next.Invoke(context);
        }

        private AuthorizationTypes GetAuthorizationType(string tenant, ITenantConfigurationProvider tenantConfigurationProvider)
        {
            TenantConfiguration tenantConfiguration = tenantConfigurationProvider.Get(tenant).ConfigureAwait(false).GetAwaiter().GetResult();
            var authorizationType = tenantConfiguration.Authorization.Type;
            return Enum.Parse<AuthorizationTypes>(authorizationType);
        }
    }
}
