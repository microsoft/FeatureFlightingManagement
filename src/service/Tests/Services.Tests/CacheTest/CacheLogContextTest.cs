using Microsoft.FeatureFlighting.Infrastructure.Cache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.FeatureFlighting.Infrastructure.Tests.CacheTest
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class CacheLogContextTest
    {
        [TestMethod]
        public void GetMetadata_Success()
        {
            var result = CacheLogContext.GetMetadata("host", "delete", "1243dsadde23213");
            Assert.IsNotNull(result);
        }


    }
}
