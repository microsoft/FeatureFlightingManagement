using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureFlighting.Core.FeatureFilters;
using Microsoft.FeatureManagement.FeatureFilters;
using Microsoft.FeatureManagement;
using Microsoft.FeatureFlighting.Core;
using Microsoft.FeatureFlighting.Core.Operators;
using Microsoft.FeatureFlighting.Core.Spec;
using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.FeatureFlighting.Common.Caching;
using Microsoft.FeatureFlighting.Core.RulesEngine;
using AppInsights.EnterpriseTelemetry.Web.Extension;
using AppInsights.EnterpriseTelemetry.Web.Extension.Middlewares;
using Microsoft.FeatureFlighting.Infrastructure.Graph;
using Microsoft.FeatureFlighting.Common.Group;
using Microsoft.FeatureFlighting.Core.AzureAppConfiguration;
using Microsoft.FeatureFlighting.Common.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.FeatureFlighting.Infrastructure.Cache;
using Microsoft.FeatureFlighting.Infrastructure.Authorization;
using Microsoft.FeatureFlighting.Infrastructure.Storage;
using Microsoft.FeatureFlighting.Common.Authorization;
namespace Microsoft.FeatureFlighting.SDK
{
    public static class FlightingSDKBuilder
    {

        public static IConfigurationBuilder AddAppConfiguration(this IConfigurationBuilder config, string AppConfigConString, string EnvironmentLabel)
        {
            return AddAzureAppConfig(config, AppConfigConString, EnvironmentLabel);
        }
        private static IConfigurationBuilder AddAzureAppConfig(IConfigurationBuilder config, string AppConfigConString, string EnvironmentLabel)
        {
            var configuration = config.Build();

            config.AddAzureAppConfiguration((options =>
            {
                options
                    .Connect(AppConfigConString)
                    .UseFeatureFlags(configure =>
                    {
                        configure.Label = (EnvironmentLabel);
                    });
            }));
            return config;
        }
        public static IServiceCollection AddFeatureConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IFlightingSDKFlagEvaluator, FlightingSDKFlagEvaluator>();
            AddInfrastructure(services, configuration);
            AddFeatureManagement(services);
            return services;
        }
        private static void AddInfrastructure(IServiceCollection services, IConfiguration configuration)
        {
            
            services.AddEnterpriseTelemetry(configuration);
            services.AddSingleton<ITenantConfigurationProvider, TenantConfigurationProvider>();
            services.AddSingleton<ICacheFactory, FlightingCacheFactory>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IAuthorizationService, AuthorizationService>();
            services.AddSingleton<IGroupVerificationService, GraphGroupVerificationService>();
            services.AddSingleton<IAzureConfigurationClientProvider, AzureConfigurationClientProvider>();
            services.AddSingleton<IBlobProviderFactory, BlobProviderFactory>();
        }
        private static void AddFeatureManagement(IServiceCollection services)
        {

            services.AddAzureAppConfiguration();
            services.AddSingleton<BaseOperator, EqualOperator>();
            services.AddSingleton<BaseOperator, NotEqualOperator>();
            services.AddSingleton<BaseOperator, InOperator>();
            services.AddSingleton<BaseOperator, NotInOperator>();
            services.AddSingleton<BaseOperator, LesserThanOperator>();
            services.AddSingleton<BaseOperator, GreaterThanOperator>();
            services.AddSingleton<BaseOperator, MemberOfSecurityGroupOperator>();
            services.AddSingleton<BaseOperator, NotMemberOfSecurityGroupOperator>();
            services.AddSingleton<IOperatorStrategy>(sp =>
                new OperatorStrategy(sp.GetServices<BaseOperator>(), sp.GetService<ITenantConfigurationProvider>(), sp.GetService<ICacheFactory>()));
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
                    //.AddFeatureFilter<RulesEngineFilter>();

            services.AddSingleton<IFeatureFlagEvaluator, FeatureFlagEvaluator>();
        }

    }
}
