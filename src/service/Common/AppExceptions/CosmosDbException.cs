using System;
using System.Text;
using AppInsights.EnterpriseTelemetry.Context;
using AppInsights.EnterpriseTelemetry.Exceptions;

namespace Microsoft.FeatureFlighting.Common.AppExceptions
{
    /// <summary>
    /// Exception when there is a error in Cosmos DB
    /// </summary>
    [Serializable]
    public class CosmosDbException : BaseAppException
    {
        public override string Type => nameof(CosmosDbException);
        
        public string ContainerName { get; set; }

        public CosmosDbException(Exception innerException, string containerName, string exceptionCode, string source, string correlationId, string transactionId)
            : base(message: $"Unhandled exception in Cosmos DB",
                 innerException: innerException,
                 exceptionCode: exceptionCode,
                 source: source,
                 correlationId: correlationId ?? Guid.NewGuid().ToString(),
                 transactionId: transactionId ?? Guid.NewGuid().ToString())
        {
            ContainerName = containerName;
        }

        public override ExceptionContext CreateLogContext()
        {
            ExceptionContext context = base.CreateLogContext();
            context.AddProperty(nameof(ContainerName), ContainerName);
            return context;
        }

        protected override string CreateDisplayMessage()
        {
            StringBuilder messageBuilder = new();
            messageBuilder.Append("OOPS! Some error ocurred in the database. Please contact support with Correlation ID");
            messageBuilder.Append(CorrelationId);
            return messageBuilder.ToString();
        }
    }
}
