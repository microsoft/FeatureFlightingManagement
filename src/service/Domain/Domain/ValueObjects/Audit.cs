using System;
using CQRS.Mediatr.Lite.SDK.Domain;
using Microsoft.FeatureFlighting.Common.Model;

namespace Microsoft.FeatureFlighting.Core.Domain.ValueObjects
{
    public class Audit: ValueObject
    {
        public string CreatedBy { get; private set; }
        public DateTime CreatedOn { get; private set; }
        public string LastModifiedBy { get; private set; }
        public DateTime LastModifiedOn { get; private set; }
        public string LastUpdateType { get; private set; }
        public DateTime? EnabledOn { get; private set; }
        public DateTime? DisabledOn { get; private set; }

        public Audit(string createdBy, DateTime createdOn, string lastModifiedBy, DateTime lastModifiedOn, string updateType)
        {
            CreatedBy = createdBy;
            CreatedOn = createdOn;
            LastModifiedBy = lastModifiedBy;
            LastModifiedOn = lastModifiedOn;
            LastUpdateType = updateType;
        }

        public Audit(string createdBy, DateTime createdOn, bool isActive)
            : this(createdBy, createdOn, createdBy, createdOn, "Flag Created")
        {
            EnabledOn = isActive ? DateTime.UtcNow : null;
            DisabledOn = !isActive ? DateTime.UtcNow : null;
        }

        public Audit(AuditDto audit)
            :this(audit.CreatedBy, audit.CreatedOn, audit.LastModifiedBy, audit.LastModifiedOn, audit.LastUpdateType)
        {
            EnabledOn = audit.EnabledOn;
            DisabledOn = audit.DisabledOn;
        }

        public void Update(string updatedBy, DateTime updatedOn, string updateType)
        {
            LastModifiedBy = updatedBy;
            LastModifiedOn = updatedOn;
            LastUpdateType = updateType;
        }

        public void UpdateEnabledStatus(string updatedBy, DateTime updatedOn, bool isActive)
        {
            LastModifiedBy = updatedBy;
            LastModifiedOn = updatedOn;
            if (isActive)
            {
                LastUpdateType = "Flag Enabled";
                EnabledOn = updatedOn;
                return;
            }
            LastUpdateType = "Flag Disabled";
            DisabledOn = updatedOn;
        }
    }
}
