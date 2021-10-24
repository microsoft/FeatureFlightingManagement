using System;
using System.Net;
using Microsoft.AspNetCore.Http;
using AppInsights.EnterpriseTelemetry;
using AppInsights.EnterpriseTelemetry.Context;
using AppInsights.EnterpriseTelemetry.Exceptions;
using Microsoft.FeatureFlighting.Common.AppExceptions;
using AppInsights.EnterpriseTelemetry.Web.Extension.Middlewares;

namespace Microsoft.FeatureFlighting.Api.ExceptionHandler
{
    /// <summary>
    /// Handles unhandled global exception
    /// </summary>
    public class GenericExceptionHandler : IGlobalExceptionHandler
    {
        private readonly ILogger _logger;

        public GenericExceptionHandler(ILogger logger)
        {
            _logger = logger;
        }

        public void Handle(Exception exception, HttpContext httpContext, string correlationId, string transactionId)
        {
            if ((exception is DomainException) || (exception is AccessForbiddenException))
                return;

            var baseException = exception is BaseAppException
                ? exception as BaseAppException
                : new GeneralException(exception);

            var exceptionContext = new ExceptionContext(baseException);
            _logger.Log(exceptionContext);

            if (httpContext.Response.HasStarted)
                return;

            httpContext.Response.Clear();
            httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            httpContext.Response.WriteAsync(baseException.DisplayMessage).Wait();
        }
    }
}
