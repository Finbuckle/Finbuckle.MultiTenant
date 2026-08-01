// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Test.Extensions.EntityTypeBuilderExtensions;

// Verifies IsMultiTenant, EnforceMultiTenant, and AdjustKey work for non-string tenant id types.
public class GenericTenantIdEntityTypeBuilderExtensionsShould
{
    [Fact]
    public void CreateIntTenantIdShadowProperty()
    {
        var options = new DbContextOptionsBuilder<GenericTenantIdDbContext<int>>()
            .UseSqlite("DataSource=:memory:")
            .Options;
        using var db = new GenericTenantIdDbContext<int>(options, new TenantInfo<int> { Id = 1, Identifier = "t" });
        var prop = db.Model.FindEntityType(typeof(GenericTenantBlog<int>))!.FindProperty("TenantId")!;

        Assert.Equal(typeof(int), prop.ClrType);
        Assert.True(prop.IsShadowProperty());
        Assert.False(prop.IsNullable);
    }

    [Fact]
    public void CreateGuidTenantIdShadowProperty()
    {
        var options = new DbContextOptionsBuilder<GenericTenantIdDbContext<Guid>>()
            .UseSqlite("DataSource=:memory:")
            .Options;
        using var db = new GenericTenantIdDbContext<Guid>(options,
            new TenantInfo<Guid> { Id = Guid.NewGuid(), Identifier = "t" });
        var prop = db.Model.FindEntityType(typeof(GenericTenantBlog<Guid>))!.FindProperty("TenantId")!;

        Assert.Equal(typeof(Guid), prop.ClrType);
        Assert.True(prop.IsShadowProperty());
        Assert.False(prop.IsNullable);
    }

    [Fact]
    public void FilterIntTenantRowsByTypedId()
    {
        var tenant1 = new TenantInfo<int> { Id = 1, Identifier = "t1" };
        var tenant2 = new TenantInfo<int> { Id = 2, Identifier = "t2" };

        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<GenericTenantIdDbContext<int>>()
            .UseSqlite(connection)
            .Options;

        // Insert via SaveChanges so EnforceMultiTenant auto-fills the typed TenantId (exercises the int code path).
        using (var db = new GenericTenantIdDbContext<int>(options, tenant1))
        {
            db.Database.EnsureCreated();
            db.Blogs.Add(new GenericTenantBlog<int> { Id = 1 });
            db.SaveChanges();
        }

        using (var db = new GenericTenantIdDbContext<int>(options, tenant2))
        {
            db.Blogs.Add(new GenericTenantBlog<int> { Id = 2 });
            db.SaveChanges();
        }

        using (var db = new GenericTenantIdDbContext<int>(options, tenant1))
            Assert.Equal(new[] { 1 }, db.Blogs.Select(b => b.Id).ToArray());

        using (var db = new GenericTenantIdDbContext<int>(options, tenant2))
            Assert.Equal(new[] { 2 }, db.Blogs.Select(b => b.Id).ToArray());
    }

    [Fact]
    public void FilterGuidTenantRowsByTypedId()
    {
        var tenant1 = new TenantInfo<Guid> { Id = Guid.NewGuid(), Identifier = "t1" };
        var tenant2 = new TenantInfo<Guid> { Id = Guid.NewGuid(), Identifier = "t2" };

        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<GenericTenantIdDbContext<Guid>>()
            .UseSqlite(connection)
            .Options;

        using (var db = new GenericTenantIdDbContext<Guid>(options, tenant1))
        {
            db.Database.EnsureCreated();
            db.Blogs.Add(new GenericTenantBlog<Guid> { Id = 1 });
            db.SaveChanges();
        }

        using (var db = new GenericTenantIdDbContext<Guid>(options, tenant2))
        {
            db.Blogs.Add(new GenericTenantBlog<Guid> { Id = 2 });
            db.SaveChanges();
        }

        using (var db = new GenericTenantIdDbContext<Guid>(options, tenant1))
            Assert.Equal(new[] { 1 }, db.Blogs.Select(b => b.Id).ToArray());

        using (var db = new GenericTenantIdDbContext<Guid>(options, tenant2))
            Assert.Equal(new[] { 2 }, db.Blogs.Select(b => b.Id).ToArray());
    }

    [Fact]
    public void CreateIntTenantIdDependentForeignKeyProperty()
    {
        var options = new DbContextOptionsBuilder<GenericTenantRelationshipDbContext<int>>()
            .UseSqlite("DataSource=:memory:")
            .Options;
        using var db = new GenericTenantRelationshipDbContext<int>(options);
        var prop = db.Model.FindEntityType(typeof(GenericTenantRelationshipPost<int>))!.FindProperty("TenantId")!;

        Assert.Equal(typeof(int), prop.ClrType);
    }

    [Fact]
    public void CreateGuidTenantIdDependentForeignKeyProperty()
    {
        var options = new DbContextOptionsBuilder<GenericTenantRelationshipDbContext<Guid>>()
            .UseSqlite("DataSource=:memory:")
            .Options;
        using var db = new GenericTenantRelationshipDbContext<Guid>(options);
        var prop = db.Model.FindEntityType(typeof(GenericTenantRelationshipPost<Guid>))!.FindProperty("TenantId")!;

        Assert.Equal(typeof(Guid), prop.ClrType);
    }
}
