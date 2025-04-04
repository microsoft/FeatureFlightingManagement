using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Text.Json.Serialization;
using Microsoft.FeatureFlighting.Core;
using System.Diagnostics.CodeAnalysis;
using Microsoft.FeatureFlighting.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureFlighting.API.Extensions;
using Microsoft.FeatureFlighting.Infrastructure;
using Microsoft.FeatureFlighting.API.Background;
using Microsoft.FeatureFlighting.Api.Controllers;
using Microsoft.FeatureFlighting.Api.Middlewares;
using Microsoft.FeatureFlighting.API.Controllers;
using AppInsights.EnterpriseTelemetry.Web.Extension;
using AppInsights.EnterpriseTelemetry.Web.Extension.Filters;
using Microsoft.Identity.ServiceEssentials.Extensions.AspNetCoreMiddleware;

namespace Microsoft.PS.Services.FlightingService.Api
{
    /// <summary>
    /// Application setup
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Application configuration
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Configures applicaton services
        /// </summary>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(Configuration);
            services.AddSwagger();
            services.AddTelememtry(Configuration);
            AddMvc(services);
            services.AddHttpClients(Configuration);
            services.AddFeatureFilters();

            services.AddHostedService<CacheBuilderBackgroundService>();
        }

        /// <summary>
        /// Configures the HTTP pipeline
        /// </summary>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Flighting Service V2");
                    c.RoutePrefix = string.Empty;
                });
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
            app.UseMise();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }

        /// <summary>
        /// Configures Autofac modules for DI
        /// </summary>
        public void ConfigureContainer(ContainerBuilder builder)
        {   
            builder.RegisterModule(new CommonModule());
            builder.RegisterModule(new InfrastructureModule());
            builder.RegisterModule(new CoreModule());
        }

        private void AddMvc(IServiceCollection services)
        {   
            services.AddMvc(options =>
            {
                options.Filters.Add<TrackingPropertiesFilterAttribute>();
            }).AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        }
    }
}
