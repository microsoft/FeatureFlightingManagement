using Moq;
using System;
using Newtonsoft.Json;
using System.Threading.Tasks;
using AppInsights.EnterpriseTelemetry;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.FeatureManagement;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.FeatureFlighting.Common.AppExceptions;
using Microsoft.FeatureFlighting.Core.FeatureFilters;
using static Microsoft.AspNetCore.Hosting.Internal.HostingApplication;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.FeatureFlighting.Core.Tests.FilterTests
{
    [ExcludeFromCodeCoverage]
    [TestCategory("CountryFilter")]
    [TestClass]
    public class CountryFilterTests : InitializeFilterTests
    {
        private Mock<IHttpContextAccessor> httpContextAccessorMockWithoutCountry;
        private Mock<IHttpContextAccessor> httpContextAccessorMockInDefinedCountry;
        private Mock<IHttpContextAccessor> httpContextAccessorMockInDefinedInt;
        private Mock<IHttpContextAccessor> httpContextAccessorMockNotInDefinedCountry;
        private FeatureFilterEvaluationContext featureContextOperatorIn;
        private FeatureFilterEvaluationContext featureContextOperatorNotIn;
        private FeatureFilterEvaluationContext featureContextOperatorEquals;
        private FeatureFilterEvaluationContext featureContextOperatorNotEquals;
        private FeatureFilterEvaluationContext featureContextOperatorGreaterThan;
        private FeatureFilterEvaluationContext featureContextOperatorLessThan;
        private Mock<ILogger> loggerMock;
        private readonly string countries = "India";
        private Mock<IConfiguration> configMock;

        [TestInitialize]
        public void TestStartup()
        {
            successfullMockEvaluatorStrategy = SetupMockOperatorEvaluatorStrategy(true);
            failureMockEvaluatorStrategy = SetupMockOperatorEvaluatorStrategy(false);
            
            httpContextAccessorMockWithoutCountry = SetupHttpContextAccessorMock(httpContextAccessorMockWithoutCountry, false, null);
            httpContextAccessorMockInDefinedCountry = SetupHttpContextAccessorMock(httpContextAccessorMockInDefinedCountry, true, "India");
            httpContextAccessorMockInDefinedInt = SetupHttpContextAccessorMock(httpContextAccessorMockInDefinedInt, true, "345");
            httpContextAccessorMockNotInDefinedCountry = SetupHttpContextAccessorMock(httpContextAccessorMockNotInDefinedCountry, true, "UK");

            featureContextOperatorIn = SetFilterContext(featureContextOperatorIn, Operator.In);
            featureContextOperatorNotIn = SetFilterContext(featureContextOperatorNotIn, Operator.NotIn);
            featureContextOperatorEquals = SetFilterContext(featureContextOperatorEquals, Operator.Equals);
            featureContextOperatorNotEquals = SetFilterContext(featureContextOperatorNotEquals, Operator.NotEquals);
            featureContextOperatorGreaterThan = SetFilterContext(featureContextOperatorGreaterThan, Operator.GreaterThan);
            featureContextOperatorLessThan = SetFilterContext(featureContextOperatorLessThan, Operator.LessThan);
            configMock = SetConfigMock(configMock);
            loggerMock = SetLoggerMock(loggerMock);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_True_If_Succeeds_Equals_Operator()
        {
            CountryFilter countryFilter = new CountryFilter(configMock.Object, httpContextAccessorMockInDefinedCountry.Object, loggerMock.Object, successfullMockEvaluatorStrategy.Object);
            featureContextOperatorEquals.Settings = countryFilter.BindParameters(featureContextOperatorEquals.Parameters);
            var featureFlagStatus = await countryFilter.EvaluateAsync(featureContextOperatorEquals);
            Assert.IsTrue(featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_False_If_Fails_Equals_Operator()
        {
            CountryFilter countryFilter = new CountryFilter(configMock.Object, httpContextAccessorMockNotInDefinedCountry.Object, loggerMock.Object, failureMockEvaluatorStrategy.Object);
            featureContextOperatorEquals.Settings = countryFilter.BindParameters(featureContextOperatorEquals.Parameters);
            var featureFlagStatus = await countryFilter.EvaluateAsync(featureContextOperatorEquals);
            Assert.IsFalse(featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_True_If_Succeeds_NotEquals_Operator()
        {
            CountryFilter countryFilter = new CountryFilter(configMock.Object, httpContextAccessorMockNotInDefinedCountry.Object, loggerMock.Object, successfullMockEvaluatorStrategy.Object);
            featureContextOperatorNotEquals.Settings = countryFilter.BindParameters(featureContextOperatorNotEquals.Parameters);
            var featureFlagStatus = await countryFilter.EvaluateAsync(featureContextOperatorNotEquals);
            Assert.IsTrue(featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_False_If_Fails_NotEquals_Operator()
        {
            CountryFilter countryFilter = new CountryFilter(configMock.Object, httpContextAccessorMockInDefinedCountry.Object, loggerMock.Object, failureMockEvaluatorStrategy.Object);
            featureContextOperatorNotEquals.Settings = countryFilter.BindParameters(featureContextOperatorNotEquals.Parameters);
            var featureFlagStatus = await countryFilter.EvaluateAsync(featureContextOperatorNotEquals);
            Assert.IsFalse(featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_True_If_Succeeds_In_Operator()
        {
            CountryFilter countryFilter = new CountryFilter(configMock.Object, httpContextAccessorMockInDefinedCountry.Object, loggerMock.Object, successfullMockEvaluatorStrategy.Object);
            featureContextOperatorIn.Settings = countryFilter.BindParameters(featureContextOperatorIn.Parameters);
            var featureFlagStatus = await countryFilter.EvaluateAsync(featureContextOperatorIn);
            Assert.IsTrue(featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_False_If_Fails_In_Operator()
        {
            CountryFilter countryFilter = new CountryFilter(configMock.Object, httpContextAccessorMockNotInDefinedCountry.Object, loggerMock.Object, failureMockEvaluatorStrategy.Object);
            featureContextOperatorIn.Settings = countryFilter.BindParameters(featureContextOperatorIn.Parameters);
            var featureFlagStatus = await countryFilter.EvaluateAsync(featureContextOperatorIn);
            Assert.IsFalse(featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_True_If_Succeeds_NotIn_Operator()
        {
            CountryFilter countryFilter = new CountryFilter(configMock.Object, httpContextAccessorMockNotInDefinedCountry.Object, loggerMock.Object, successfullMockEvaluatorStrategy.Object);
            featureContextOperatorNotIn.Settings = countryFilter.BindParameters(featureContextOperatorNotIn.Parameters);
            var featureFlagStatus = await countryFilter.EvaluateAsync(featureContextOperatorNotIn);
            Assert.IsTrue(featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_False_If_Fails_NotIn_Operator()
        {
            CountryFilter countryFilter = new CountryFilter(configMock.Object, httpContextAccessorMockInDefinedCountry.Object, loggerMock.Object, failureMockEvaluatorStrategy.Object);
            featureContextOperatorNotIn.Settings = countryFilter.BindParameters(featureContextOperatorNotIn.Parameters);
            var featureFlagStatus = await countryFilter.EvaluateAsync(featureContextOperatorNotIn);
            Assert.IsFalse(featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_For_GreaterThan_Operator()
        {
            Dictionary<string, string> filterSettings = new Dictionary<string, string>
            {
                { "IsActive", "true" },
                { "StageId", "1" },
                { "Value", "123" },
                { "Operator", nameof(Operator.GreaterThan) }
            };
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(filterSettings)
                .Build();

            FeatureFilterEvaluationContext context = new FeatureFilterEvaluationContext
            {
                Parameters = configuration
            };

            CountryFilter countryFilter = new CountryFilter(configMock.Object, httpContextAccessorMockInDefinedInt.Object, loggerMock.Object, successfullMockEvaluatorStrategy.Object);
            context.Settings = countryFilter.BindParameters(context.Parameters);
            var featureFlagStatus = await countryFilter.EvaluateAsync(context);
            Assert.IsTrue(featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_False_For_GreaterThan_Operator_For_Incorrect_Input()
        {
            Dictionary<string, string> filterSettings = new Dictionary<string, string>
            {
                { "IsActive", "true" },
                { "StageId", "1" },
                { "Value", "abc" },
                { "Operator", nameof(Operator.GreaterThan) }
            };
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(filterSettings)
                .Build();

            FeatureFilterEvaluationContext context = new FeatureFilterEvaluationContext
            {
                Parameters = configuration
            };

            CountryFilter countryFilter = new CountryFilter(configMock.Object, httpContextAccessorMockInDefinedInt.Object, loggerMock.Object, failureMockEvaluatorStrategy.Object);
            context.Settings = countryFilter.BindParameters(context.Parameters);
            var featureFlagStatus = await countryFilter.EvaluateAsync(context);
            Assert.IsFalse(featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_For_LessThan_Operator()
        {
            Dictionary<string, string> filterSettings = new Dictionary<string, string>
            {
                { "IsActive", "true" },
                { "StageId", "1" },
                { "Value", "123" },
                { "Operator", nameof(Operator.LessThan) }
            };
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(filterSettings)
                .Build();

            FeatureFilterEvaluationContext context = new FeatureFilterEvaluationContext
            {
                Parameters = configuration
            };

            CountryFilter countryFilter = new CountryFilter(configMock.Object, httpContextAccessorMockInDefinedInt.Object, loggerMock.Object, failureMockEvaluatorStrategy.Object);
            context.Settings = countryFilter.BindParameters(context.Parameters);
            var featureFlagStatus = await countryFilter.EvaluateAsync(context);
            Assert.IsFalse(featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_False_For_LessThan_Operator_For_Incorrect_Input()
        {
            Dictionary<string, string> filterSettings = new Dictionary<string, string>
            {
                { "IsActive", "true" },
                { "StageId", "1" },
                { "Value", "abc" },
                { "Operator", nameof(Operator.LessThan) }
            };
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(filterSettings)
                .Build();

            FeatureFilterEvaluationContext context = new FeatureFilterEvaluationContext
            {
                Parameters = configuration
            };

            CountryFilter countryFilter = new CountryFilter(configMock.Object, httpContextAccessorMockInDefinedInt.Object, loggerMock.Object, failureMockEvaluatorStrategy.Object);
            context.Settings = countryFilter.BindParameters(context.Parameters);
            var featureFlagStatus = await countryFilter.EvaluateAsync(context);
            Assert.IsFalse(featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_False_For_StageId_Being_Begative()
        {
            Dictionary<string, string> filterSettings = new Dictionary<string, string>
            {
                { "IsActive", "true" },
                { "StageId", "-10" },
                { "Value", "abc" },
                { "Operator", nameof(Operator.LessThan) }
            };
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(filterSettings)
                .Build();

            FeatureFilterEvaluationContext context = new FeatureFilterEvaluationContext
            {
                Parameters = configuration
            };

            CountryFilter countryFilter = new CountryFilter(configMock.Object, httpContextAccessorMockInDefinedInt.Object, loggerMock.Object, failureMockEvaluatorStrategy.Object);
            context.Settings = countryFilter.BindParameters(context.Parameters);
            var featureFlagStatus = await countryFilter.EvaluateAsync(context);
            Assert.IsFalse(featureFlagStatus);
        }

        [TestMethod]
        [ExpectedException(typeof(EvaluationException))]
        public async Task Feature_Filter_GetFilterSettings_Must_Evaluate_To_False()
        {
            CountryFilter countryFilter = new CountryFilter(configMock.Object, httpContextAccessorMockInDefinedInt.Object, loggerMock.Object, failureMockEvaluatorStrategy.Object);
            var featureFlagStatus = await countryFilter.EvaluateAsync(null);
            Assert.IsFalse(featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_False_If_Fails_GreaterThan_Operator()
        {
            CountryFilter countryFilter = new CountryFilter(configMock.Object, httpContextAccessorMockInDefinedCountry.Object, loggerMock.Object, failureMockEvaluatorStrategy.Object);
            featureContextOperatorLessThan.Settings = countryFilter.BindParameters(featureContextOperatorLessThan.Parameters);
            var featureFlagStatus = await countryFilter.EvaluateAsync(featureContextOperatorLessThan);
            Assert.IsFalse(featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Fail_For_GettingFlightingContext()
        {
            Mock<IHttpContextAccessor> httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            Dictionary<string, string> contextParams = new Dictionary<string, string>
            {
                { "country", "India" }
            };

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["testingFailure"] = JsonConvert.SerializeObject(contextParams);
            httpContext.Items[Constants.Flighting.FLIGHT_TRACKER_PARAM] = JsonConvert.SerializeObject(new LoggerTrackingIds()
            {
                CorrelationId = "TCId",
                TransactionId = "TTId"
            });
            httpContextAccessorMock.Setup(_ => _.HttpContext).Returns(httpContext);

            CountryFilter countryFilter = new CountryFilter(configMock.Object, httpContextAccessorMock.Object, loggerMock.Object, failureMockEvaluatorStrategy.Object);
            featureContextOperatorLessThan.Settings = countryFilter.BindParameters(featureContextOperatorLessThan.Parameters);
            var featureFlagStatus = await countryFilter.EvaluateAsync(featureContextOperatorLessThan);
            Assert.IsFalse(featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_False_If_HttpContext_Has_No_country()
        {
            CountryFilter countryFilter = new CountryFilter(configMock.Object, httpContextAccessorMockWithoutCountry.Object, loggerMock.Object, failureMockEvaluatorStrategy.Object);
            featureContextOperatorIn.Settings = countryFilter.BindParameters(featureContextOperatorIn.Parameters);
            var featureFlagStatus = await countryFilter.EvaluateAsync(featureContextOperatorIn);
            Assert.IsFalse(featureFlagStatus);
        }

        private Mock<IHttpContextAccessor> SetupHttpContextAccessorMock(Mock<IHttpContextAccessor> httpContextAccessorMock, bool hascountry, string country)
        {
            httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            Dictionary<string, string> contextParams = new Dictionary<string, string>();
            if (hascountry)
                contextParams.Add("country", country);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.Flighting.FLIGHT_CONTEXT_HEADER] = JsonConvert.SerializeObject(contextParams);
            httpContext.Items[Constants.Flighting.FLIGHT_TRACKER_PARAM] = JsonConvert.SerializeObject(new LoggerTrackingIds()
            {
                CorrelationId = "TCId",
                TransactionId = "TTId"
            });
            httpContextAccessorMock.Setup(_ => _.HttpContext).Returns(httpContext);

            return httpContextAccessorMock;
        }

        private FeatureFilterEvaluationContext SetFilterContext(FeatureFilterEvaluationContext context, Operator filterOperator)
        {
            Dictionary<string, string> filterSettings = new Dictionary<string, string>
            {
                { "IsActive", "true" },
                { "StageId", "1" },
                { "Value", countries }
            };

            switch (filterOperator)
            {
                case Operator.Equals:
                    filterSettings.Add("Operator", nameof(Operator.Equals));
                    break;
                case Operator.NotEquals:
                    filterSettings.Add("Operator", nameof(Operator.NotEquals));
                    break;
                case Operator.In:
                    filterSettings.Add("Operator", nameof(Operator.In));
                    break;
                case Operator.NotIn:
                    filterSettings.Add("Operator", nameof(Operator.NotIn));
                    break;
                case Operator.GreaterThan:
                    filterSettings.Add("Operator", nameof(Operator.GreaterThan));
                    break;
                case Operator.LessThan:
                    filterSettings.Add("Operator", nameof(Operator.LessThan));
                    break;
                default:
                    filterSettings.Add("Operator", nameof(Operator.Equals));
                    break;
            }
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(filterSettings)
                .Build();

            context = new FeatureFilterEvaluationContext
            {
                Parameters = configuration
            };
            return context;
        }
    }
}
