namespace Microsoft.FeatureFlighting.Common.Config
{
    /// <summary>
    /// Configuration to connect to Azure Storage Blob
    /// </summary>
    public class StorageConfiguration
    {   
        /// <summary>
        /// Azure Key Vault secret name where account key is located
        /// </summary>
        public string StorageConnectionStringKey { get; set; }
        
        /// <summary>
        /// Key for connecting to the storage account
        /// </summary>
        public string StorageConnectionString { get; set; }
        
        /// <summary>
        /// Container where files are located
        /// </summary>
        public string ContainerName { get; set; }
    }
}
