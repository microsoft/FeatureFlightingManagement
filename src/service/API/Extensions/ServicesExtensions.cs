using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.OpenApi.Models;
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
using Microsoft.Identity.ServiceEssentials.Extensions.AspNetCoreMiddleware;
using Microsoft.IdentityModel.S2S.Extensions.AspNetCore;
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
            services.AddMiseWithDefaultAuthentication(configuration, options =>
            {
                options.Authority= configuration["Authentication:Authority"];
                options.Audiences.Add(configuration["Authentication:Audience"]);
                options.ClientId = configuration["ClientInfo:ClientId"];
                options.Instance = configuration["InstanceInfo:Instance"];
                options.TenantId = configuration["TenantInfo:Tenant"];
            });

        }

        /// <summary>
        /// Adds Swagger documenation
        /// </summary>
        /// <remarks>
        /// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        /// </remarks>
        public static void AddSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Flighting Service",
                    Version = "v2",
                    Contact = new OpenApiContact
                    {
                        Email = "fxpswe@microsoft.com",
                        Name = "Field Experience Engineering Team",
                        Url = new System.Uri("https://aka.ms/fxpdocs")
                    },
                    Description = "APIs for managing and evaluating feature flags. Powered by Azure Configuration (Feature Management)"
                });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please insert JWT with Bearer into field",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement {
                    {
                        new OpenApiSecurityScheme()
                        {
                            Reference = new OpenApiReference()
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        System.Array.Empty<string>()
                    }
                });
                try
                {
                    string documentationFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    string documentationPath = Path.Combine(AppContext.BaseDirectory, documentationFile);
                    c.IncludeXmlComments(documentationPath);
                }
                catch 
                {
                    // Do nothing if documentation fails
                }
            });
            services.AddEndpointsApiExplorer();
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
