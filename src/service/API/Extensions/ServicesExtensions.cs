using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.FeatureManagement;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement.FeatureFilters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using AppInsights.EnterpriseTelemetry.Web.Extension;
using Microsoft.FeatureFlighting.Core.FeatureFilters;
using Microsoft.FeatureFlighting.Api.ExceptionHandler;
using AppInsights.EnterpriseTelemetry.Web.Extension.Middlewares;
using Microsoft.IdentityModel.Validators;
using Microsoft.Identity.ServiceEssentials;
using Microsoft.Identity.ServiceEssentials.Configuration;
using System.Configuration;


namespace Microsoft.FeatureFlighting.API.Extensions
{
    internal static class ServicesExtensions
    {
        /// <summary>
        /// Adds authentication to the pipeline
        /// </summary>
        public static void AddAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var primaryAudience = configuration["Authentication:Audience"];
            var clientId = configuration["ClientInfo:ClientId"];
            var tenantId = configuration["TenantInfo:Tenant"];
            var instance = configuration["InstanceInfo:Instance"];

            // Accept the configured audience and its api:// URI form (v1.0 and v2.0 tokens), plus any
            // extra audiences from Authentication:AdditionalAudiences (comma-separated), each in raw and api:// forms.
            var additionalAudiences = !string.IsNullOrWhiteSpace(configuration["Authentication:AdditionalAudiences"])
                ? configuration["Authentication:AdditionalAudiences"].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                : Array.Empty<string>();
            var validAudiences = new[] { primaryAudience, clientId, $"api://{primaryAudience}", $"api://{clientId}" }
                .Concat(additionalAudiences)
                .Concat(additionalAudiences.Select(a => $"api://{a}"))
                .Where(a => !string.IsNullOrWhiteSpace(a))
                .Distinct()
                .ToArray();

            // Inbound token validation via MISE 2.x with the default module set.
            services.AddAuthentication(MiseAuthenticationDefaults.AuthenticationScheme)
                .AddMiseWithDefaultModules(configuration, miseOptions =>
                {
                    miseOptions.AzureAd ??= new MiseAuthenticationOptions();
                    miseOptions.AzureAd.Instance = instance;
                    miseOptions.AzureAd.TenantId = tenantId;
                    miseOptions.AzureAd.ClientId = clientId;
                    miseOptions.AzureAd.Audience = primaryAudience;
                    miseOptions.AzureAd.Audiences = validAudiences;

                    // Explicit inbound policy accepting both app and user Bearer tokens.
                    // Required for MISE 2.x to validate inbound tokens (the default policy alone rejects them).
                    var inboundPolicy = new MiseInboundPolicyOptions
                    {
                        Label = "FeatureFlightingInbound",
                        Instance = instance,
                        TenantId = tenantId,
                        Audiences = validAudiences,
                    };

                    var bearerTokenTypeOptions = new TokenTypeOptions
                    {
                        AppToken = true,
                        UserToken = true,
                        AllowMissingIdTypeClaim = true,
                    };
                    var bearerProtocol = new ProtocolOptions();
                    bearerProtocol.TokenTypes["AccessToken"] = bearerTokenTypeOptions;
                    bearerProtocol.TokenTypes["AppToken"] = bearerTokenTypeOptions;
                    inboundPolicy.Protocols[Microsoft.Identity.ServiceEssentials.Authentication.Protocol.BearerConstants.ProtocolName] = bearerProtocol;

                    miseOptions.AzureAd.InboundPolicies = new List<MiseInboundPolicyOptions> { inboundPolicy };
                }, MiseAuthenticationDefaults.AuthenticationScheme);
        }

        /// <summary>
        /// Adds telemetry to API
        /// </summary>
        public static void AddTelememtry(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IGlobalExceptionHandler, DomainExceptionHandler>();
            services.AddSingleton<IGlobalExceptionHandler, AccessForbiddenExceptionHandler>();
            services.AddSingleton<IGlobalExceptionHandler, GenericExceptionHandler>();
            services.AddEnterpriseTelemetry(configuration);
        }

        public static void AddFeatureFilters(this IServiceCollection services)
        {
            services.AddAzureAppConfiguration();
            services.AddFeatureManagement()
                .AddFeatureFilter<AliasFilter>()
                .AddFeatureFilter<RoleGroupFilter>()
                .AddFeatureFilter<DateFilter>()
                .AddFeatureFilter<TimeWindowFilter>()
                .AddFeatureFilter<PercentageFilter>()
                .AddFeatureFilter<CountryFilter>()
                .AddFeatureFilter<RegionFilter>()
                .AddFeatureFilter<RoleFilter>()
                .AddFeatureFilter<UserUpnFilter>()
                .AddFeatureFilter<GenericFilter>()
                .AddFeatureFilter<RulesEngineFilter>();
        }

        public static void AddHttpClients(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            if (!string.IsNullOrWhiteSpace(configuration["EventStore:BaseEndpoint"]))
            {
                services.AddHttpClient(configuration["EventStore:WebhookId"], httpClient =>
                {
                    httpClient.BaseAddress = new System.Uri(configuration["EventStore:BaseEndpoint"]);
                });
            }

            if (!string.IsNullOrWhiteSpace(configuration["Kusto:Endpoint:BaseEndpoint"]))
            {
                services.AddHttpClient(configuration["Kusto:Endpoint:WebhookId"], httpClient =>
                {
                    httpClient.BaseAddress = new System.Uri(configuration["Kusto:Endpoint:BaseEndpoint"]);
                });
            }

            IConfigurationSection tenantConfigurationSection = configuration.GetSection("Tenants");
            IEnumerable<IConfigurationSection> tenantConfigurations = tenantConfigurationSection.GetChildren();

            foreach(IConfigurationSection tenantConfiguration in tenantConfigurations)
            {
                string changeSubscriptionWebhookId = tenantConfiguration["ChangeNotificationSubscription:Webhook:WebhookId"];
                if (!string.IsNullOrWhiteSpace(changeSubscriptionWebhookId) && changeSubscriptionWebhookId != configuration["EventStore:WebhookId"])
                {
                    services.AddHttpClient(tenantConfiguration["ChangeNotificationSubscription:Webhook:WebhookId"], httpClient =>
                    {
                        httpClient.BaseAddress = new System.Uri(configuration["EventStore:BaseEndpoint"]);
                    });
                }
            }
        }
    }
}
