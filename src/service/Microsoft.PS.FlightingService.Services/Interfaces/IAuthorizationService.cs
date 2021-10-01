using System.Threading.Tasks;
using Microsoft.PS.FlightingService.Common.AppExcpetions;

namespace Microsoft.PS.FlightingService.Services.Interfaces
{
    /// <summary>
    /// Service for authorizing requests
    /// </summary>
    public interface IAuthorizationService
    {
        /// <summary>
        /// Verifies that the calling principal has the required claims. If the required claims are missing then <see cref="AccessForbiddenException"/> is thrown
        /// </summary>
        /// <param name="appName">Tenant Name</param>
        /// <param name="operation">Operation under consideretion</param>
        /// <param name="correlationId">Correlation ID of the operation</param>
        /// <exception cref="AccessForbiddenException">When required claims are missing</exception>
        void EnsureAuthorized(string appName, string operation, string correlationId);
        
        /// <summary>
        /// Returns of the signed-in identity has the required claims
        /// </summary>
        /// <param name="appName">Tenant name</param>
        /// <returns>True if the required claims are present</returns>
        bool IsAuthorized(string appName);
        
        /// <summary>
        /// Creates the bearer token
        /// </summary>
        /// <param name="authority">IDP authority</param>
        /// <param name="clientId">AAD Client ID</param>
        /// <param name="clientSecret">AAD Client Secret</param>
        /// <param name="resourceId">AAD Client ID against which the token is acquired</param>
        /// <returns>Bearer token</returns>
        Task<string> GetAuthenticationToken(string authority, string clientId, string clientSecret, string resourceId);
        
        /// <summary>
        /// Augments the user identity with the required claims
        /// </summary>
        /// <param name="tenant">Flighting tenant name</param>
        void AugmentAdminClaims(string tenant);
    }
}
