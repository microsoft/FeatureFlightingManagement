using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.FeatureFlighting.Infrastructure.Tests.GraphTest
{
    public interface IConfidentialClientApplicationBuilderWrapper
    {
        IConfidentialClientApplication Build(string clientId, string authority, string clientSecret);
    }

    public class ConfidentialClientApplicationBuilderWrapper
    {
        public IConfidentialClientApplication Build(string clientId, string authority, string clientSecret)
        {
            return ConfidentialClientApplicationBuilder
                .Create(clientId)
                .WithAuthority(authority)
                .WithClientSecret(clientSecret)
                .Build();
        }

    }
}
