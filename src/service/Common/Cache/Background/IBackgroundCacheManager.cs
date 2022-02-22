using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.FeatureFlighting.Common.Cache
{
    public interface IBackgroundCacheManager
    {
        void Init(int period);
        Task Recache(LoggerTrackingIds trackingIds, CancellationToken cancellationToken);
        void Cleanup();
    }
}
