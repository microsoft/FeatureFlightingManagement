using System;
using System.Linq;
using System.Text;
using CQRS.Mediatr.Lite.SDK.Domain;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Common.Model;
using Microsoft.FeatureFlighting.Core.Optimizer;
using Microsoft.FeatureFlighting.Core.Domain.Events;
using Microsoft.FeatureFlighting.Common.AppExceptions;
using Microsoft.FeatureFlighting.Core.Domain.Assembler;
using Microsoft.FeatureFlighting.Core.Domain.ValueObjects;
using Microsoft.FeatureFlighting.Common.Model.AzureAppConfig;

namespace Microsoft.FeatureFlighting.Core.Domain
{
    internal class FeatureFlightAggregateRoot : AggregateRoot
    {
        public Feature Feature { get; private set; }
        public Status Status { get; private set; }
        public Tenant Tenant { get; private set; }
        public Settings Settings { get; private set; }
        public Condition Condition { get; private set; }
        public ValueObjects.Version Version { get; private set; }
        public Audit Audit { get; private set; }
        public Metric EvaluationMetrics { get; private set; }
        public Report Report { get; private set; }
        public AzureFeatureFlag ProjectedFlag { get; private set; }

        public FeatureFlightAggregateRoot(Feature feature,
            Status status,
            Tenant tenant,
            Settings settings,
            Condition condition,
            ValueObjects.Version version) : base(null)
        {
            Feature = feature;
            Status = status;
            Tenant = tenant;
            Settings = settings;
            Condition = condition;
            Version = version;
            _id = GetFlightId();
        }

        public FeatureFlightAggregateRoot(Feature feature,
            Status status,
            Tenant tenant,
            Settings settings,
            Condition condition,
            ValueObjects.Version version,
            Audit audit,
            Metric evaluationMetrics,
            Report report) : this(feature,
                status,
                tenant,
                settings,
                condition,
                version)
        {
            Audit = audit;
            EvaluationMetrics = evaluationMetrics;
            Report = report;
        }

        public void CreateFeatureFlag(IFlightOptimizer optimizer, string createdBy, string source, LoggerTrackingIds trackingIds)
        {
            Feature.Validate(trackingIds);
            Audit = new Audit(createdBy, System.DateTime.UtcNow, Status.Enabled);
            Version = new ValueObjects.Version();
            Status.UpdateActiveStatus(Condition);
            Condition.Validate(trackingIds);
            ProjectAzureFlag(optimizer, trackingIds);
            ApplyChange(new FeatureFlightCreated(this, trackingIds, source));
        }

        public void UpdateFeatureFlag(IFlightOptimizer optimizer, AzureFeatureFlag updatedFlag, string updatedBy, LoggerTrackingIds trackingIds, string source, out bool isUpdated)
        {
            FeatureFlightDto originalFlag = FeatureFlightDtoAssembler.Assemble(this);
            Status newStatus = new(updatedFlag.Enabled, updatedFlag.IsFlagOptimized);
            bool isStatusUpdated = false;
            if (Status.Enabled != newStatus.Enabled)
            {
                Status = newStatus;
                isStatusUpdated = true;
                if (Status.Enabled)
                    ApplyChange(new FeatureFlightEnabled(this, trackingIds, source));
                else
                    ApplyChange(new FeatureFlightDisabled(this, trackingIds, source));
                Audit.UpdateEnabledStatus(updatedBy, DateTime.UtcNow, Status.Enabled);
            }

            Condition newCondition = new(updatedFlag.IncrementalRingsEnabled, updatedFlag.Conditions);
            var (areStageSettingsUpdated, areStagesUpdated, areStagesAdded, areStagesDeleted) = Condition.Update(newCondition);
            Status.UpdateActiveStatus(Condition);

            isUpdated = isStatusUpdated || areStageSettingsUpdated || areStagesUpdated || areStagesAdded || areStagesDeleted;
            if (!isUpdated)
                return;
            Condition.Validate(trackingIds);
            if (areStagesAdded || areStagesDeleted)
                Version.UpdateMajor();
            else
                Version.UpdateMinor();

            ProjectAzureFlag(optimizer, trackingIds);
            string updateType = GetUpdateType(isStatusUpdated, areStageSettingsUpdated, areStagesUpdated, areStagesAdded, areStagesDeleted);
            Audit.Update(updatedBy, DateTime.UtcNow, updateType);
            ApplyChange(new FeatureFlightUpdated(this, originalFlag, updateType, trackingIds));
        }

        public void Enable(string enabledBy, IFlightOptimizer optimizer, LoggerTrackingIds trackingIds, string source, out bool isUpdated)
        {
            if (Status.Enabled)
            {
                isUpdated = false;
                return;
            }

            Status.Toggle();
            Status.UpdateActiveStatus(Condition);
            Version.UpdateMinor();
            Audit.UpdateEnabledStatus(enabledBy, DateTime.UtcNow, Status.Enabled);
            ProjectAzureFlag(optimizer, trackingIds);
            isUpdated = true;
            ApplyChange(new FeatureFlightEnabled(this, trackingIds, source));
        }

        public void Disable(string disabledBy, IFlightOptimizer optimizer, LoggerTrackingIds trackingIds, string source, out bool isUpdated)
        {
            if (!Status.Enabled)
            {
                isUpdated = false;
                return;
            }

            Status.Toggle();
            Status.UpdateActiveStatus(Condition);
            Version.UpdateMinor();
            Audit.UpdateEnabledStatus(disabledBy, DateTime.UtcNow, Status.Enabled);
            ProjectAzureFlag(optimizer, trackingIds);
            isUpdated = true;
            ApplyChange(new FeatureFlightDisabled(this, trackingIds, source));
        }

        public void ActivateStage(string stageName, string activatedBy, IFlightOptimizer optimizer, string source, LoggerTrackingIds trackingIds, out bool isStageActivated)
        {
            isStageActivated = false;
            Stage stage = Condition.Stages.FirstOrDefault(s => s.Name.ToLowerInvariant() == stageName.ToLowerInvariant());
            if (stage == null)
                throw new DomainException($"Stage with name {stageName} doesn't exist in flight for feature {Feature.Name}", "ACTIVATE_STAGE_001",
                    trackingIds.CorrelationId, trackingIds.TransactionId, "FeatureFlightAggregateRoot:ActivateStage");

            if (stage.IsActive && Condition.GetHighestActiveStage().Id == stage.Id)
                return;

            stage.Activate();

            foreach (Stage lowerStage in Condition.Stages.Where(s => s.Id < stage.Id))
            {
                if (Condition.IncrementalActivation)
                    lowerStage.Activate();
                else
                    lowerStage.Deactivate();
            }

            foreach (Stage higerStage in Condition.Stages.Where(s => s.Id > stage.Id))
            {
                higerStage.Deactivate();
            }
            isStageActivated = true;
            Version.UpdateMinor();
            Audit.Update(activatedBy, DateTime.UtcNow, "Stage Activated");
            ProjectAzureFlag(optimizer, trackingIds);
            ApplyChange(new FeatureFlightStageActivated(this, stageName, trackingIds, source));
        }

        public void Delete(string deletedBy, string source, LoggerTrackingIds trackingIds)
        {
            Audit.Update(deletedBy, DateTime.UtcNow, "Flight Deleted");
            ApplyChange(new FeatureFlightDeleted(this, trackingIds, source));
        }

        public void ReBuild(string triggeredBy, string reason, IFlightOptimizer optimizer, string source, LoggerTrackingIds trackingIds)
        {
            if (Audit == null)
                Audit = new("SYSTEM", DateTime.UtcNow, triggeredBy, DateTime.UtcNow, $"Flight Rebuild - {reason}");
            else
                Audit.Update(triggeredBy, DateTime.UtcNow, $"Flight Rebuild - {reason}");
            Version.UpdateMinor();
            ProjectAzureFlag(optimizer, trackingIds);
            ApplyChange(new FeatureFlightRebuilt(this, reason, trackingIds, source));
        }

        public void GenerateReport(string requestedBy, LoggerTrackingIds trackingIds)
        {
            DateTime now = DateTime.UtcNow;
            Status.UpdateActiveStatus(Condition);

            int activePeriod = 0;
            if (Report.Settings.VerifyActivationPeriod && Status.Enabled && Status.IsActive && Audit.EnabledOn != null)
            {
                activePeriod = (int)(now - Audit.EnabledOn).Value.TotalDays;
            }

            int inactivePeriod = 0;
            if (Report.Settings.VerifyDisabledPeriod && !Status.Enabled && Audit.DisabledOn != null)
            {
                inactivePeriod = (int)(now - Audit.DisabledOn).Value.TotalDays;
            }
            if (inactivePeriod == 0 && Report.Settings.VerifyDisabledPeriod && Status.Enabled && !Status.IsActive && Condition.FinalStage.DeactivatedOn != null)
            {
                inactivePeriod = (int)(now - Condition.FinalStage.DeactivatedOn).Value.TotalDays;
            }

            int unusedPeriod = 0;
            if (Report.Settings.VerifyUnusedPeriod && EvaluationMetrics != null && EvaluationMetrics.LastEvaluatedOn != null)
            {
                unusedPeriod = (int)(now - EvaluationMetrics.LastEvaluatedOn).Value.TotalDays;
            }

            int launchedPeriod = 0;
            if (Report.Settings.VerifyLaunchedPeriod && Condition != null && Condition.IsLaunched())
            {
                launchedPeriod = (int)(now - Condition.GetHighestActiveStage().ActivatedOn).Value.TotalDays;
            }

            int createdSince = (int)(now - Audit.CreatedOn).TotalDays;
            bool isNew = createdSince < 10;

            Report.UpdateStatus(isNew, activePeriod, inactivePeriod, unusedPeriod, launchedPeriod, requestedBy, now);
        }

        public void Subscribe(string unsubscribedBy, string source, LoggerTrackingIds trackingIds)
        {
            if (Report == null)
                return;

            if (Report.Settings.Status)
                throw new DomainException("Alerts for the flight is already enabled", "FLGT_ALERT_001", trackingIds.CorrelationId, trackingIds.TransactionId, "FeatureFlightAggregateRoot:Subscribe");

            Report.Settings.EnableAlerts();
            Audit.Update(unsubscribedBy, DateTime.UtcNow, "Enabled Alerts");
            ApplyChange(new FeatureFlightAlertsEnabled(this, trackingIds, source));
        }

        public void Unsubscribe(string unsubscribedBy, string source, LoggerTrackingIds trackingIds)
        {
            if (Report == null)
                return;

            if (!Report.Settings.Status)
                throw new DomainException("Alerts for the flight is already disabled", "FLGT_ALERT_002", trackingIds.CorrelationId, trackingIds.TransactionId, "FeatureFlightAggregateRoot:Unsubscribe");

            Report.Settings.DisableAlerts();
            Audit.Update(unsubscribedBy, DateTime.UtcNow, "Disabled Alerts");
            ApplyChange(new FeatureFlightAlertsDisabled(this, trackingIds, source));
        }

        public bool IsMetricsGenerated()
        {
            return EvaluationMetrics != null || EvaluationMetrics.CompletedOn > DateTime.MinValue;
        }

        public void UpdateEvaluationMetrics(EvaluationMetricsDto weeklyMetrics, string updatedBy, string source, LoggerTrackingIds trackingIds)
        {
            if (EvaluationMetrics == null)
                EvaluationMetrics = new(weeklyMetrics);

            EvaluationMetrics.Update(weeklyMetrics);
            Audit.Update(updatedBy, DateTime.UtcNow, "Metrics updated");
            ApplyChange(new FeatureFlightMetricsUpdated(this, trackingIds, source));
        }

        private string GetFlightId()
        {
            return new StringBuilder()
                .Append(Tenant.Id.ToLowerInvariant())
                .Append("_")
                .Append(Tenant.Environment.ToLowerInvariant())
                .Append("_")
                .Append(Feature.Name)
                .ToString();
        }

        private void ProjectAzureFlag(IFlightOptimizer optimizer, LoggerTrackingIds trackingIds)
        {
            ProjectedFlag = AzureFeatureFlagAssember.Assemble(this);
            if (Settings.EnableOptimization)
            {
                optimizer.Optmize(ProjectedFlag, Settings.OptimizationRules, trackingIds);
                Status.SetOptimizationStatus(ProjectedFlag.IsFlagOptimized);
            }
        }

        private string GetUpdateType(bool isStatusUpdated, bool areStageSettingsUpdated, bool areStagesUpdated, bool areStagesAdded, bool areStagesDeleted)
        {
            StringBuilder updateType = new();
            if (isStatusUpdated)
            {
                if (Status.Enabled)
                    updateType.Append("Flag Enabled");
                else
                    updateType.Append("Flag Disabled");
                updateType.Append(" | ");
            }

            if (areStageSettingsUpdated)
            {
                updateType.Append("Stage Settings Updated").Append(" | ");
            }

            if (areStagesUpdated)
            {
                updateType.Append("Filters changed").Append(" | ");
            }

            if (areStagesAdded)
            {
                updateType.Append("New Stage Added").Append(" | ");
            }

            if (areStagesDeleted)
            {
                updateType.Append("Existing Stage Deleted").Append(" | ");
            }

            return updateType.ToString()[..(updateType.Length - 3)];
        }
    }
}
