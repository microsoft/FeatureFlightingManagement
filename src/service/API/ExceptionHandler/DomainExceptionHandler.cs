using System;
using System.Net;
using AppInsights.EnterpriseTelemetry;
using Microsoft.AspNetCore.Http;
using AppInsights.EnterpriseTelemetry.Context;
using Microsoft.FeatureFlighting.Common;
using AppInsights.EnterpriseTelemetry.Web.Extension.Middlewares;
using Microsoft.FeatureFlighting.Common.AppExceptions;

namespace Microsoft.FeatureFlighting.Api.ExceptionHandler
{
    public class DomainExceptionHandler : IGlobalExceptionHandler
    {
        private readonly ILogger _logger;

        public DomainExceptionHandler(ILogger logger)
        {
            _logger = logger;
        }

        public void Handle(Exception exception, HttpContext httpContext, string correlationId, string transactionId)
        {
            if (exception is DomainException)
            {
                var domainException = exception as DomainException;
                var exceptionContext = new ExceptionContext(domainException);
                _logger.Log(exceptionContext);

                if (httpContext.Response.HasStarted)
                    return;

                httpContext.Response.Clear();
                httpContext.Response.Headers.Add("x-error-code", domainException.ExceptionCode?.ToString());
                if (domainException.ExceptionCode.ToLowerInvariant() == Constants.Exception.DomainException.FlagDoesntExist.ExceptionCode.ToLowerInvariant())
                    httpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                else
                    httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                httpContext.Response.WriteAsync(domainException.Message).Wait();
            }
        }
    }
}
