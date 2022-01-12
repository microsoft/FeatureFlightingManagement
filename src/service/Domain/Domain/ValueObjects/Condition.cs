﻿using System;
using System.Linq;
using System.Collections.Generic;
using CQRS.Mediatr.Lite.SDK.Domain;
using Microsoft.FeatureFlighting.Common.Model;
using Microsoft.FeatureFlighting.Common.Model.AzureAppConfig;

namespace Microsoft.FeatureFlighting.Core.Domain.ValueObjects
{
    public class Condition : ValueObject
    {
        public bool IncrementalActivation { get; private set; }
        public List<Stage> Stages { get; private set; } = new();

        public Condition(bool isIncrementalEnabled, AzureFilterCollection filterCollection)
        {
            IncrementalActivation = isIncrementalEnabled;
            if (filterCollection == null || filterCollection.Client_Filters == null || !filterCollection.Client_Filters.Any())
                return;

            Stages = new List<Stage>();
            foreach (AzureFilter azureFilter in filterCollection.Client_Filters)
            {
                Stage stage = Stages.FirstOrDefault(s => s.Id.ToString() == azureFilter.Parameters.StageId);
                if (stage == null)
                {
                    stage = new Stage(int.Parse(azureFilter.Parameters.StageId), azureFilter.Parameters.StageName, bool.Parse(azureFilter.Parameters.IsActive));
                }
                stage.AddAzureFilter(azureFilter);
            }
        }

        public Condition(bool isIncrementalEnabled, List<StageDto> stages)
        {
            IncrementalActivation = isIncrementalEnabled;
            if (stages == null || !stages.Any())
            {
                Stages = new();
                return;
            }   

            Stages = stages.Select(stage => new Stage(stage)).ToList();
        }


        private Stage _initialStage;
        public Stage InitialStage
        {
            get
            {
                if (_initialStage != null)
                    return _initialStage;
                if (Stages == null || !Stages.Any())
                    return null;

                _initialStage = Stages.OrderBy(stage => stage.Id).First();
                return _initialStage;
            }
        }

        private Stage _finalStage;
        public Stage FinalStage
        {
            get
            {
                if (_finalStage != null)
                    return _finalStage;
                if (Stages == null || !Stages.Any())
                    return null;

                _finalStage = Stages.OrderByDescending(stage => stage.Id).First();
                return _finalStage;
            }
        }

        public bool IsActive()
        {
            return !IsEmpty()
                && Stages.Any(stage =>
                    stage.IsActive && stage.Filters != null && stage.Filters.Any());
        }

        public bool IsEmpty()
        {
            return Stages != null && Stages.Any();
        }

        public Tuple<bool, bool, bool, bool> Update(Condition updatedCondition)
        {
            bool areSettingsUpdated = false;
            if (updatedCondition.IncrementalActivation != IncrementalActivation)
            {
                IncrementalActivation = updatedCondition.IncrementalActivation;
                areSettingsUpdated = true;
            }

            var (areStagesUpdated, areStagesAdded, areStagesDeleted) = UpdateStages(updatedCondition);
            return new(areSettingsUpdated, areStagesUpdated, areStagesAdded, areStagesDeleted);
        }

        private Tuple<bool, bool, bool> UpdateStages(Condition updatedCondition)
        {   
            bool areStagesAdded = false;
            bool areStagesDeleted = false;
            bool areStagesUpdated = false;

            IEnumerable<Stage> newStages = updatedCondition.Stages.Where(updatedStage => !Stages.Any(stage => stage.Id == updatedStage.Id));
            IEnumerable<Stage> deletedStages = Stages.Where(stage => !updatedCondition.Stages.Any(updatedStage => updatedStage.Id == stage.Id));
            IEnumerable<Stage> updatedStages = Stages.Where(stage => updatedCondition.Stages.Any(updatedStage => updatedStage.Id == stage.Id));

            if (updatedStages.Any())
            {
                foreach (Stage updatedStage in updatedStages)
                {
                    areStagesUpdated = Stages.First(stage => stage.Id == updatedStage.Id).TryUpdate(updatedStage);
                }
            }

            if (newStages.Any())
            {
                Stages.AddRange(newStages.ToList());
                areStagesAdded = true;
            }

            if (deletedStages.Any())
            {
                areStagesDeleted = true;
                foreach (Stage deletedStage in deletedStages)
                {
                    Stages.Remove(deletedStage);
                }
            }

            return new(areStagesAdded, areStagesDeleted, areStagesUpdated);
        }

    }
}
