using AppInsights.EnterpriseTelemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.API.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.FeatureFlighting.API.Tests.ControllerTests
{
    public class BaseClassExposedToTest:BaseController
    {
        public BaseClassExposedToTest(IConfiguration configuration, ILogger logger) : base(configuration, logger)
        {
        }

        public Tuple<string, string, string, string, string> GetHeaders()
        {
            return base.GetHeaders();
        }

        public string GetHeaderValue(string headerKey, string defaultValue)
        {
            return base.GetHeaderValue(headerKey, defaultValue);
        }

        public string GetHeaderValue(string headerName)
        {
            return base.GetHeaderValue(headerName);
        }
    }
}
