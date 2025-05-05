using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.FeatureFlighting.Infrastructure.Tests.GraphTest
{
    public interface IGraphServiceClientWrapper
    {
        Task<User> GetUser(string userId);
        // Add other methods as needed
    }

    public class GraphServiceClientWrapper
    {
        private readonly IGraphServiceClient _graphServiceClient;

        public GraphServiceClientWrapper(IGraphServiceClient graphServiceClient)
        {
            _graphServiceClient = graphServiceClient;
        }

        public Task<User> GetUser(string userId)
        {
            return _graphServiceClient.Users[userId].Request().GetAsync();
        }
        // Implement other methods as needed
    }
}

