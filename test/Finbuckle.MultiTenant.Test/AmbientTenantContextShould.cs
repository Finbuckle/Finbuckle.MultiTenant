using Finbuckle.MultiTenant.Abstractions;
using Xunit;

namespace Finbuckle.MultiTenant.Test;

public class AmbientTenantContextShould
{
    [Fact]
    public void ThrowBeforeScopeIsBegun()
    {
        var context = new AmbientTenantContext<TenantInfo>();

        Assert.Throws<MultiTenantException>(() => _ = context.TenantInfo);
        Assert.Throws<MultiTenantException>(() => context.TenantInfo = new TenantInfo { Id = "1", Identifier = "1" });
    }

    [Fact]
    public void BeginScopeStartsUnresolvedForGenericAndNonGenericContexts()
    {
        var context = new AmbientTenantContext<TenantInfo>();
        context.BeginScope();

        Assert.Null(context.TenantInfo);
        Assert.False(((ITenantContext<TenantInfo>)context).IsResolved);

        ITenantContext nonGeneric = context;
        Assert.Null(nonGeneric.TenantInfo);
        Assert.False(nonGeneric.IsResolved);
    }

    [Fact]
    public void TenantInfoCanOnlyBeSetOnceAfterScopeBegins()
    {
        var context = new AmbientTenantContext<TenantInfo>();
        context.BeginScope();
        var tenant = new TenantInfo { Id = "1", Identifier = "1" };

        context.TenantInfo = tenant;

        Assert.Same(tenant, context.TenantInfo);
        Assert.Throws<MultiTenantException>(() => context.TenantInfo = new TenantInfo { Id = "2", Identifier = "2" });
        Assert.Throws<MultiTenantException>(() => context.TenantInfo = null);
    }

    [Fact]
    public void NullDoesNotConsumeTheFirstNonNullAssignment()
    {
        var context = new AmbientTenantContext<TenantInfo>();
        context.BeginScope();

        context.TenantInfo = null;
        var tenant = new TenantInfo { Id = "1", Identifier = "1" };
        context.TenantInfo = tenant;

        Assert.Same(tenant, context.TenantInfo);
    }

    [Fact]
    public void BeginScopeResetsTheCurrentTenant()
    {
        var context = new AmbientTenantContext<TenantInfo>();
        context.BeginScope();
        context.TenantInfo = new TenantInfo { Id = "1", Identifier = "1" };

        context.BeginScope();

        Assert.Null(context.TenantInfo);
        Assert.False(((ITenantContext<TenantInfo>)context).IsResolved);
        context.TenantInfo = new TenantInfo { Id = "2", Identifier = "2" };
        Assert.Equal("2", context.TenantInfo!.Id);
    }

    [Fact]
    public async Task ChildScopeDoesNotLeakIntoParentExecutionContext()
    {
        var context = new AmbientTenantContext<TenantInfo>();
        var parentTenant = new TenantInfo { Id = "parent", Identifier = "parent" };
        var childTenant = new TenantInfo { Id = "child", Identifier = "child" };
        context.BeginScope();
        context.TenantInfo = parentTenant;

        await Task.Run(async () =>
        {
            Assert.Same(parentTenant, context.TenantInfo);
            context.BeginScope();
            context.TenantInfo = childTenant;
            await Task.Yield();
            Assert.Same(childTenant, context.TenantInfo);
        });

        Assert.Same(parentTenant, context.TenantInfo);
    }

    [Fact]
    public async Task ParallelChildScopesRemainIndependent()
    {
        var context = new AmbientTenantContext<TenantInfo>();
        var tenant1 = new TenantInfo { Id = "1", Identifier = "1" };
        var tenant2 = new TenantInfo { Id = "2", Identifier = "2" };

        var task1 = Task.Run(async () =>
        {
            context.BeginScope();
            context.TenantInfo = tenant1;
            await Task.Yield();
            return context.TenantInfo;
        });
        var task2 = Task.Run(async () =>
        {
            context.BeginScope();
            context.TenantInfo = tenant2;
            await Task.Yield();
            return context.TenantInfo;
        });

        var tenants = await Task.WhenAll(task1, task2);

        Assert.Same(tenant1, tenants[0]);
        Assert.Same(tenant2, tenants[1]);
    }
}
