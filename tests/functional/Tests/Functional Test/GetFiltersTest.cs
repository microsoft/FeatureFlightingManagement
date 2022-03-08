using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.FeatureFlighting.Tests.Functional.Helper;

namespace Microsoft.FeatureFlighting.Tests.Functional.Functional_Test
{
    [TestClass]
    public class GetFiltersTest
    {
        private static TestContext _testContext;

        [ClassInitialize]
        public static void Setup(TestContext testContext)
        {
            _testContext = testContext;
        }

        [TestCategory("Functional")]
        [TestMethod]
        [Priority(4)]
        [Description("Test Case ID - 5676153")]
        public async Task Verify_Count_of_Filters()
        {
            //Arrange
            FeatureFlagClient flightingClient = ClientCreator.CreateFeatureFlagClient(_testContext);

            //Act
            var result = await flightingClient.GetFilters();
            
            //Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
            Assert.IsTrue(result.Count() >= 8);
        }


    }
}
