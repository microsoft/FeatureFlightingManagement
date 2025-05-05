using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.FeatureFlighting.Common.Tests.ExtentionsTest
{
    [ExcludeFromCodeCoverage]
    [TestCategory(" StringExtensions")]
    [TestClass]
    public class StringExtensionsTest
    {
        [TestMethod]
        public void RemoveNonAscii_ShouldRemoveNonAsciiCharacters()
        {
            // Arrange
            string source = "tést stríng";

            // Act
            string result = source.RemoveNonAscii();

            // Assert
            Assert.AreEqual("tst strng", result);
        }

        [TestMethod]
        public void RemoveSpecialCharacters_ShouldRemoveSpecialCharacters()
        {
            // Arrange
            string source = "tést; stríng\\";

            // Act
            string result = source.RemoveSpecialCharacters();

            // Assert
            Assert.AreEqual("tst strng\\", result);
        }
    }
}
