using System;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Microsoft.FeatureFlighting.Common.Model
{
    /// <summary>
    /// Feature flighting usage report for a tenant
    /// </summary>
    public class UsageReportDto
    {
        /// <summary>
        /// Tenant Name
        /// </summary>
        public string Tenant { get; set; }

        /// <summary>
        /// Environment for which the report is generated
        /// </summary>
        public string Environment { get; set; }

        /// <summary>
        /// Maximum period for keeping an feature flight active
        /// </summary>
        public int ActivePeriodThreshold { get; set; }

        /// <summary>
        /// List of active features
        /// </summary>
        public List<string> ActiveFeatures { get; set; }
        
        /// <summary>
        /// Count of active features
        /// </summary>
        public int ActiveFeaturesCount => ActiveFeatures != null ? ActiveFeatures.Count : 0;

        /// <summary>
        /// List of features newly added in the tenant
        /// </summary>
        public List<string> NewlyAddedFeatures { get; set; }

        /// <summary>
        /// Count of newly added features
        /// </summary>
        public int NewAddedFeaturesCount => NewlyAddedFeatures != null ? NewlyAddedFeatures.Count : 0;

        /// <summary>
        /// Maximum period for keeping an feature flight inactive
        /// </summary>
        public int InactivePeriodThreshold { get; set; }

        /// <summary>
        /// List of disabled features
        /// </summary>
        public List<string> InactiveFeatures { get; set; }
        
        /// <summary>
        /// Count of disabled features
        /// </summary>
        public int InactiveFeaturesCount => InactiveFeatures != null ? InactiveFeatures.Count : 0;
        
        /// <summary>
        /// Maximum period for keeping an unused feature flight
        /// </summary>
        public int UnusedPeriodThreshold { get; set; }

        /// <summary>
        /// Total number of flag evaluations
        /// </summary>
        public int TotalEvaluations { get; set; }

        /// <summary>
        /// Formatted (comma-separated) total evaluations
        /// </summary>
        public string TotalEvaluationsDisplay => string.Format("{0:n0}", TotalEvaluations);

        /// <summary>
        /// List of features flights active for a long time
        /// </summary>
        public List<ThresholdExceededReportDto> LongActiveFeatures { get; set; }

        /// <summary>
        /// List of features flights disabled for a long time
        /// </summary>
        public List<ThresholdExceededReportDto> LongInactiveFeatures { get; set; }
        
        /// <summary>
        /// List of active feature flights that hasn't been executed for a long time
        /// </summary>
        public List<ThresholdExceededReportDto> UnusedFeatures { get; set; }

        /// <summary>
        /// Indicates if action is needed by tenant admin
        /// </summary>
        public bool PendingAction { get; set; }

        /// <summary>
        /// Describes the action needs to be taken by tenant admin
        /// </summary>
        public string PendingActionDescription { get; set; }

        /// <summary>
        /// UPN of the user requesting the report
        /// </summary>
        public string ReportRequestedBy { get; set; }
        
        /// <summary>
        /// Date time when the report was created
        /// </summary>
        public DateTime ReportCreatedOn { get; set; }

        /// <summary>
        /// Actionable card body
        /// </summary>
        public string FlightSelectorBody { get; set; }

        public UsageReportDto(string tenant, string environment, string requestedBy) 
        {
            Tenant = tenant;
            Environment = environment;
            ReportRequestedBy = requestedBy;
            ReportCreatedOn = DateTime.UtcNow;
        }

        public void CreateFlightSelectorBody()
        {
            Dictionary<string, string> flightSelector = new();
            foreach(ThresholdExceededReportDto unusedFlight in UnusedFeatures)
            {
                if (flightSelector.ContainsKey(unusedFlight.FeatureName))
                {
                    UnusedFeatures.Remove(unusedFlight);
                    continue;
                }
                flightSelector.Add(unusedFlight.FeatureName, $"{{{{{unusedFlight.FeatureName}.value}}}}");
            }

            foreach (ThresholdExceededReportDto inactiveFeature in LongInactiveFeatures)
            {
                if (flightSelector.ContainsKey(inactiveFeature.FeatureName))
                {
                    UnusedFeatures.Remove(inactiveFeature);
                    continue;
                }
                flightSelector.Add(inactiveFeature.FeatureName, $"{{{{{inactiveFeature.FeatureName}.value}}}}");
            }

            foreach (ThresholdExceededReportDto activeFeature in LongActiveFeatures)
            {
                if (flightSelector.ContainsKey(activeFeature.FeatureName))
                {
                    UnusedFeatures.Remove(activeFeature);
                    continue;
                }
                flightSelector.Add(activeFeature.FeatureName, $"{{{{{activeFeature.FeatureName}.value}}}}");
            }

            FlightSelectorBody = JsonConvert.SerializeObject(JsonConvert.SerializeObject(flightSelector));
        }

        public void UpdatePendingAction()
        {
            PendingAction = (UnusedFeatures != null && UnusedFeatures.Any())
                || (LongInactiveFeatures != null && LongInactiveFeatures.Any())
                || (LongActiveFeatures != null && LongActiveFeatures.Any());

            PendingActionDescription = PendingAction ? "Yes (See Below)" : "No pending action";
        }
    }
}
