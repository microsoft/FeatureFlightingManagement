using System;
using System.Linq;
using System.Collections.Generic;
using CQRS.Mediatr.Lite.SDK.Domain;
using Microsoft.FeatureFlighting.Common.Model;
using Microsoft.FeatureFlighting.Common.Model.AzureAppConfig;

namespace Microsoft.FeatureFlighting.Core.Domain.ValueObjects
{
    public class Stage: ValueObject
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public bool IsActive { get; private set; }
        public List<Filter> Filters { get; private set; } = new();
        public DateTime? ActivatedOn { get; private set; }
        public DateTime? DeactivatedOn { get; private set; }

        public Stage(int stageId, string stageName, bool isActive)
        {
            Id = stageId;
            Name = stageName;
            IsActive = isActive;
            Filters = new List<Filter>();
            UpdateAuditDates();
        }

        public Stage(StageDto stage)
        {
            Id = stage.StageId;
            Name = stage.StageName;
            IsActive = stage.IsActive;
            ActivatedOn = stage.LastActivatedOn;
            DeactivatedOn = stage.LastDeactivatedOn;
            if (stage.Filters == null || !stage.Filters.Any())
                return;

            Filters = stage.Filters.Select(filter => new Filter(filter)).ToList();
        }

        public void AddAzureFilter(AzureFilter azureFilter)
        {
            Filters.Add(new Filter(azureFilter));
        }

        public void Activate()
        {
            IsActive = true;
            ActivatedOn = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            DeactivatedOn = DateTime.UtcNow;
        }

        public bool TryUpdate(Stage updatedStage)
        {
            bool isUpdated = false;
            if (Name != updatedStage.Name)
            {
                Name = updatedStage.Name;
                isUpdated = true;
            }

            if (IsActive != updatedStage.IsActive)
            {
                isUpdated = true;
                IsActive = updatedStage.IsActive;
                UpdateAuditDates();
            }

            if (Filters.Count != updatedStage.Filters.Count)
            {
                isUpdated = true;
                Filters = updatedStage.Filters;
            }
            else
            {   
                bool areFiltersUpdated = !updatedStage.Filters.TrueForAll(updatedFilter => Filters.Any(filter => filter.Equals(updatedFilter)));
                if (areFiltersUpdated)
                {
                    Filters = updatedStage.Filters;
                    isUpdated = true;
                }
            }

            return isUpdated;
        }

        private void UpdateAuditDates()
        {
            ActivatedOn = IsActive ? DateTime.UtcNow : ActivatedOn;
            DeactivatedOn = !IsActive ? DateTime.UtcNow : DeactivatedOn;
        }
    }
}
