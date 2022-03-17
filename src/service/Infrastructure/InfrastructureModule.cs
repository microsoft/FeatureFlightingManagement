using Autofac;
using Microsoft.FeatureFlighting.Common.Group;
using Microsoft.FeatureFlighting.Common.Caching;
using Microsoft.FeatureFlighting.Common.Storage;
using Microsoft.FeatureFlighting.Common.Webhook;
using Microsoft.FeatureFlighting.Common.AppConfig;
using Microsoft.FeatureFlighting.Infrastructure.Cache;
using Microsoft.FeatureFlighting.Infrastructure.Graph;
using Microsoft.FeatureFlighting.Common.Authorization;
using Microsoft.FeatureFlighting.Common.Authentication;
using Microsoft.FeatureFlighting.Infrastructure.Storage;
using Microsoft.FeatureFlighting.Infrastructure.Webhook;
using Microsoft.FeatureFlighting.Infrastructure.AppConfig;
using Microsoft.FeatureFlighting.Infrastructure.Authorization;
using Microsoft.FeatureFlighting.Infrastructure.Authentication;
using Microsoft.FeatureFlighting.Common.Cache;
using Autofac.Core;
using System.Collections.Generic;

namespace Microsoft.FeatureFlighting.Infrastructure
{
    /// <summary>
    /// <see cref="Module"/> for registering infrastructure dependencies
    /// </summary>
    public class InfrastructureModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            RegisterAppConfig(builder);
            RegisterAuthentication(builder);
            RegisterAuthorization(builder);
            RegisterCache(builder);
            RegisterGraph(builder);
            RegisterStorage(builder);
            RegisterWebhook(builder);
        }

        private void RegisterAppConfig(ContainerBuilder builder)
        {
            builder.RegisterType<AzureConfigurationClientProvider>()
                .As<IAzureConfigurationClientProvider>()
                .SingleInstance();

            builder.RegisterType<AzureFeatureManager>()
                .As<IAzureFeatureManager>()
                .SingleInstance();
        }

        private void RegisterAuthentication(ContainerBuilder builder)
        {
            builder.RegisterType<IdentityContext>()
                .As<IIdentityContext>()
                .SingleInstance();

            builder.RegisterType<AadTokenGenerator>()
                .As<ITokenGenerator>()
                .SingleInstance();
        }

        private void RegisterAuthorization(ContainerBuilder builder)
        {
            builder.RegisterType<AuthorizationService>()
                .As<IAuthorizationService>()
                .SingleInstance();
        }

        private void RegisterCache(ContainerBuilder builder)
        {
            builder.RegisterType<FlightingCacheFactory>()
                .As<ICacheFactory>()
                .SingleInstance();

            builder.RegisterType<BackgroundCacheManager>()
                .As<IBackgroundCacheManager>()
                .WithParameter(new ResolvedParameter(
                    (pi, ctx) => pi.ParameterType == typeof(IEnumerable<IBackgroundCacheable>),
                    (pi, ctx) => ctx.Resolve<IEnumerable<IBackgroundCacheable>>()
                    ));
        }

        private void RegisterGraph(ContainerBuilder builder)
        {
            builder.RegisterType<GraphGroupVerificationService>()
                .As<IGroupVerificationService>()
                .As<IBackgroundCacheable>()
                .SingleInstance();
        }

        private void RegisterStorage(ContainerBuilder builder)
        {
            builder.RegisterType<BlobProviderFactory>()
                .As<IBlobProviderFactory>()
                .SingleInstance();

            builder.RegisterType<FlightsDbRepositoryFactory>()
                .As<IFlightsDbRepositoryFactory>()
                .SingleInstance();
        }

        private void RegisterWebhook(ContainerBuilder builder)
        {
            builder.RegisterType<WebhookTriggerManager>()
                .As<IWebhookTriggerManager>()
                .SingleInstance();
        }
    }
}
