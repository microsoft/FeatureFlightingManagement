using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Common.AppExceptions;

namespace Microsoft.FeatureFlighting.API.Controllers
{
    [ApiController]
    [AspNetCore.Authorization.Authorize]
    public class BaseController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public BaseController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected Tuple<string, string, string, string> GetHeaders(bool validateHeaders = true)
        {
            string tenant = GetHeaderValue("x-application");
            string environment = GetHeaderValue("x-environment");
            string correlationId = GetHeaderValue("x-correlationId", Guid.NewGuid().ToString());
            string transactionId = GetHeaderValue("x-messageId", Guid.NewGuid().ToString());

            var headers = new Tuple<string, string, string, string>(tenant, environment, correlationId, transactionId);

            if (!validateHeaders)
                return headers;

            if (string.IsNullOrWhiteSpace(tenant))
                throw new DomainException("Tenant name is missing. Tenant name needs to be passed in the x-application header", "CNTRL_001",
                    correlationId, transactionId, "FeatureFlagsAdminController:GetHeaders");

            if (string.IsNullOrWhiteSpace(environment))
                throw new DomainException("Enviornment name is missing. Enviornment needs to be passed in the x-environment header", "CNTRL_002",
                    correlationId, transactionId, "FeatureFlagsAdminController:GetHeaders");

            ValidateEnvironment(environment, correlationId, transactionId);

            return headers;
        }

        private string GetHeaderValue(string headerKey, string defaultValue = default)
        {
            if (Request.Headers == null || !Request.Headers.Any(header => header.Key.ToLowerInvariant() == headerKey.ToLowerInvariant()))
                return defaultValue;
            return Request.Headers[headerKey].ToString();
        }

        private void ValidateEnvironment(string envName, string correlationId, string transactionId)
        {
            var supportedEnvironments = _configuration.GetSection("Env:Supported").Value.Split(",");
            bool isSupported = supportedEnvironments.Any(supportedEnvironment => supportedEnvironment.ToLowerInvariant() == envName.ToLowerInvariant());
            if (!isSupported)
                throw new DomainException("Provided environment is not supported", "CNTRL_003",
                    correlationId, transactionId, "FeatureFlagsAdminController:GetHeaders:ValidateEnvironment");
        }
    }
}
