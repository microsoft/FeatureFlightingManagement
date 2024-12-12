using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.FeatureFlighting.Common.Tests.ConfigTest
{
    [ExcludeFromCodeCoverage]
    [TestCategory("ParallelEvaluationConfiguration")]
    [TestClass]
    public class ParallelEvaluationConfigurationTest
    {
        [TestMethod]
        public void GetBatchSize_ShouldReturnCorrectBatchSize()
        {
            // Arrange
            var configZero = new ParallelEvaluationConfiguration { BatchSize = 0 };
            var configNegative = new ParallelEvaluationConfiguration { BatchSize = -1 };
            var configPositive = new ParallelEvaluationConfiguration { BatchSize = 5 };

            // Act
            var batchSizeZero = configZero.GetBatchSize();
            var batchSizeNegative = configNegative.GetBatchSize();
            var batchSizePositive = configPositive.GetBatchSize();

            // Assert
            Assert.AreEqual(1, batchSizeZero);
            Assert.AreEqual(int.MaxValue, batchSizeNegative);
            Assert.AreEqual(5, batchSizePositive);
        }
    }
}
