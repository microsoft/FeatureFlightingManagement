using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.PS.FlightingService.Common;
using Microsoft.PS.FlightingService.Services.Interfaces;
using static Microsoft.PS.FlightingService.Common.Constants.Authorization;

namespace Microsoft.PS.FlightingService.Api.Middlewares
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
