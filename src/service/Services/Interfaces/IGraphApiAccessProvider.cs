using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.FeatureFlighting.Common;

namespace Microsoft.FeatureFlighting.Services.Interfaces
{
    public interface IGraphApiAccessProvider
    {
        Task<bool> IsUserUpnPartOfSecurityGroup(string userUpn, List<string> groupOid, LoggerTrackingIds trackingIds);
        Task<bool> IsUserAliasPartOfSecurityGroup(string userAlias, List<string> securityGroupIds, LoggerTrackingIds trackingIds);
    }
}
