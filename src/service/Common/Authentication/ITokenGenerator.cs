using System.Threading.Tasks;

namespace Microsoft.FeatureFlighting.Common.Authentication
{
    /// <summary>
    /// Generates authentication token
    /// </summary>
    public interface ITokenGenerator
    {
        /// <summary>
        /// Generates token using AAD App credentials
        /// </summary>
        /// <param name="authority">Authority to generate the token</param>
        /// <param name="clientId">ID of the application for generating the token</param>
        /// <param name="clientSecret">Secret for the Client ID</param>
        /// <param name="resourceId">Resource ID for which the token is generated</param>
        /// <returns>Bearer token</returns>
        Task<string> GenerateToken(string authority, string clientId, /*string clientSecret,*/ string resourceId);
    }
}
