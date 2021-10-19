using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.FeatureFlighting.Common.AppExcpetions;


namespace Microsoft.FeatureFlighting.Common.Tests.AppExceptions
{   
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class EvaluationExceptionTests
    {
        [TestMethod]
        public void EvaluationException_ShouldGetCreated()
        {
            #region Act
            var exception = new EvaluationException(message: Guid.NewGuid().ToString(),
                    correlationId: Guid.NewGuid().ToString(),
                    transactionId: Guid.NewGuid().ToString(),
                    failedMethod: Guid.NewGuid().ToString(),
                    innerException: new Exception());
            #endregion Act

            #region Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(Constants.Exception.Types.DOMAIN, exception.Type);
            Assert.AreEqual(Constants.Exception.EvaluationException.ExceptionCode, exception.ExceptionCode);
            Assert.AreEqual(string.Format(Constants.Exception.EvaluationException.DisplayMessage, exception.Message, exception.CorrelationId), exception.DisplayMessage);
            #endregion Assert
        }
    }
}
