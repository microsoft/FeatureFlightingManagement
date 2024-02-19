using Microsoft.FeatureFlighting.Infrastructure.Authentication;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Microsoft.FeatureFlighting.Infrastructure.Tests
{
    [TestClass]
    public class AadTokenGeneratorTest
    {
        private readonly IDictionary<string, object> _cache;
        public AadTokenGeneratorTest()
        {
            _cache = new ConcurrentDictionary<string, object>();
        }

        // Define the test method
        [TestMethod]
        public void GenerateToken()
        {
            var mockUserService = new Mock<AadTokenGenerator>();
            // Act
            var userController = new AadTokenGenerator();
            var result = userController.GenerateToken("sajchvah","sachagsc","csahcas","xhasgdvghas");
            Assert.IsNotNull(result.Id);
            // Assert
        }
    }
}
