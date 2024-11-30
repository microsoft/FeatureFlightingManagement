using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.FeatureFlighting.Common.Tests.ConfigTest
{
    [ExcludeFromCodeCoverage]
    [TestCategory("FlagEvaluationConfiguration")]
    [TestClass]
    public class FlagEvaluationConfigurationTest
    {
        [TestMethod]
        public void CalculateFlagValue_ShouldReturnCorrectValue()
        {
            // Act
            var result = FlagEvaluationConfiguration.GetDefault();

            // Assert
            Assert.IsNotNull(result);
        }
    }
}
