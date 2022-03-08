using System;
using System.Text;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;
using AppInsights.EnterpriseTelemetry;
using Microsoft.FeatureFlighting.Common;
using AppInsights.EnterpriseTelemetry.Context;
using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.FeatureFlighting.Common.Webhook;
using Microsoft.FeatureFlighting.Common.Authentication;
using Microsoft.FeatureFlighting.Common.Model.ChangeNotification;
using System.Collections.Generic;
using Microsoft.Graph;
using System.Linq;

namespace Microsoft.FeatureFlighting.Infrastructure.Webhook
{   
    // <inheritdoc/>
    internal class WebhookTriggerManager: IWebhookTriggerManager
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ITokenGenerator _tokenGenerator;
        private readonly ILogger _logger;

        public WebhookTriggerManager(IHttpClientFactory httpClientFactory, ITokenGenerator tokenGenerator, ILogger logger)
        {
            
            _httpClientFactory = httpClientFactory;
            _tokenGenerator = tokenGenerator;
            _logger= logger;
        }

        // <inheritdoc/>
        public Task<string> Trigger(WebhookConfiguration webhook, FeatureFlightChangeEvent @event, LoggerTrackingIds trackingIds)
        {
            string payload = JsonConvert.SerializeObject(@event);
            return Trigger(webhook, payload, null, trackingIds);
        }

        // <inheritdoc/>
        public async Task<string> Trigger(WebhookConfiguration webhook, string payload, Dictionary<string, string>? headers, LoggerTrackingIds trackingIds)
        {
            HttpClient client = _httpClientFactory.CreateClient(webhook.WebhookId);
            if (client.BaseAddress == null)
                client.BaseAddress = new Uri(webhook.BaseEndpoint);

            DependencyContext dependency = CreateDependencyContext(webhook, trackingIds);
            HttpRequestMessage request = new(new HttpMethod(webhook.HttpMethod), webhook.Uri ?? "");
            string bearerToken = await _tokenGenerator.GenerateToken(webhook.AuthenticationAuthority, webhook.ClientId, webhook.ClientSecret, webhook.ResourceId);
            request.Headers.Add("Authorization", $"Bearer {bearerToken}");
            request.Headers.Add("x-correlationId", trackingIds.CorrelationId);
            request.Headers.Add("x-messageId", trackingIds.TransactionId);

            if (headers != null && headers.Any())
            {
                foreach(KeyValuePair<string, string> header in headers)
                {
                    if (!request.Headers.Contains(header.Key))
                        request.Headers.Add(header.Key, header.Value);
                }
            }
            
            request.Content = new StringContent(payload, Encoding.UTF8, "application/json");
            dependency.RequestDetails = payload;

            HttpResponseMessage response = await client.SendAsync(request);
            string responseMessage = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                dependency.FailDependency(response.StatusCode.ToString(), responseMessage);
                _logger.Log(dependency);
                response.EnsureSuccessStatusCode();
            }

            dependency.CompleteDependency(response.StatusCode.ToString(), responseMessage);
            return responseMessage;
        }

        private DependencyContext CreateDependencyContext(WebhookConfiguration webhook, LoggerTrackingIds trackingIds)
        {
            return new DependencyContext(webhook.BaseEndpoint, webhook.BaseEndpoint, "HTTPS", true, "", trackingIds.CorrelationId, trackingIds.TransactionId, "WebhookTriggerManager:Trigger", "", "");
        }
    }
}
