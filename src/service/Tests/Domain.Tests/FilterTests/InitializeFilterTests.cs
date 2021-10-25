using Moq;
using System.Threading.Tasks;
using AppInsights.EnterpriseTelemetry;
using AppInsights.EnterpriseTelemetry.Context;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.FeatureFlighting.Core.Spec;
using Microsoft.FeatureFlighting.Core.Operators;
using Microsoft.FeatureFlighting.Core.FeatureFilters;

namespace Microsoft.FeatureFlighting.Core.Tests.FilterTests
{
    [TestClass]
    public class InitializeFilterTests
    {
        protected Mock<IOperatorStrategy> successfullMockEvaluatorStrategy;
        protected Mock<IOperatorStrategy> failureMockEvaluatorStrategy;

        protected Mock<ILogger> SetLoggerMock(Mock<ILogger> logger)
        {
            logger = new Mock<ILogger>();
            logger.Setup(m => m.Log(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
            logger.Setup(m => m.Log(It.IsAny<System.Exception>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
            logger.Setup(m => m.Log(It.IsAny<ExceptionContext>()));
            logger.Setup(m => m.Log(It.IsAny<MessageContext>()));
            logger.Setup(m => m.Log(It.IsAny<EventContext>()));
            logger.Setup(m => m.Log(It.IsAny<MetricContext>()));
            return logger;
        }
        protected Mock<IConfiguration> SetConfigMock(Mock<IConfiguration> configMock)
        {
            configMock = new Mock<IConfiguration>();
            var configurationSection = new Mock<IConfigurationSection>();
            configurationSection.Setup(a => a.Value).Returns("ENABLE:1");
            configMock.Setup(a => a.GetSection("FlightingDefaultContextParams:ContextParam")).Returns(configurationSection.Object);
            return configMock;
        }

        protected Mock<IOperatorStrategy> SetupMockOperatorEvaluatorStrategy(bool evaluatePositive)
        {
            var mockEvaluator = new Mock<BaseOperator>();
            mockEvaluator.Setup(evaluator => evaluator.Evaluate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<LoggerTrackingIds>()))
                .Returns(Task.FromResult(new EvaluationResult(evaluatePositive)));

            var mockOperatorEvaluatorStrategy = new Mock<IOperatorStrategy>();
            mockOperatorEvaluatorStrategy.Setup(strategy => strategy.Get(It.IsAny<Operator>()))
                .Returns(mockEvaluator.Object);
            return mockOperatorEvaluatorStrategy;
        }
    }
}
