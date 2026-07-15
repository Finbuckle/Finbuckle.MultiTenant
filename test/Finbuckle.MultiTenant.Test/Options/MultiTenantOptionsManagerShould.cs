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
        var factory = new Mock<IOptionsFactory<object>>();
        factory.Setup(f => f.Create(optionName)).Returns(new object());
        var context = BuildTenantContext("tenant-1");
        var cache = new MultiTenantOptionsCache<object>(context);
        var manager = new MultiTenantOptionsManager<object>(factory.Object, cache);

        manager.Get(optionName);
        manager.Get(optionName);

        factory.Verify(f => f.Create(It.Is<string>(p => p == optionName)), Times.Once);
    }

    [Fact]
    public void GetOptionByDefaultNameIfNameNull()
    {
        var factory = new Mock<IOptionsFactory<object>>();
        factory.Setup(f => f.Create(Microsoft.Extensions.Options.Options.DefaultName)).Returns(new object());
        var context = BuildTenantContext("tenant-1");
        var manager = new MultiTenantOptionsManager<object>(factory.Object,
            new MultiTenantOptionsCache<object>(context));

        manager.Get(null!);

        factory.Verify(f => f.Create(It.Is<string>(p => p == Microsoft.Extensions.Options.Options.DefaultName)), Times.Once);
    }

    [Fact]
    public void GetOptionByDefaultNameIfGettingValueProp()
    {
        var factory = new Mock<IOptionsFactory<object>>();
        factory.Setup(f => f.Create(Microsoft.Extensions.Options.Options.DefaultName)).Returns(new object());
        var context = BuildTenantContext("tenant-1");
        var manager = new MultiTenantOptionsManager<object>(factory.Object,
            new MultiTenantOptionsCache<object>(context));

        _ = manager.Value;

        factory.Verify(f => f.Create(It.Is<string>(p => p == Microsoft.Extensions.Options.Options.DefaultName)), Times.Once);
    }

    [Fact]
    public void ClearCacheOnReset()
    {
        var factory = new Mock<IOptionsFactory<object>>();
        factory.Setup(f => f.Create(Microsoft.Extensions.Options.Options.DefaultName)).Returns(() => new object());
        var context = BuildTenantContext("tenant-1");
        var manager = new MultiTenantOptionsManager<object>(factory.Object,
            new MultiTenantOptionsCache<object>(context));

        var first = manager.Value;
        manager.Reset();
        var second = manager.Value;

        Assert.NotSame(first, second);
        factory.Verify(f => f.Create(It.Is<string>(p => p == Microsoft.Extensions.Options.Options.DefaultName)), Times.Exactly(2));
    }

    [Fact]
    public void UseTenantIdFromTenantContextForCachePartitioning()
    {
        var factory = new Mock<IOptionsFactory<object>>();
        factory.Setup(f => f.Create(Microsoft.Extensions.Options.Options.DefaultName)).Returns(() => new object());
        var context = new AmbientTenantContext<TenantInfo>();
        var manager = new MultiTenantOptionsManager<object>(factory.Object,
            new MultiTenantOptionsCache<object>(context));

        context.BeginScope();
        context.TenantInfo = new TenantInfo { Id = "tenant-1", Identifier = "tenant-1" };
        var tenant1First = manager.Value;
        var tenant1Second = manager.Value;

        context.BeginScope();
        context.TenantInfo = new TenantInfo { Id = "tenant-2", Identifier = "tenant-2" };
        var tenant2First = manager.Value;
        var tenant2Second = manager.Value;

        Assert.Same(tenant1First, tenant1Second);
        Assert.Same(tenant2First, tenant2Second);
        Assert.NotSame(tenant1First, tenant2First);
        factory.Verify(f => f.Create(It.IsAny<string>()), Times.Exactly(2));
    }

    private static AmbientTenantContext<TenantInfo> BuildTenantContext(string tenantId)
    {
        var context = new AmbientTenantContext<TenantInfo>();
        context.BeginScope();
        context.TenantInfo = new TenantInfo { Id = tenantId, Identifier = tenantId };
        return context;
    }
}
