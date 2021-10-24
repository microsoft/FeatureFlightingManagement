using System;
using AppInsights.EnterpriseTelemetry.Context;
using AppInsights.EnterpriseTelemetry.Exceptions;

namespace Microsoft.FeatureFlighting.Common.AppExceptions
{
    /// <summary>
    /// Exception when there is an error in connecting to Azure Storage
    /// </summary>
    [Serializable]
    public class StorageException : BaseAppException
    {
        public override string Type => Constants.Exception.Types.STORAGE;

        public string StorageAccount { get; set; }
        public string Container { get; set; }
        public string Blob { get; set; }

        public StorageException(string storageAccount, string container, string blob, Exception innerException, string source, string correlationId, string transactionId)
            :base(string.Format(Constants.Exception.StorageException.BlobFailureMessage, blob, container, storageAccount),
                 exceptionCode: Constants.Exception.StorageException.BlobExceptionCode,
                 innerException: innerException,
                 source: source,
                 correlationId: correlationId,
                 transactionId: transactionId)
        {
            StorageAccount = storageAccount;
            Container = container;
            Blob = blob;
        }

        public override ExceptionContext CreateLogContext()
        {
            ExceptionContext context = base.CreateLogContext();
            context.AddProperty(nameof(StorageAccount), StorageAccount);
            context.AddProperty(nameof(Container), Container);
            context.AddProperty(nameof(Blob), Blob);
            return context;
        }

        protected override string CreateDisplayMessage() =>
            string.Format(Constants.Exception.StorageException.DisplayMessage, CorrelationId);
    }
}
