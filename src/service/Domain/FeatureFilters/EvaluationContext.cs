using Microsoft.FeatureFlighting.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.FeatureFlighting.Core.FeatureFilters
{
    public class EvaluationContext
    {
        public Dictionary<string, object> FlagContext { get; set; }
        public string FlightingEnvironment { get; set; }
        public string FlightingApplication { get; set; }
        public LoggerTrackingIds TrackingIds { get; set; }
        public bool AddEnabledContext { get; set; } = false;
        public bool AddDisabledContext { get; set; } = false;


        public EvaluationContext(Dictionary<string, object> flagContext, string environment, string application, string correlationId, string transactionId, bool addEnabledContext =false, bool addDisabledContext=false)
        {
            this.TrackingIds =  new LoggerTrackingIds()
            {
                CorrelationId = correlationId!=String.Empty ? correlationId :new Guid().ToString(),
                TransactionId = transactionId != String.Empty ? transactionId : new Guid().ToString()
            };
            this.FlightingApplication = application;
            this.FlightingEnvironment = environment;
            this.FlagContext = flagContext;
            this.AddDisabledContext = addDisabledContext;
            this.AddEnabledContext = addEnabledContext;
        }
        public EvaluationContext(Dictionary<string, object> flagContext, string environment, string application, bool addEnabledContext = false, bool addDisabledContext = false)
        {
            this.TrackingIds = new LoggerTrackingIds() 
            { 
                CorrelationId = new Guid().ToString(),
                TransactionId = new Guid().ToString()
            };
            this.FlightingApplication = application;
            this.FlightingEnvironment = environment;
            this.FlagContext = flagContext;
        }
    }
}
