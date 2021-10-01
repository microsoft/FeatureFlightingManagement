namespace Microsoft.PS.FlightingService.Common.Caching
{
    public class CachedUser
    {
        public CachedUser() { }

        public CachedUser(string objectId, string userPrincipalName)
        {
            ObjectId = objectId;
            UserPrincipalName = userPrincipalName;
        }

        public string ObjectId { get; set; }
        public string UserPrincipalName { get; set; }
    }
}
