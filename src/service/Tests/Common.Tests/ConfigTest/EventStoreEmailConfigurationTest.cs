using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.FeatureFlighting.Common.Tests.ConfigTest
{
    [ExcludeFromCodeCoverage]
    [TestCategory(" EventStoreEmailConfiguration")]
    [TestClass]
    public class EventStoreEmailConfigurationTest
    {
        [TestMethod]
        public void SetDefaultEmailTemplates_ShouldSetDefaultValues_WhenValuesAreNull()
        {
            var config = new EventStoreEmailConfiguration();

            config.SetDefaultEmailTemplates();

            Assert.AreEqual("A new feature flight \"<<FeatureName>>\" has been created", config.FeatureFlightCreatedEmailSubject);
            Assert.AreEqual("email-feature-flight-created", config.FeatureFlightCreatedEmailTemplate);
            Assert.AreEqual("Feature flight \"<<FeatureName>>\" has been updated", config.FeatureFlightUpdatedEmailSubject);
            Assert.AreEqual("email-feature-flight-updated", config.FeatureFlightUpdatedEmailTemplate);
            Assert.AreEqual("Feature flight \"<<FeatureName>>\" has been enabled", config.FeatureFlightEnabledEmailSubject);
            Assert.AreEqual("email-feature-flight-enabled", config.FeatureFlightEnabledEmailTemplate);
            Assert.AreEqual("Feature flight \"<<FeatureName>>\" has been disabled", config.FeatureFlightDisabledEmailSubject);
            Assert.AreEqual("email-feature-flight-disabled", config.FeatureFlightDisabledEmailTemplate);
            Assert.AreEqual("Feature flight \"<<FeatureName>>\" has been deleted", config.FeatureFlightDeletedEmailSubject);
            Assert.AreEqual("email-feature-flight-deleted", config.FeatureFlightDeletedEmailTemplate);
            Assert.AreEqual("Alerts for Feature flight \"<<FeatureName>>\" has been enabled", config.FeatureFlightAlertsEnabledEmailSubject);
            Assert.AreEqual("email-feature-flight-alerts-enabled", config.FeatureFlightAlertsEnabledTemplate);
        }
    }
}

