using System;
using System.Net;
using AppInsights.EnterpriseTelemetry;
using Microsoft.AspNetCore.Http;
using AppInsights.EnterpriseTelemetry.Context;
using AppInsights.EnterpriseTelemetry.Web.Extension.Middlewares;
using Microsoft.FeatureFlighting.Common.AppExceptions;

namespace Microsoft.FeatureFlighting.Api.ExceptionHandler
{
    public class AccessForbiddenExceptionHandler: IGlobalExceptionHandler
    {
        private readonly ILogger _logger;

        public AccessForbiddenExceptionHandler(ILogger logger)
        {
            _logger = logger;
        }

        public void Handle(Exception exception, HttpContext httpContext, string correlationId, string transactionId)
        {   
            if (exception is AccessForbiddenException)
            {
                var accessForbiddenException = exception as AccessForbiddenException;
                var exceptionContext = new ExceptionContext(accessForbiddenException);
                var accessForbiddenMetric = new MetricContext("AccessForbidden", 1);
                accessForbiddenMetric.AddProperty("Operation", accessForbiddenException.Operation);
                accessForbiddenMetric.AddProperty("Application", accessForbiddenException.Partner);

                _logger.Log(exceptionContext);
                _logger.Log(accessForbiddenMetric);

                if (httpContext.Response.HasStarted)
                    return;

                httpContext.Response.Clear();
                httpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                httpContext.Response.WriteAsync(accessForbiddenException.DisplayMessage).Wait();
            }
        }
    }
}
