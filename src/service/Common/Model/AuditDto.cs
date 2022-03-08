using System;

namespace Microsoft.FeatureFlighting.Common.Model
{
    public class AuditDto
    {
        public DateTime CreatedOn { get; set; }

        public string CreatedBy { get; set; }

        public DateTime LastModifiedOn { get; set; }

        public string LastModifiedBy { get; set; }

        public string LastUpdateType { get; set; }

        public DateTime? EnabledOn { get; set; }

        public DateTime? DisabledOn { get; set; }
    }
}
