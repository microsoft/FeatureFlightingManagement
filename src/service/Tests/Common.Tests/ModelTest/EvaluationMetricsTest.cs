using Microsoft.FeatureFlighting.Common.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.FeatureFlighting.Common.Tests.ModelTest
{
    [ExcludeFromCodeCoverage]
    [TestCategory("EvaluationMetrics")]
    [TestClass]
    public class EvaluationMetricsTest
    {
        [TestMethod]
        public void GetDefault_ShouldReturnDefaultEvaluationMetricsDto()
        {
            // Act
            var defaultEvaluationMetricsDto = EvaluationMetricsDto.GetDefault();

            // Assert
            Assert.AreEqual(0, defaultEvaluationMetricsDto.EvaluationCount);
            Assert.AreEqual(0, defaultEvaluationMetricsDto.TotalEvaluations);
            Assert.AreEqual(0.0, defaultEvaluationMetricsDto.AverageLatency);
            Assert.AreEqual(0.0, defaultEvaluationMetricsDto.P95Latency);
            Assert.AreEqual(0.0, defaultEvaluationMetricsDto.P90Latency);
        }
    }
}
