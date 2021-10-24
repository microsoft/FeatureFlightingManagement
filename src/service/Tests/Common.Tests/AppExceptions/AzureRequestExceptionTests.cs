using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.FeatureFlighting.Common.AppExceptions;

namespace Microsoft.FeatureFlighting.Common.Tests.AppExceptions
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class AzureRequestExceptionTests
    {
        [TestMethod]
        public void AzureRequestException_ShouldGetCreated()
        {
            #region Act
            var exception = new AzureRequestException(message: Guid.NewGuid().ToString(),
                    statusCode: 500,
                    correlationId: Guid.NewGuid().ToString(),
                    transactionId: Guid.NewGuid().ToString(),
                    source: "Test");
            #endregion Act

            #region Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(Constants.Exception.Types.AZURERREQUESTEXCEPTION, exception.Type);
            Assert.AreEqual(string.Format(Constants.Exception.AzureRequestException.DisplayMessage, exception.CorrelationId), exception.DisplayMessage);
            #endregion Assert
        }

        [TestMethod]
        public void AzureRequestException_ShouldGetCreated_WithInnerException()
        {
            #region Act
            var exception = new AzureRequestException(message: Guid.NewGuid().ToString(),
                    statusCode: 500,
                    correlationId: Guid.NewGuid().ToString(),
                    transactionId: Guid.NewGuid().ToString(),
                    source: "Test",
                    innerException: new Exception());
            #endregion Act

            #region Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(Constants.Exception.Types.AZURERREQUESTEXCEPTION, exception.Type);
            Assert.AreEqual(string.Format(Constants.Exception.AzureRequestException.DisplayMessage, exception.CorrelationId), exception.DisplayMessage);
            #endregion Assert
        }
    }
}
