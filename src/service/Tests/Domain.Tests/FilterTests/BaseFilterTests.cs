//using Microsoft.AspNetCore.Http;
//using Microsoft.Extensions.Configuration;
//using AppInsights.EnterpriseTelemetry;
//using Microsoft.FeatureFlighting.Core.FeatureFilters;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Moq;
//using System;

//namespace Microsoft.FeatureFlighting.Core.Tests.FilterTests
//{
//    [TestCategory("BaseFilter")]
//    [TestClass]
//    public class BaseFilterTests
//    {
//        private Mock<IHttpContextAccessor> httpContextAccessorMock = new Mock<IHttpContextAccessor>();
//        private Mock<ILogger> loggerMock = new Mock<ILogger>();
//        private Mock<IConfiguration> configMock = new Mock<IConfiguration>();
//        [TestInitialize]
//        public void TestStartup()
//        {
//        }

//        [TestMethod]
//        [ExpectedException(typeof(ArgumentNullException))]
//        public void BaseFilter_Constructor_Null_Input()
//        {
//            BaseFilter baseFilter = new BaseFilter(configMock.Object,null, loggerMock.Object);
//        }
//    }
//}
