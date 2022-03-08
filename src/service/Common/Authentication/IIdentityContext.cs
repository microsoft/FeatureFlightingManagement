namespace Microsoft.FeatureFlighting.Common.Authentication
{
    /// <summary>
    /// Identity Context
    /// </summary>
    public interface IIdentityContext
    {
        /// <summary>
        /// Gets the UPN of the current user. Under impersonation mode the UPN of the impersonated user is returned. Under app context, app id of the singed in app is returned
        /// </summary>
        /// <returns>UPN of the current user</returns>
        string? GetCurrentUserPrincipalName();

        /// <summary>
        /// Gets the User Principal Name of the logged in user
        /// </summary>
        /// <returns>UPN of Logged In User, null under app context</returns>
        string? GetSignedInUserPrincipalName();

        /// <summary>
        /// Gets the AAD Application ID of the logged-in App
        /// </summary>
        /// <returns>AAD App ID of the logged app, null under user context</returns>
        string? GetSignedInAppId();
    }
}
