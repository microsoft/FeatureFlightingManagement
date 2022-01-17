using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.FeatureFlighting.Core;
using System.Diagnostics.CodeAnalysis;
using Microsoft.FeatureFlighting.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureFlighting.API.Extensions;
using Microsoft.FeatureFlighting.Infrastructure;
using Microsoft.FeatureFlighting.Api.Controllers;
using Microsoft.FeatureFlighting.Api.Middlewares;
using Microsoft.FeatureFlighting.API.Controllers;
using AppInsights.EnterpriseTelemetry.Web.Extension;
using AppInsights.EnterpriseTelemetry.Web.Extension.Filters;

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
            services.AddAuthentication(Configuration);
            services.AddSwagger();
            services.AddTelememtry(Configuration);
            AddMvc(services);
            services.AddHttpClients(Configuration);
            services.AddFeatureFilters();
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

        public void ConfigureContainer(ContainerBuilder builder)
        {   
            builder.RegisterModule(new CommonModule());
            builder.RegisterModule(new InfrastructureModule());
            builder.RegisterModule(new CoreModule());
        }

        private void AddMvc(IServiceCollection services)
        {
            services.AddSingleton<FeatureFlagsEvaluationController, FeatureFlagsEvaluationController>();
            services.AddSingleton<ProbeController, ProbeController>();

            services.AddMvc(options =>
            {   
                options.Filters.Add<TrackingPropertiesFilterAttribute>();
            }).AddControllersAsServices();
        }
    }
}
