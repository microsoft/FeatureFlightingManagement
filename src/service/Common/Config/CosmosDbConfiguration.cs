using Microsoft.Extensions.Configuration;

namespace Microsoft.FeatureFlighting.Common.Config
{
    /// <summary>
    /// Configuration to connect to Azure Cosmos DB
    /// </summary>
    public class CosmosDbConfiguration
    {
        public bool Disabled { get; set; }
        public string Endpoint { get; set; }
        public string PrimaryKeyLocation { get; set; }
        public string PrimaryKey { get; set; }
        public string DatabaseId { get; set; }
        public string ContainerId { get; set; }

        public void MergeWithDefault(CosmosDbConfiguration defaultConfiguration)
        {
            if (defaultConfiguration == null)
                return;
            
            if (Disabled)
                return;

            Endpoint = string.IsNullOrWhiteSpace(Endpoint) ? defaultConfiguration.Endpoint : Endpoint;
            PrimaryKeyLocation = string.IsNullOrWhiteSpace(PrimaryKeyLocation) ? defaultConfiguration.PrimaryKeyLocation : PrimaryKeyLocation;
            PrimaryKey = string.IsNullOrWhiteSpace(PrimaryKey) ? defaultConfiguration.PrimaryKey : PrimaryKey;
            DatabaseId = string.IsNullOrWhiteSpace(DatabaseId) ? defaultConfiguration.DatabaseId : DatabaseId;
            ContainerId = string.IsNullOrWhiteSpace(ContainerId) ? defaultConfiguration.ContainerId : ContainerId;
        }

        public void AddPrimaryKey(IConfiguration configuration)
        {
            PrimaryKey = configuration.GetValue<string>(PrimaryKeyLocation);
        }
    }

}
