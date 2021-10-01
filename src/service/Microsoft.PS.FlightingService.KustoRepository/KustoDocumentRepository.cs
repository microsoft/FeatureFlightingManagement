using Kusto.Data;
using Kusto.Data.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.PS.FlightingService.KustoRepository.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Microsoft.PS.FlightingService.KustoRepository
{
    [ExcludeFromCodeCoverage]
    public class KustoDocumentRepository : IKustoDocumentRepository
    {
        readonly IConfiguration _configuration;
        public KustoDocumentRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private string GetKustoConnectionString(string clusterUrl, string token, string database)
        {
            if (token == null)
            {
                throw new Exception();
            }

            var connection = new KustoConnectionStringBuilder(clusterUrl)
            {
                FederatedSecurity = true,
                UserToken = token,
                InitialCatalog = database
            };
            return connection.ConnectionString;
        }

        private async Task<string> GetAccessTokenAsync()
        {
            var authority = string.Format(_configuration["Kusto:Authentication:Authority"], _configuration["Kusto:Authentication:TenantId"]);
            var clientId = _configuration["Kusto:Authentication:ClientId"];
            var clientSecret = _configuration["KustoAuthenticationClientSecret"];
            var appResourceId = _configuration["Kusto:Authentication:AppResourceId"];

            var authContext = new AuthenticationContext(authority);
            ClientCredential clientCredential = new ClientCredential(clientId, clientSecret);
            var authResult = await authContext.AcquireTokenAsync(appResourceId, clientCredential);

            return authResult.AccessToken;
        }

        public async Task<IList<string>> GetApplicationList(string query)
        {
            string token = await GetAccessTokenAsync();

            var connection = GetKustoConnectionString(_configuration["Kusto:Cluster:Url"], token, _configuration["Kusto:Cluster:Database"]);
            var client = KustoClientFactory.CreateCslQueryProvider(connection);

            IDataReader reader = null;
            try
            {
                reader = client.ExecuteQuery(query);
            }
            catch (Exception ex)
            {
                if (reader != null)
                {
                    reader.Dispose();
                }
                throw ex;
            }

            if (reader == null)
                return null;

            try
            {
                IList<string> applications = new List<string>();
                while (reader.Read())
                {
                    applications.Add(Convert.ToString(reader["ComponentName"].ToString()));
                }
                return applications;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                reader = null;
            }
        }
    }
}
