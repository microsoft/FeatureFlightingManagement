using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Domain.Interfaces;
using Microsoft.FeatureFlighting.Services.Interfaces;
using Microsoft.FeatureFlighting.Domain.Configuration;
using Microsoft.FeatureFlighting.Common.AppExcpetions;
using Microsoft.PS.Services.FlightingService.Api.ActionFilters;

namespace Microsoft.PS.Services.FlightingService.Api.Controllers
{
    [Route("api/v1/featureflags")]
    [ApiController]
    [AspNetCore.Authorization.Authorize]
    public class FeatureFlagsController : ControllerBase
    {
        private readonly IFeatureFlagEvaluator _featureFlagEvaluator;
        private readonly IFeatureFlagManager _featureFlagManager;
        private readonly IAuthorizationService _authService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _config;

        public FeatureFlagsController(IFeatureFlagEvaluator featureFlagEvaluator, IFeatureFlagManager featureFlagManager, IAuthorizationService authService, IHttpContextAccessor httpContextAccesor, IConfiguration config)
        {
            _featureFlagManager = featureFlagManager;
            _featureFlagEvaluator = featureFlagEvaluator;
            _authService = authService;
            _httpContextAccessor = httpContextAccesor;
            _config = config;
        }

        [HttpGet]
        [Produces(typeof(Dictionary<string, bool>))]
        [ValidateModel]
        [Route("{appName}/{envName}/flighting")]
        [Route("/api/v1/{appName}/{envName}/flighting")]
        public async Task<IActionResult> EvaluateFeatureFlag_Backward([FromRoute] string appName, [FromRoute] string envName, [FromQuery] string featureNames)
        {
            _httpContextAccessor.HttpContext.Request.Headers.AddOrUpdate("x-application", appName);
            _httpContextAccessor.HttpContext.Request.Headers.AddOrUpdate("x-environment", envName);
            return await EvaluateFeatureFlag(appName, envName, featureNames);
        }

        [HttpGet]
        [Produces(typeof(Dictionary<string, bool>))]
        [ValidateModel]
        [Route("Evaluate")]
        [Route("/api/v1/Evaluate")]
        public async Task<IActionResult> EvaluateFeatureFlag([FromHeader(Name = "X-Application")] string appName, [FromHeader(Name = "X-Environment")] string envName, [FromQuery] string featureNames)
        {
            var trackingIds = GetLoggerTrackingIds();
            ValidateQueryRequests(appName, envName, new LoggerTrackingIds());
            IList<string> featureList;
            if (string.IsNullOrWhiteSpace(featureNames))
            {
                featureList = await _featureFlagManager.GetFeatures(appName, envName, trackingIds);
                if (featureList == null || !featureList.Any())
                {
                    return Ok(new Dictionary<string, bool>());
                }
            }
            else
            {
                featureList = featureNames.Split(',').ToList();
            }

            var flightResponse = await _featureFlagEvaluator.Evaluate(appName, envName, featureList.ToList());
            return Ok(flightResponse);
        }

        [HttpGet]
        [ValidateModel]
        [Route("{featureName}")]
        [Route("/api/v1/{featureName}")]
        public async Task<ActionResult> GetFeatureFlag([FromHeader(Name = "X-Application")] string appName, [FromHeader(Name = "X-Environment")] string envName, [FromRoute] string featureName)
        {
            var trackingIds = GetLoggerTrackingIds();
            ValidateQueryRequests(appName, envName, trackingIds);
            var flag = await _featureFlagManager.GetFeatureFlag(appName, envName, featureName, trackingIds);
            if (flag == null)
                return new NotFoundObjectResult(string.Format("Feature Flag '{0}' does not exist", featureName));
            return Ok(flag);
        }

        [HttpGet]
        [ValidateModel]
        [Route("/api/v1")]
        [Route("")]
        public async Task<ActionResult> GetFeatureFlags([FromHeader(Name = "X-Application")] string appName, [FromHeader(Name = "X-Environment")] string envName)
        {
            var trackingIds = GetLoggerTrackingIds();
            ValidateQueryRequests(appName, envName, trackingIds);
            var res = await _featureFlagManager.GetFeatureFlags(appName, envName, trackingIds);
            if (res == null || !res.Any())
                return new NotFoundObjectResult("Feature Flags do not exist for this Application");
            return Ok(res);
        }

        [HttpPost]
        [ValidateModel]
        [Route("/api/v1")]
        [Route("")]
        public async Task<IActionResult> CreateFeatureFlag([FromHeader(Name = "X-Application")] string appName, [FromHeader(Name = "X-Environment")] string envName, [FromBody] FeatureFlag featureFlag)
        {
            var trackingIds = GetLoggerTrackingIds();
            ValidateUpdateRequests(appName, envName, "CreateFeatureFlag", trackingIds);
            await _featureFlagManager.CreateFeatureFlag(appName.ToLowerInvariant(), envName.ToLowerInvariant(), featureFlag, trackingIds);
            return new CreatedAtActionResult("GetFeatureFlag", "FeatureFlags", new { featureName = featureFlag.Name }, new { });
        }

        [HttpPut]
        [ValidateModel]
        [Route("/api/v1")]
        [Route("")]
        public async Task<ActionResult> UpdateFeatureFlag([FromHeader(Name = "X-Application")] string appName, [FromHeader(Name = "X-Environment")] string envName, [FromBody] FeatureFlag featureFlag)
        {
            var trackingIds = GetLoggerTrackingIds();
            ValidateUpdateRequests(appName, envName, "UpdateFeatureFlag", trackingIds);
            await _featureFlagManager.UpdateFeatureFlag(appName.ToLowerInvariant(), envName.ToLowerInvariant(), featureFlag, trackingIds);
            return new NoContentResult();
        }

        [HttpPatch]
        [ValidateModel]
        [Route("{featureName}/enable")]
        [Route("/api/v1/{featureName}/enable")]
        public async Task<ActionResult> EnableFeatureFlag([FromHeader(Name = "X-Application")] string appName, [FromHeader(Name = "X-Environment")] string envName, [FromRoute] string featureName)
        {
            var trackingIds = GetLoggerTrackingIds();
            ValidateUpdateRequests(appName, envName, "EnableFeatureFlag", trackingIds);
            await _featureFlagManager.UpdateFeatureFlagStatus(appName.ToLowerInvariant(), envName.ToLowerInvariant(), new FeatureFlag(featureName), true, trackingIds);
            return new NoContentResult();
        }

        [HttpPatch]
        [ValidateModel]
        [Route("{featureName}/disable")]
        [Route("/api/v1/{featureName}/disable")]
        public async Task<ActionResult> DisableFeatureFlag([FromHeader(Name = "X-Application")] string appName, [FromHeader(Name = "X-Environment")] string envName, [FromRoute] string featureName)
        {
            var trackingIds = GetLoggerTrackingIds();
            ValidateUpdateRequests(appName, envName, "DisableFeatureFlag", trackingIds);
            await _featureFlagManager.UpdateFeatureFlagStatus(appName.ToLowerInvariant(), envName.ToLowerInvariant(), new FeatureFlag(featureName), false, trackingIds);
            return new NoContentResult();
        }

        [HttpPatch]
        [ValidateModel]
        [Route("{featureName}/activatestage/{stageName}")]
        [Route("/api/v1/{featureName}/activatestage/{stageName}")]
        public async Task<IActionResult> ActivateStage([FromHeader(Name = "X-Application")] string appName, [FromHeader(Name = "X-Environment")] string envName,
            [FromRoute] string featureName, [FromRoute] string stageName)
        {
            var trackingIds = GetLoggerTrackingIds();
            ValidateUpdateRequests(appName, envName, "ActivateStage", trackingIds);
            await _featureFlagManager.ActivateStage(appName, envName, new FeatureFlag(featureName), stageName, trackingIds);
            return NoContent();
        }

        [HttpDelete]
        [ValidateModel]
        [Route("{featureName}")]
        [Route("/api/v1/{featureName}")]
        public async Task<IActionResult> DeleteFeatureFlag([FromHeader(Name = "X-Application")] string appName, [FromHeader(Name = "X-Environment")] string envName, [FromRoute] string featureName)
        {
            var trackingIds = GetLoggerTrackingIds();
            ValidateUpdateRequests(appName, envName, "DeleteFeatureFlag", trackingIds);
            await _featureFlagManager.DeleteFeatureFlag(appName, envName, featureName, trackingIds);
            return NoContent();
        }

        private void ValidateHeaders(string appName, string envName)
        {
            if (string.IsNullOrEmpty(appName))
                ModelState.AddModelError("X-Application", "Invalid Application Header");

            if (string.IsNullOrEmpty(envName))
                ModelState.AddModelError("X-Environment", "Invalid Environment Header");

            if (!ValidateEnvironment(envName))
                ModelState.AddModelError("X-Environment", "Environment {0} is not supported");
        }

        private bool ValidateEnvironment(string envName)
        {
            var supportedEnvironments = _config.GetSection("Env:Supported").Value.Split(",");
            return supportedEnvironments.Any(supportedEnvironment => supportedEnvironment.ToLowerInvariant() == envName.ToLowerInvariant());
        }

        protected LoggerTrackingIds GetLoggerTrackingIds()
        {
            var correlationIdKey = _config["Application:CorrelationIdHeaderKey"];
            var correlationId = GetSingleHeaderValue(correlationIdKey);
            correlationId = string.IsNullOrEmpty(correlationId) ? Guid.NewGuid().ToString() : correlationId;

            var transactionIdKey = _config["Application:TransactionIdHeaderKey"];
            var transactionId = GetSingleHeaderValue(transactionIdKey);
            transactionId = string.IsNullOrEmpty(transactionId) ? Guid.NewGuid().ToString() : transactionId;

            var trackingIds = new LoggerTrackingIds()
            {
                CorrelationId = correlationId,
                TransactionId = transactionId
            };
            return trackingIds;
        }

        private string GetSingleHeaderValue(string headerKey)
        {
            if (string.IsNullOrEmpty(headerKey) || string.IsNullOrWhiteSpace(headerKey))
                return null;
            var headers = _httpContextAccessor.HttpContext.Request.Headers;
            if (headers.ContainsKey(headerKey))
            {
                var header = headers[headerKey];
                return header.FirstOrDefault();
            }
            return null;
        }

        private void ValidateUpdateRequests(string appName, string envName, string operation, LoggerTrackingIds trackingIds)
        {
            ValidateRequest(appName, envName, trackingIds);
            _authService.EnsureAuthorized(appName, operation, trackingIds.CorrelationId);
        }

        private void ValidateQueryRequests(string appName, string envName, LoggerTrackingIds trackingIds)
        {
            ValidateRequest(appName, envName, trackingIds);
        }

        private void ValidateRequest(string appName, string envName, LoggerTrackingIds trackingIds)
        {
            ValidateHeaders(appName, envName);
            if (!ModelState.IsValid)
            {
                var modelErrors = ModelState.Values.SelectMany(state => state.Errors.Select(error => error.ErrorMessage)).ToList();
                var message = string.Join(',', modelErrors);

                throw new DomainException(
                    message,
                    Constants.Exception.DomainException.RequestValidationFailed.ExceptionCode,
                    trackingIds.CorrelationId,
                    trackingIds.TransactionId,
                    "FeatureFlagController.ValidateRequest");
            }
        }
    }
}