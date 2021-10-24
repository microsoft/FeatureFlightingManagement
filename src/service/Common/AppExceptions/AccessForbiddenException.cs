using AppInsights.EnterpriseTelemetry.Exceptions;

namespace Microsoft.FeatureFlighting.Common.AppExceptions
{
    public class AccessForbiddenException: BaseAppException
    {   
        public string Partner { get; set; }
        public string Operation { get; set; }

        public override string Type => Constants.Exception.Types.ACCESS_FORBIDDEN;

        public AccessForbiddenException(string partner, string operation, string correlationId):
            base(string.Format(Constants.Exception.AccessForbiddenException.Message, operation, partner), correlationId: correlationId)
        { }

        protected override string CreateDisplayMessage() => string.Format(Constants.Exception.AccessForbiddenException.DisplayMessage, CorrelationId);
    }
}
