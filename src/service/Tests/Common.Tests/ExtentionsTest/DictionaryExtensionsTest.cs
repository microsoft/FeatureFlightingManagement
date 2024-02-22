using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.FeatureFlighting.Common.Tests.ExtentionsTest
{
    [ExcludeFromCodeCoverage]
    [TestCategory("DictionaryExtensions")]
    [TestClass]
    public class DictionaryExtensionsTest
    {
        [TestMethod]
        public void Merge_ShouldMergeTwoDictionaries()
        {
            // Arrange
            var dictionary1 = new Dictionary<string, int> { { "key1", 1 }, { "key2", 2 } };
            var dictionary2 = new Dictionary<string, int> { { "key3", 3 }, { "key4", 4 } };

            // Act
            dictionary1.Merge(dictionary2);

            // Assert
            Assert.AreEqual(4, dictionary1.Count);
            Assert.IsTrue(dictionary1.ContainsKey("key1"));
            Assert.IsTrue(dictionary1.ContainsKey("key2"));
            Assert.IsTrue(dictionary1.ContainsKey("key3"));
            Assert.IsTrue(dictionary1.ContainsKey("key4"));
        }
    }
}
