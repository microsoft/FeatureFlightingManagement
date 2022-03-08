using System;
using System.Collections.Generic;

namespace Microsoft.FeatureFlighting.Common.Config
{
    /// <summary>
    /// Compares 2 tenants
    /// </summary>
    public class TenantConfigurationComparer : IComparer<TenantConfiguration>, IEqualityComparer<TenantConfiguration>
    {
        private TenantConfigurationComparer() { }

        private static readonly TenantConfigurationComparer _default = new();
        public static Lazy<TenantConfigurationComparer> Default = new(_default);

        /// <summary>
        /// Compares tenants based on tenant name
        /// </summary>
        public int Compare(TenantConfiguration tenant1, TenantConfiguration tenant2)
        {
            return tenant1.Name.CompareTo(tenant2.Name);
        }

        /// <summary>
        /// Equates 2 tenants based on tenant name
        /// </summary>
        /// <returns>True if the tenants are equal</returns>
        public bool Equals(TenantConfiguration tenant1, TenantConfiguration tenant2)
        {
            return tenant1.Equals(tenant2);
        }

        public int GetHashCode(TenantConfiguration obj)
        {
            return obj.Name.GetHashCode();
        }
    }
}
