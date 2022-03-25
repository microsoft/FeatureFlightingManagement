using System;
using System.Collections.Generic;
using CQRS.Mediatr.Lite.SDK.Domain;
using Microsoft.FeatureFlighting.Common.Model.AzureAppConfig;

namespace Microsoft.FeatureFlighting.Core.Domain.ValueObjects
{
    public class Status: ValueObject
    {
        public bool Enabled { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsOptimized { get; private set; }
        public List<string> Optimizations { get; private set; }

        public Status(bool enabled, bool isOptimized, List<string> optimizations)
        {
            Enabled = enabled;
            IsActive = false;
            IsOptimized = isOptimized;
            Optimizations = optimizations;
        }

        public void Toggle()
        {
            Enabled = !Enabled;
        }

        public void UpdateActiveStatus(Condition condition)
        {
            IsActive = Enabled && condition != null && condition.IsActive();
        }

        public void SetOptimizationStatus(AzureFeatureFlag flag)
        {
            IsOptimized = flag.IsFlagOptimized;
            Optimizations = flag.Optimizations;
        }

        public override bool Equals(object obj)
        {
            if (obj is not Status otherStatus)
                return false;

            return Enabled == otherStatus.Enabled
                && IsOptimized == otherStatus.IsOptimized;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Enabled, IsActive, IsOptimized);
        }
    }
}
