// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Data.Common;
using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.EntityFrameworkCore.Extensions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Test.Extensions.MultiTenantDbContextExtensions;

// Exercises EnforceMultiTenant / EnforceMultiTenantOnTracking with a value-type (int) TId.
// The key behaviours are that a TenantId equal to default(TId) (0 for int) is treated as
// "not set" (auto-filled) rather than as a mismatch, and that a genuine mismatch is still caught.
public class IntTenantIdEnforceMultiTenantShould
{
    private sealed class IntTenantDbContext : EntityFrameworkCore.MultiTenantDbContext<int>
    {
        public IntTenantDbContext(ITenantInfo<int> tenantInfo, DbContextOptions options) : base(options)
        {
            TenantInfo = tenantInfo;
        }

        public DbSet<IntBlog> Blogs => Set<IntBlog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IntBlog>().IsMultiTenant<int>();
            base.OnModelCreating(modelBuilder);
        }
    }

    private sealed class IntBlog
    {
        public int Id { get; set; }
        public string? Title { get; set; }
    }

    private readonly DbConnection _connection;
    private readonly DbContextOptions _options;

    public IntTenantIdEnforceMultiTenantShould()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _options = new DbContextOptionsBuilder().UseSqlite(_connection).Options;
    }

    private IntTenantDbContext NewDb(ITenantInfo<int> tenant) => new(tenant, _options);

    [Fact]
    public void TreatDefaultIntTenantIdAsNotSetAndOverwriteOnAdd()
    {
        try
        {
            _connection.Open();
            var tenant = new TenantInfo<int> { Id = 5, Identifier = "t5" };

            using var db = NewDb(tenant);
            db.Database.EnsureCreated();

            // TenantId left unset -> shadow property defaults to 0 (default(int)).
            var blog = new IntBlog { Title = "a" };
            db.Blogs.Add(blog);
            db.SaveChanges();

            // Should have been treated as "not set" and auto-filled with the tenant id, not thrown as a mismatch.
            Assert.Equal(5, db.Entry(blog).Property("TenantId").CurrentValue);
        }
        finally
        {
            _connection.Close();
        }
    }

    [Fact]
    public void ThrowOnGenuineIntTenantIdMismatchWhenAdding()
    {
        try
        {
            _connection.Open();
            var tenant = new TenantInfo<int> { Id = 5, Identifier = "t5" };

            using var db = NewDb(tenant);
            db.Database.EnsureCreated();
            db.TenantMismatchMode = TenantMismatchMode.Throw;

            var blog = new IntBlog { Title = "a" };
            db.Blogs.Add(blog);
            db.Entry(blog).Property("TenantId").CurrentValue = 77; // non-default, wrong tenant

            Assert.Throws<MultiTenantException>(() => db.SaveChanges());
        }
        finally
        {
            _connection.Close();
        }
    }

    [Fact]
    public void OverwriteIntTenantIdMismatchWhenModeIsOverwrite()
    {
        try
        {
            _connection.Open();
            var tenant = new TenantInfo<int> { Id = 5, Identifier = "t5" };

            using var db = NewDb(tenant);
            db.Database.EnsureCreated();
            db.TenantMismatchMode = TenantMismatchMode.Overwrite;

            var blog = new IntBlog { Title = "a" };
            db.Blogs.Add(blog);
            db.Entry(blog).Property("TenantId").CurrentValue = 77;
            db.SaveChanges();

            Assert.Equal(5, db.Entry(blog).Property("TenantId").CurrentValue);
        }
        finally
        {
            _connection.Close();
        }
    }

    [Fact]
    public void IgnoreIntTenantIdMismatchWhenModeIsIgnore()
    {
        try
        {
            _connection.Open();
            var tenant = new TenantInfo<int> { Id = 5, Identifier = "t5" };

            using var db = NewDb(tenant);
            db.Database.EnsureCreated();
            db.TenantMismatchMode = TenantMismatchMode.Ignore;

            var blog = new IntBlog { Title = "a" };
            db.Blogs.Add(blog);
            db.Entry(blog).Property("TenantId").CurrentValue = 77;
            db.SaveChanges();

            Assert.Equal(77, db.Entry(blog).Property("TenantId").CurrentValue);
        }
        finally
        {
            _connection.Close();
        }
    }

    [Fact]
    public void ThrowWhenModifiedIntTenantIdNotSetAndModeIsThrow()
    {
        try
        {
            _connection.Open();
            var tenant = new TenantInfo<int> { Id = 5, Identifier = "t5" };

            // Seed a row belonging to tenant 5.
            using (var db = NewDb(tenant))
            {
                db.Database.EnsureCreated();
                var blog = new IntBlog { Title = "a" };
                db.Blogs.Add(blog);
                db.SaveChanges();
            }

            using (var db = NewDb(tenant))
            {
                db.TenantNotSetMode = TenantNotSetMode.Throw;
                var blog = db.Blogs.Single();
                blog.Title = "changed";
                // Force the TenantId to default (0) to simulate "not set" on a modified entity.
                db.Entry(blog).Property("TenantId").CurrentValue = 0;

                Assert.Throws<MultiTenantException>(() => db.SaveChanges());
            }
        }
        finally
        {
            _connection.Close();
        }
    }

    [Fact]
    public void AutoFillDefaultIntTenantIdViaTrackingHandler()
    {
        try
        {
            _connection.Open();
            var tenant = new TenantInfo<int> { Id = 5, Identifier = "t5" };

            using var db = NewDb(tenant);
            db.Database.EnsureCreated();
            db.EnforceMultiTenantOnTracking<IntTenantDbContext, int>();

            // On tracking, the default (0) TenantId should be filled from the tenant, despite being non-null when boxed.
            var blog = new IntBlog { Title = "a" };
            db.Blogs.Add(blog);

            Assert.Equal(5, db.Entry(blog).Property("TenantId").CurrentValue);
        }
        finally
        {
            _connection.Close();
        }
    }
}
