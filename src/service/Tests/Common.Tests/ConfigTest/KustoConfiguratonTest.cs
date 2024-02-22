using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.FeatureFlighting.Common.Tests.ConfigTest
{
    [ExcludeFromCodeCoverage]
    [TestCategory("KustoConfiguraton")]
    [TestClass]
    public class KustoConfiguratonTest
    {
        [TestMethod]
        public void SetDefault_ShouldCorrectlySetDefaultValues()
        {
            // Arrange
            var config = new KustoConfiguraton();

            // Act
            config.SetDefault();

            // Assert
            Assert.AreEqual("count_", config.Column_Count);
            Assert.AreEqual("Count", config.Column_Transformed_Count);
            Assert.AreEqual("avg_TimeTaken", config.Column_AvgTime);
            Assert.AreEqual("Average", config.Column_Transformed_AvgTime);
            Assert.AreEqual("percentile_TimeTaken_95", config.Column_P95);
            Assert.AreEqual("P95", config.Column_Transformed_P95);
            Assert.AreEqual("percentile_TimeTaken_90", config.Column_P90);
            Assert.AreEqual("P90", config.Column_Transformed_P90);
            Assert.AreEqual("timestamp", config.Column_Timestamp);
            Assert.AreEqual("Timestamp", config.Column_Transformed_Timestamp);
            Assert.AreEqual("user_Id", config.Column_UserId);
            Assert.AreEqual("userId", config.Column_Transformed_UserId);
        }
    }
}
