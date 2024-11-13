// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Test.MultiTenantDbContext;

public class MultiTenantDbContextShould
{
    [Fact]
    public void WorkWithDependencyInjection()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>();
        services.AddDbContext<TestBlogDbContext>(options =>
        {
            options.UseSqlite("DataSource=:memory:");
        });
        var scope = services.BuildServiceProvider().CreateScope();
            
        var context = scope.ServiceProvider.GetService<TestBlogDbContext>();
        Assert.NotNull(context);
    }

    [Fact]
    public void WorkWithSingleParamCtor()
    {
        var tenant1 = new TenantInfo
        {
            Id = "abc",
            Identifier = "abc",
            Name = "abc"
        };
        var mca = new StaticMultiTenantContextAccessor<TenantInfo>(tenant1);
        var c = new TestBlogDbContext(mca);

        Assert.NotNull(c);
    }

    [Fact]
    public void WorkWithTwoParamCtor()
    {
        var tenant1 = new TenantInfo
        {
            Id = "abc",
            Identifier = "abc",
            Name = "abc"
        };
        var mca = new StaticMultiTenantContextAccessor<TenantInfo>(tenant1);
        var c = new TestBlogDbContext(mca, new DbContextOptions<TestBlogDbContext>());

        Assert.NotNull(c);
    }
    
    [Fact]
    public void WorkWithCreate()
    {
        var tenant1 = new TenantInfo
        {
            Id = "abc",
            Identifier = "abc",
            Name = "abc"
        };
        var c =
            EntityFrameworkCore.MultiTenantDbContext.Create<TestBlogDbContext, TenantInfo>(tenant1, new DbContextOptions<TestBlogDbContext>());

        Assert.NotNull(c);
    }
    
    [Fact]
    public void WorkWithCreateNoOptions()
    {
        var tenant1 = new TenantInfo
        {
            Id = "abc",
            Identifier = "abc",
            Name = "abc"
        };
        var c = EntityFrameworkCore.MultiTenantDbContext.Create<TestBlogDbContext, TenantInfo>(tenant1);

        Assert.NotNull(c);
    }
    
    [Fact]
    public void CreateMultiTenantIdentityDbContext()
    {
        var tenant1 = new TenantInfo
        {
            Id = "abc",
            Identifier = "abc",
            Name = "abc"
        };
        var c = EntityFrameworkCore.MultiTenantDbContext.Create<EntityFrameworkCore.MultiTenantIdentityDbContext, TenantInfo>(tenant1);

        Assert.NotNull(c);
    }
    
    [Fact]
    public void ThrowOnInvalidDbContext()
    {
        var tenant1 = new TenantInfo
        {
            Id = "abc",
            Identifier = "abc",
            Name = "abc"
        };

        Assert.Throws<ArgumentException>(() => EntityFrameworkCore.MultiTenantDbContext.Create<DbContext, TenantInfo>(tenant1));
    }
}