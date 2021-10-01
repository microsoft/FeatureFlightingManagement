using System;
using System.Net;
using AppInsights.EnterpriseTelemetry;
using Microsoft.AspNetCore.Http;
using AppInsights.EnterpriseTelemetry.Context;
using AppInsights.EnterpriseTelemetry.Web.Extension.Middlewares;
using Microsoft.PS.FlightingService.Common.AppExcpetions;

namespace Microsoft.PS.FlightingService.Api.ExceptionHandler
{
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
