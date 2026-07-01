// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Options;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Finbuckle.MultiTenant.Test.Options;

public class MultiTenantOptionsManagerShould
{
    [Theory]
    [InlineData("OptionName1")]
    [InlineData("OptionName2")]
    public void GetOptionByName(string optionName)
    {
        var factory = new Mock<IOptionsFactory<Object>>();
        factory.Setup(f => f.Create(optionName)).Returns(new Object());

        var cache = new MultiTenantOptionsCache<Object>();
        var tenantContext = BuildTenantContext("tenant-1");
        var manager = new MultiTenantOptionsManager<Object>(factory.Object, cache, tenantContext);

        manager.Get(optionName);
        manager.Get(optionName);

        factory.Verify(f => f.Create(It.Is<string>(p => p == optionName)), Times.Once);
    }

    [Fact]
    public void GetOptionByDefaultNameIfNameNull()
    {
        var factory = new Mock<IOptionsFactory<Object>>();
        factory.Setup(f => f.Create(Microsoft.Extensions.Options.Options.DefaultName)).Returns(new Object());

        var cache = new MultiTenantOptionsCache<Object>();
        var tenantContext = BuildTenantContext("tenant-1");
        var manager = new MultiTenantOptionsManager<Object>(factory.Object, cache, tenantContext);

        manager.Get(null!);

        factory.Verify(f => f.Create(It.Is<string>(p => p == Microsoft.Extensions.Options.Options.DefaultName)), Times.Once);
    }

    [Fact]
    public void GetOptionByDefaultNameIfGettingValueProp()
    {
        var factory = new Mock<IOptionsFactory<Object>>();
        factory.Setup(f => f.Create(Microsoft.Extensions.Options.Options.DefaultName)).Returns(new Object());

        var cache = new MultiTenantOptionsCache<Object>();
        var tenantContext = BuildTenantContext("tenant-1");
        var manager = new MultiTenantOptionsManager<Object>(factory.Object, cache, tenantContext);

        var dummy = manager.Value;

        factory.Verify(f => f.Create(It.Is<string>(p => p == Microsoft.Extensions.Options.Options.DefaultName)), Times.Once);
    }

    [Fact]
    public void ClearCacheOnReset()
    {
        var factory = new Mock<IOptionsFactory<Object>>();
        factory.Setup(f => f.Create(Microsoft.Extensions.Options.Options.DefaultName)).Returns(() => new Object());

        var cache = new MultiTenantOptionsCache<Object>();
        var tenantContext = BuildTenantContext("tenant-1");
        var manager = new MultiTenantOptionsManager<Object>(factory.Object, cache, tenantContext);

        var first = manager.Value;
        manager.Reset();
        var second = manager.Value;

        Assert.NotSame(first, second);
        factory.Verify(f => f.Create(It.Is<string>(p => p == Microsoft.Extensions.Options.Options.DefaultName)), Times.Exactly(2));
    }

    [Fact]
    public void UseTenantIdFromTenantContextForCachePartitioning()
    {
        var factory = new Mock<IOptionsFactory<Object>>();
        factory.Setup(f => f.Create(Microsoft.Extensions.Options.Options.DefaultName)).Returns(() => new Object());

        var cache = new MultiTenantOptionsCache<Object>();
        var tenant1Manager = new MultiTenantOptionsManager<Object>(factory.Object, cache, BuildTenantContext("tenant-1"));
        var tenant2Manager = new MultiTenantOptionsManager<Object>(factory.Object, cache, BuildTenantContext("tenant-2"));

        var tenant1First = tenant1Manager.Value;
        var tenant1Second = tenant1Manager.Value;
        var tenant2First = tenant2Manager.Value;
        var tenant2Second = tenant2Manager.Value;

        Assert.Same(tenant1First, tenant1Second);
        Assert.Same(tenant2First, tenant2Second);
        Assert.NotSame(tenant1First, tenant2First);
        factory.Verify(f => f.Create(It.IsAny<string>()), Times.Exactly(2));
    }

    private static ITenantContext BuildTenantContext(string tenantId)
    {
        return new TenantContext<TenantInfo>{ TenantInfo = new TenantInfo { Id = tenantId, Identifier = tenantId } };
    }
}