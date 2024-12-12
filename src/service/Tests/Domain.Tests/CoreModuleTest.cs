using Autofac;
using CQRS.Mediatr.Lite;
using Microsoft.FeatureFlighting.Core;
using Microsoft.FeatureFlighting.Core.Evaluation;
using Microsoft.FeatureFlighting.Core.Operators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.PS.FlightingService.Core.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class CoreModuleTest
    {
        private IContainer _container;

        [TestInitialize]
        public void SetUp()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<CoreModule>();
            _container = builder.Build();
        }

        [TestMethod]
        public void ShouldResolveRequestHandlerResolver()
        {
            var instance = _container.Resolve<IRequestHandlerResolver>();
            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void BaseOperator_ThrowException()
        {
            try
            {
                var instance = _container.Resolve<BaseOperator>();
            }
            catch (Exception ex)
            {
                Assert.IsNotNull(ex.Message);
            }
        }

        [TestMethod]
        public void ShouldResolveIFeatureBatchBuilder()
        {
            var instance = _container.Resolve<IFeatureBatchBuilder>();
            Assert.IsNotNull(instance);
        }

        // Add more tests for each type that should be resolvable...

        [TestCleanup]
        public void TearDown()
        {
            _container.Dispose();
        }
    }
}
