using System.Linq;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.FeatureManagement;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Domain;
using Microsoft.FeatureFlighting.Caching;
using Microsoft.FeatureFlighting.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement.FeatureFilters;
using Microsoft.FeatureFlighting.Common.Caching;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using AppInsights.EnterpriseTelemetry.Web.Extension;
using Microsoft.FeatureFlighting.Api.Controllers;
using Microsoft.FeatureFlighting.Api.Middlewares;
using Microsoft.FeatureFlighting.Domain.Evaluators;
using Microsoft.FeatureFlighting.Domain.Interfaces;
using Microsoft.FeatureFlighting.Services.Interfaces;
using Microsoft.FeatureFlighting.Domain.Configuration;
using Microsoft.FeatureFlighting.Api.ExceptionHandler;
using Microsoft.FeatureFlighting.Domain.FeatureFilters;
using AppInsights.EnterpriseTelemetry.Web.Extension.Filters;
using Microsoft.PS.Services.FlightingService.Api.Controllers;
using Microsoft.PS.Services.FlightingService.Api.ActionFilters;
using AppInsights.EnterpriseTelemetry.Web.Extension.Middlewares;

namespace Microsoft.PS.Services.FlightingService.Api
{
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            AddAuthentication(services);
            AddMvc(services);
            AddInfrastructure(services);
            AddFeatureManagement(services);
            AddSwagger(services);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Flighting Service V2");
                c.RoutePrefix = string.Empty;
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseAuthentication();
            app.UseMiddleware<MSALMiddleware>();
            app.UseMiddleware<ClaimsAugmentationMiddleware>();
            app.UseEnterpriseTelemetry(Configuration);

            app.UseAzureAppConfiguration();
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }

        private void AddAuthentication(IServiceCollection services)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                   .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                   {

                       options.Authority = Configuration["Authentication:Authority"];
                       var primaryAudience = Configuration["Authentication:Audience"];
                       IList<string> validAudiences = !string.IsNullOrWhiteSpace(Configuration["Authentication:AdditionalAudiences"])
                        ? Configuration["Authentication:AdditionalAudiences"].Split(',').ToList()
                        : new List<string>();
                       validAudiences.Add(primaryAudience);
                       options.TokenValidationParameters = new IdentityModel.Tokens.TokenValidationParameters
                       {
                           ValidAudiences = validAudiences
                       };
                   })
                    // Adding support for MSAL
                    .AddJwtBearer("MSAL", options =>
                    {
                        options.Authority = Configuration["Authentication:AuthorityV2"];
                        var primaryAudience = Configuration["Authentication:Audience"];
                        IList<string> validAudiences = !string.IsNullOrWhiteSpace(Configuration["Authentication:AdditionalAudiences"])
                         ? Configuration["Authentication:AdditionalAudiences"].Split(',').ToList()
                         : new List<string>();
                        validAudiences.Add(primaryAudience);
                        options.TokenValidationParameters = new IdentityModel.Tokens.TokenValidationParameters
                        {
                            ValidAudiences = validAudiences
                        };
                    });
        }

        private void AddMvc(IServiceCollection services)
        {
            services.AddSingleton<FeatureFlagsController, FeatureFlagsController>();
            services.AddSingleton<ProbeController, ProbeController>();

            services.AddMvc(options =>
            {
                options.Filters.Add(typeof(ValidateModelAttribute));
                options.Filters.Add<TrackingPropertiesFilterAttribute>();
            })
            .AddControllersAsServices();
        }

        private void AddInfrastructure(IServiceCollection services)
        {
            services.AddSingleton<IGlobalExceptionHandler, DomainExceptionHandler>();
            services.AddSingleton<IGlobalExceptionHandler, AccessForbiddenExceptionHandler>();
            services.AddSingleton<IGlobalExceptionHandler, GenericExceptionHandler>();
            services.AddEnterpriseTelemetry(Configuration);
            services.AddSingleton<ICacheFactory, FlightingCacheFactory>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IAuthorizationService, AuthorizationService>();
            services.AddSingleton<IGraphApiAccessProvider, GraphApiAccessProvider>();

            services.AddSingleton<IConfigurationClientProvider, ConfigurationClientProvider>();

            services.AddHttpClient(Configuration.GetValue<string>("CarbonFlightingService:Name"), client =>
            {
                client.BaseAddress = new System.Uri(Configuration.GetValue<string>("CarbonFlightingService:BaseUrl"));
            });
            services.AddSingleton<IBackwardCompatibleFeatureManager, CarbonFlightingService>();
        }

        private void AddFeatureManagement(IServiceCollection services)
        {
            services.AddAzureAppConfiguration();
            services.AddSingleton<BaseOperatorEvaluator, EqualEvaluator>();
            services.AddSingleton<BaseOperatorEvaluator, NotEqualEvaluator>();
            services.AddSingleton<BaseOperatorEvaluator, InEvaluator>();
            services.AddSingleton<BaseOperatorEvaluator, NotInEvaluator>();
            services.AddSingleton<BaseOperatorEvaluator, LesserThanEvaluator>();
            services.AddSingleton<BaseOperatorEvaluator, GreaterThanEvaluator>();
            services.AddSingleton<BaseOperatorEvaluator, MemberOfSecurityGroupEvaluator>();
            services.AddSingleton<BaseOperatorEvaluator, NotMemberOfSecurityGroupEvaluator>();
            services.AddSingleton<IOperatorEvaluatorStrategy>(factory =>
                new OperatorEvaluatorStrategy(factory.GetServices<BaseOperatorEvaluator>()));

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
                .AddFeatureFilter<GenericFilter>();

            services.AddSingleton<IFeatureFlagEvaluator, FeatureFlagEvaluator>();
            services.AddSingleton<IFeatureFlagManager, FeatureFlagManager>();
        }

        private void AddSwagger(IServiceCollection services)
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
            });
        }
    }
}
