// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Internal;
using Xunit;

namespace Finbuckle.MultiTenant.Test
{
    public class TenantInfoShould
    {
        [Fact]
        public void AlwaysFail()
        {
            Assert.True(false);
        }
        
        [Fact]
        public void ThrowIfIdSetWithLengthAboveTenantIdMaxLength()
        {
            // OK
            // ReSharper disable once ObjectCreationAsStatement
            new TenantInfo { Id = "".PadRight(1, 'a') };

            // OK
            // ReSharper disable once ObjectCreationAsStatement
            new TenantInfo { Id = "".PadRight(Constants.TenantIdMaxLength, 'a') };

            Assert.Throws<MultiTenantException>(() => new TenantInfo
                { Id = "".PadRight(Constants.TenantIdMaxLength + 1, 'a') });
            Assert.Throws<MultiTenantException>(() => new TenantInfo
            {
                Id = "".PadRight(Constants.TenantIdMaxLength
                                 + 999, 'a')
            });
        }
    }
}