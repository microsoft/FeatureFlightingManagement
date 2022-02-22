using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.FeatureFlighting.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Common.Cache;

namespace Microsoft.FeatureFlighting.API.Background
{
    public class RecacheHostedService : IHostedService
    {
        private readonly IBackgroundCacheManager _bgCacheManager;
        private readonly int _period;
        private Timer _timer = null!;

        public RecacheHostedService(IBackgroundCacheManager bgCacheManager, IConfiguration configuration)
        {
            _bgCacheManager = bgCacheManager;
            if (!int.TryParse(configuration["BackgroundCache:Period"], out _period))
                _period = 1;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _bgCacheManager.Init(_period);
            _timer = new Timer(Recache, null, TimeSpan.Zero, TimeSpan.FromMinutes(_period));
            return Task.CompletedTask;
        }

        private void Recache(object? state)
        {
            Task.Run(async () =>
            {
                LoggerTrackingIds trackingId = new(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
                await _bgCacheManager.Recache(trackingId, default);
            }).ConfigureAwait(false);

        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _bgCacheManager.Cleanup();
            return Task.CompletedTask;
        }
    }
}
