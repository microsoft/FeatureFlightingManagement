using System.Threading.Tasks;
using System.Collections.Generic;

namespace Microsoft.FeatureFlighting.Common.Group
{
    /// <summary>
    /// Service to verify if an user belongs to a Group
    /// </summary>
    public interface IGroupVerificationService
    {   
        /// <summary>
        /// Checks if the user is part of any of the given groups
        /// </summary>
        /// <param name="userUpn">User Principal Name of the user</param>
        /// <param name="groupOids">IDs of the groups</param>
        /// <param name="trackingIds">Tracking Ids</param>
        /// <returns>True if the user is a member of any of the given groups</returns>
        Task<bool> IsMember(string userUpn, List<string> groupOids, LoggerTrackingIds trackingIds);

        Task<bool> IsUserAliasPartOfSecurityGroup(string userAlias, List<string> securityGroupIds, LoggerTrackingIds trackingIds);
    }
}
