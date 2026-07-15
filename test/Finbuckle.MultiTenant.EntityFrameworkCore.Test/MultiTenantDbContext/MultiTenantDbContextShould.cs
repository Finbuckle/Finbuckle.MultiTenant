// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Extensions;
using Microsoft.Data.Sqlite;
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
        services.AddDbContext<TestBlogDbContext>(options => { options.UseSqlite("DataSource=:memory:"); });
        var scope = services.BuildServiceProvider().CreateScope();

        var context = scope.ServiceProvider.GetService<TestBlogDbContext>();
        Assert.NotNull(context);
    }

    [Fact]
    public void WorkWithSingleParamCtor()
    {
        var c = new TestBlogDbContext();
        Assert.NotNull(c);
    }

    [Fact]
    public void WorkWithTwoParamCtor()
    {
        var c = new TestBlogDbContext(new DbContextOptions<TestBlogDbContext>());
        Assert.NotNull(c);
    }

    [Fact]
    public void WorkWithCreateDbOptions()
    {
        var tenant1 = new TenantInfo { Id = "abc", Identifier = "abc" };
        var c =
            EntityFrameworkCore.MultiTenantDbContext.Create<TestBlogDbContext, TenantInfo>(tenant1,
                new DbContextOptions<TestBlogDbContext>());

        Assert.NotNull(c);
    }

    [Fact]
    public void WorkWithCreateDependencies()
    {
        var tenant1 = new TenantInfo { Id = "abc", Identifier = "abc" };
        var c =
            EntityFrameworkCore.MultiTenantDbContext.Create<TestBlogDbContext, TenantInfo>(tenant1, new object());

        Assert.NotNull(c);
    }

    [Fact]
    public void WorkWithCreateServiceProvider()
    {
        // create a sp
        var services = new ServiceCollection();
        services.AddTransient<object>(_ => 42);
        var sp = services.BuildServiceProvider();

        var tenant1 = new TenantInfo { Id = "abc", Identifier = "abc" };
        var c =
            EntityFrameworkCore.MultiTenantDbContext.Create<TestBlogDbContext, TenantInfo>(tenant1, sp);

        Assert.NotNull(c);
        Assert.Same(tenant1, c.TenantInfo);
    }

    [Fact]
    public void WorkWithCreateNoOptions()
    {
        var tenant1 = new TenantInfo { Id = "abc", Identifier = "abc" };
        var c = EntityFrameworkCore.MultiTenantDbContext.Create<TestBlogDbContext, TenantInfo>(tenant1);

        Assert.NotNull(c);
    }

    [Fact]
    public void ThrowOnInvalidDbContext()
    {
        // Passing args that no constructor accepts should throw ArgumentException.
        var tenant1 = new TenantInfo { Id = "abc", Identifier = "abc" };

        Assert.Throws<ArgumentException>(() =>
            EntityFrameworkCore.MultiTenantDbContext.Create<TestBlogDbContext, TenantInfo>(tenant1, new object(), new object()));
    }

    [Fact]
    public void QueryFilterIsolatesDataByTenant()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder().UseSqlite(connection).Options;

        var tenant1 = new TenantInfo { Id = "t1", Identifier = "t1" };
        var tenant2 = new TenantInfo { Id = "t2", Identifier = "t2" };

        using var setup = new TestBlogDbContext(options);
        setup.TenantInfo = tenant1;
        setup.Database.EnsureCreated();
        setup.Blogs?.Add(new Blog { Title = "tenant1 blog" });
        setup.SaveChanges();

        using var asT2 = new TestBlogDbContext(options);
        asT2.TenantInfo = tenant2;
        Assert.Equal(0, asT2.Blogs?.Count());

        using var asT1 = new TestBlogDbContext(options);
        asT1.TenantInfo = tenant1;
        Assert.Equal(1, asT1.Blogs?.Count());
    }
}
