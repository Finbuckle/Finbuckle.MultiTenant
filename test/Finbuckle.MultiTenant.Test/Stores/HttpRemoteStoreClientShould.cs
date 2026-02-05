// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Stores;
using Xunit;

namespace Finbuckle.MultiTenant.Test.Stores;

public class HttpRemoteStoreClientShould
{
    [Fact]
    public void ThrowIfHttpClientFactoryIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new HttpRemoteStoreClient<TenantInfo>(null!));
    }
}