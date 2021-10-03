// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using Finbuckle.MultiTenant.Internal;

namespace Finbuckle.MultiTenant
{
    public class TenantInfo : ITenantInfo
    {
        private string id;

        public TenantInfo()
        {
        }

        public string Id
        {
            get
            {
                return id;
            }
            set
            {
                if (value != null)
                {
                    if (value.Length > Constants.TenantIdMaxLength)
                    {
                        throw new MultiTenantException($"The tenant id cannot exceed {Constants.TenantIdMaxLength} characters.");
                    }
                    id = value;
                }
            }
        }

        public string Identifier { get; set; }
        public string Name { get; set; }
        public string ConnectionString { get; set; }
    }
}