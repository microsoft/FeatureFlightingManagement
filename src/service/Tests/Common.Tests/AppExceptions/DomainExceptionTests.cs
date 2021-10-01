using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.PS.FlightingService.Common.AppExcpetions;

namespace Microsoft.PS.FlightingService.Common.Tests.AppExceptions
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class DomainExceptionTests
    {
        [TestMethod]
        public void DomainException_ShouldGetCreated()
        {
            #region Act
            var exception = new DomainException(message: Guid.NewGuid().ToString(),
                    exceptionCode: Guid.NewGuid().ToString(),
                    correlationId: Guid.NewGuid().ToString(),
                    transactionId: Guid.NewGuid().ToString(),
                    failedMethod: "Test");
            #endregion Act

            #region Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(Constants.Exception.Types.DOMAIN, exception.Type);
            Assert.AreEqual(string.Format(Constants.Exception.DomainException.DisplayMessage, exception.Message, exception.CorrelationId), exception.DisplayMessage);
            #endregion Assert
        }

        [TestMethod]
        public void DomainException_ShouldGetCreated_WithInnerException()
        {
            #region Act
            var exception = new DomainException(message: Guid.NewGuid().ToString(),
                    exceptionCode: Guid.NewGuid().ToString(),
                    correlationId: Guid.NewGuid().ToString(),
                    transactionId: Guid.NewGuid().ToString(),
                    failedMethod: "Test",
                    innerException: new Exception());
            #endregion Act

            #region Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(Constants.Exception.Types.DOMAIN, exception.Type);
            Assert.AreEqual(string.Format(Constants.Exception.DomainException.DisplayMessage, exception.Message, exception.CorrelationId), exception.DisplayMessage);
            #endregion Assert
        }
    }
}
