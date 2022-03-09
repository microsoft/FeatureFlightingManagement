using Autofac;
using Microsoft.FeatureFlighting.Common.Config;

namespace Microsoft.FeatureFlighting.Common
{
    /// <summary>
    /// <see cref="Module"/> for registering common dependencies
    /// </summary>
    public class CommonModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<TenantConfigurationProvider>()
                .As<ITenantConfigurationProvider>()
                .SingleInstance();
        }
    }
}
