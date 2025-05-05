using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.FeatureFlighting.Common.Tests.ConfigTest
{
    [ExcludeFromCodeCoverage]
    [TestCategory(" TenantConfigurationComparer")]
    [TestClass]
    public class TenantConfigurationComparerTest
    {
        [TestMethod]
        public void Compare_Equals_GetHashCode_ShouldWorkCorrectly()
        {
            // Arrange
            var comparer = TenantConfigurationComparer.Default.Value;
            var tenant1 = new TenantConfiguration { Name = "Tenant1" };
            var tenant2 = new TenantConfiguration { Name = "Tenant2" };
            var tenant3 = new TenantConfiguration { Name = "Tenant1" };

            // Act
            var compareResult = comparer.Compare(tenant1, tenant2);
            var equalsResult1 = comparer.Equals(tenant1, tenant2);
            var equalsResult2 = comparer.Equals(tenant1, tenant3);
            var hashCode1 = comparer.GetHashCode(tenant1);
            var hashCode2 = comparer.GetHashCode(tenant3);

            // Assert
           
            Assert.IsFalse(equalsResult1);
            
            Assert.AreEqual(hashCode1, hashCode2);
        }

    }
}
