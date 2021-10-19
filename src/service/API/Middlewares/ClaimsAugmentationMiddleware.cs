using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Services.Interfaces;
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

        public async Task Invoke(HttpContext context, IConfiguration configuration, IAuthorizationService authorizationService)
        {
            string tenant = context.Request.Headers.GetOrDefault("X-Application", "Default");
            AuthorizationTypes authorizationType = GetAuthorizationType(tenant, configuration);
            if (authorizationType == AuthorizationTypes.Configuration)
            {
                authorizationService.AugmentAdminClaims(tenant);
            }
            await _next.Invoke(context);
        }

        private AuthorizationTypes GetAuthorizationType(string tenant, IConfiguration configuration)
        {
            var authorizationType = configuration.GetValue<string>($"Tenants:{Utility.GetFormattedTenantName(tenant)}:Authorization:Type")
                ?? configuration.GetValue<string>($"Tenants:Default:Authorization:Type");
            return Enum.Parse<AuthorizationTypes>(authorizationType);
        }
    }
}
