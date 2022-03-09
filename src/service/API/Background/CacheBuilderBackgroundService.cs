using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.FeatureFlighting.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Common.Cache;
using AppInsights.EnterpriseTelemetry;
using AppInsights.EnterpriseTelemetry.Context;

namespace Microsoft.FeatureFlighting.API.Background
{
    /// <summary>
    /// Background service to rebuild cache
    /// </summary>
    public class CacheBuilderBackgroundService : IHostedService
    {
        private readonly IBackgroundCacheManager _bgCacheManager;
        private readonly bool _isBackgroundRefreshEnabled;
        private readonly int _period;
        private Timer _timer = null;
        private readonly ILogger _logger;
        
        /// <summary>
        /// Creates the service
        /// </summary>
        public CacheBuilderBackgroundService(IBackgroundCacheManager bgCacheManager, ILogger logger, IConfiguration configuration)
        {
            _bgCacheManager = bgCacheManager;
            if (!bool.TryParse(configuration["BackgroundCache:Enabled"], out _isBackgroundRefreshEnabled))
                _isBackgroundRefreshEnabled = false;
            if (!int.TryParse(configuration["BackgroundCache:Period"], out _period))
                _period = 5;
            _logger = logger;
        }

        /// <summary>
        /// Starts the background service
        /// </summary> 
        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (!_isBackgroundRefreshEnabled)
            {
                _logger.Log("Background caching is disabled");
                return Task.CompletedTask;
            }   

            _bgCacheManager.Init(_period);
            _timer = new Timer(RebuildCache, null, TimeSpan.Zero, TimeSpan.FromMinutes(_period));
            _logger.Log("Background caching hosting service started", source: "CacheBuilderHostingService:StartAsync");
            return Task.CompletedTask;
        }

        private void RebuildCache(object? state)
        {
            Task.Run(async () =>
            {
                LoggerTrackingIds trackingId = new(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
                await _bgCacheManager.RebuildCache(trackingId, default);
            }).ConfigureAwait(false);

        }

        /// <summary>
        /// Stops the background service
        /// </summary>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.Log("Background caching hosting service stopped", source: "CacheBuilderHostingService:StopAsync");
            _bgCacheManager.Dispose();
            return Task.CompletedTask;
        }
    }
}
