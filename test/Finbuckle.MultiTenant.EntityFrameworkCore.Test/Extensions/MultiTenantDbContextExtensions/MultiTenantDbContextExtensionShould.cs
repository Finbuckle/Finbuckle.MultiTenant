// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Data.Common;
using System.Reflection;
using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.EntityFrameworkCore.Extensions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Xunit;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Test.Extensions.MultiTenantDbContextExtensions;

public class MultiTenantDbContextExtensionsShould
{
    private readonly DbContextOptions _options;
    private readonly DbConnection _connection;

    public MultiTenantDbContextExtensionsShould()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _options = new DbContextOptionsBuilder()
            .UseSqlite(_connection)
            .Options;
    }

    [Fact]
    public void HandleTenantNotSetWhenAttaching()
    {
        try
        {
            _connection.Open();
            var tenant1 = new TenantInfo { Id = "abc", Identifier = "abc" };

            // TenantNotSetMode.Throw, should act as Overwrite when adding
            using (var db = new TestDbContext(tenant1, _options))
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
                db.EnforceMultiTenantOnTracking<TestDbContext, string>();
                db.TenantNotSetMode = TenantNotSetMode.Throw;

                var blog1 = new Blog { Title = "abc" };
                db.Blogs?.Add(blog1);
                Assert.Equal(tenant1.Identifier, db.Entry(blog1).Property("TenantId").CurrentValue);
            }

            // TenantNotSetMode.Overwrite
            using (var db = new TestDbContext(tenant1, _options))
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                db.TenantNotSetMode = TenantNotSetMode.Overwrite;
                db.EnforceMultiTenantOnTracking<TestDbContext, string>();
                var blog1 = new Blog { Title = "abc2" };
                db.Blogs?.Add(blog1);
                Assert.Equal(tenant1.Id, db.Entry(blog1).Property("TenantId").CurrentValue);
            }
        }
        finally
        {
            _connection.Close();
        }
    }

    [Fact]
    public void HandleTenantNotSetWhenAdding()
    {
        try
        {
            _connection.Open();
            var tenant1 = new TenantInfo { Id = "abc", Identifier = "abc" };

            // TenantNotSetMode.Throw, should act as Overwrite when adding
            using (var db = new TestDbContext(tenant1, _options))
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
                db.TenantNotSetMode = TenantNotSetMode.Throw;

                var blog1 = new Blog { Title = "abc" };
                db.Blogs?.Add(blog1);
                db.SaveChanges();
                Assert.Equal(tenant1.Identifier, db.Entry(blog1).Property("TenantId").CurrentValue);
            }

            // TenantNotSetMode.Overwrite
            using (var db = new TestDbContext(tenant1, _options))
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
                db.TenantNotSetMode = TenantNotSetMode.Overwrite;

                var blog1 = new Blog { Title = "abc2" };
                db.Blogs?.Add(blog1);
                db.SaveChanges();
                Assert.Equal(tenant1.Id, db.Entry(blog1).Property("TenantId").CurrentValue);
            }
        }
        finally
        {
            _connection.Close();
        }
    }

    [Fact]
    public void HandleTenantMismatchWhenAdding()
    {
        try
        {
            _connection.Open();
            var tenant1 = new TenantInfo { Id = "abc", Identifier = "abc" };

            // TenantMismatchMode.Throw
            using (var db = new TestDbContext(tenant1, _options))
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
                db.TenantMismatchMode = TenantMismatchMode.Throw;

                var blog1 = new Blog { Title = "abc" };
                db.Blogs?.Add(blog1);
                db.Entry(blog1).Property("TenantId").CurrentValue = "77";

                Assert.Throws<MultiTenantException>(() => db.SaveChanges());
            }

            // TenantMismatchMode.Ignore
            using (var db = new TestDbContext(tenant1, _options))
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
                db.TenantMismatchMode = TenantMismatchMode.Ignore;

                var blog1 = new Blog { Title = "34" };
                db.Blogs?.Add(blog1);
                db.Entry(blog1).Property("TenantId").CurrentValue = "34";
                db.SaveChanges();
                Assert.Equal("34", db.Entry(blog1).Property("TenantId").CurrentValue);
            }

            // TenantMismatchMode.Overwrite
            using (var db = new TestDbContext(tenant1, _options))
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
                db.TenantMismatchMode = TenantMismatchMode.Overwrite;

                var blog1 = new Blog { Title = "77" };
                db.Blogs?.Add(blog1);
                db.Entry(blog1).Property("TenantId").CurrentValue = "77";
                db.SaveChanges();
                Assert.Equal(tenant1.Id, db.Entry(blog1).Property("TenantId").CurrentValue);
            }
        }
        finally
        {
            _connection.Close();
        }
    }

    [Fact]
    public void HandleTenantNotSetWhenUpdating()
    {
        try
        {
            _connection.Open();
            var tenant1 = new TenantInfo { Id = "abc", Identifier = "abc" };

            // TenantNotSetMode.Throw
            using (var db = new TestDbContext(tenant1, _options))
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                var blog1 = new Blog { Title = "abc" };
                db.Blogs?.Add(blog1);
                db.SaveChanges();

                db.TenantNotSetMode = TenantNotSetMode.Throw;
                db.Entry(blog1).Property("TenantId").CurrentValue = null;

                Assert.Throws<MultiTenantException>(() => db.SaveChanges());
            }

            // TenantNotSetMode.Overwrite
            using (var db = new TestDbContext(tenant1, _options))
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                var blog1 = new Blog { Title = "abc12" };
                db.Blogs?.Add(blog1);
                db.SaveChanges();

                db.TenantNotSetMode = TenantNotSetMode.Overwrite;
                db.Entry(blog1).Property("TenantId").CurrentValue = null;
                db.SaveChanges();

                Assert.Equal(tenant1.Id, db.Entry(blog1).Property("TenantId").CurrentValue);
            }
        }
        finally
        {
            _connection.Close();
        }
    }

    [Fact]
    public void HandleTenantMismatchWhenUpdating()
    {
        try
        {
            _connection.Open();
            var tenant1 = new TenantInfo { Id = "abc", Identifier = "abc" };

            // TenantMismatchMode.Throw
            using (var db = new TestDbContext(tenant1, _options))
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                var blog1 = new Blog { Title = "abc" };
                db.Blogs?.Add(blog1);
                db.SaveChanges();

                db.TenantMismatchMode = TenantMismatchMode.Throw;
                db.Entry(blog1).Property("TenantId").CurrentValue = "11";

                Assert.Throws<MultiTenantException>(() => db.SaveChanges());
            }

            // TenantMismatchMode.Ignore
            using (var db = new TestDbContext(tenant1, _options))
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                var blog1 = new Blog { Title = "abc" };
                db.Blogs?.Add(blog1);
                db.SaveChanges();

                db.TenantMismatchMode = TenantMismatchMode.Ignore;
                db.Entry(blog1).Property("TenantId").CurrentValue = "11";
                db.SaveChanges();

                Assert.Equal("11", db.Entry(blog1).Property("TenantId").CurrentValue);
            }

            // TenantMismatchMode.Overwrite
            using (var db = new TestDbContext(tenant1, _options))
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                var blog1 = new Blog { Title = "abc12" };
                db.Blogs?.Add(blog1);
                db.SaveChanges();

                db.TenantMismatchMode = TenantMismatchMode.Overwrite;
                db.Entry(blog1).Property("TenantId").CurrentValue = "11";
                db.SaveChanges();

                Assert.Equal(tenant1.Id, db.Entry(blog1).Property("TenantId").CurrentValue);
            }
        }
        finally
        {
            _connection.Close();
        }
    }

    [Fact]
    public void HandleTenantNotSetWhenDeleting()
    {
        try
        {
            _connection.Open();
            var tenant1 = new TenantInfo { Id = "abc", Identifier = "abc" };

            // TenantNotSetMode.Throw
            using (var db = new TestDbContext(tenant1, _options))
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                var blog1 = new Blog { Title = "abc" };
                db.Blogs?.Add(blog1);
                db.SaveChanges();

                db.TenantNotSetMode = TenantNotSetMode.Throw;
                db.Entry(blog1).Property("TenantId").CurrentValue = null;
                db.Blogs?.Remove(blog1);

                Assert.Throws<MultiTenantException>(() => db.SaveChanges());
            }

            // TenantNotSetMode.Overwrite
            using (var db = new TestDbContext(tenant1, _options))
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                var blog1 = new Blog { Title = "abc" };
                db.Blogs?.Add(blog1);
                db.SaveChanges();

                db.TenantNotSetMode = TenantNotSetMode.Overwrite;
                db.Entry(blog1).Property("TenantId").CurrentValue = null;
                db.Blogs?.Remove(blog1);

                Assert.Equal(1, db.SaveChanges());
            }
        }
        finally
        {
            _connection.Close();
        }
    }

    [Fact]
    public void HandleTenantMismatchWhenDeleting()
    {
        try
        {
            _connection.Open();
            var tenant1 = new TenantInfo { Id = "abc", Identifier = "abc" };

            // TenantMismatchMode.Throw
            using (var db = new TestDbContext(tenant1, _options))
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                var blog1 = new Blog { Title = "abc" };
                db.Blogs?.Add(blog1);
                db.SaveChanges();

                db.TenantMismatchMode = TenantMismatchMode.Throw;
                db.Entry(blog1).Property("TenantId").CurrentValue = "17";
                db.Blogs?.Remove(blog1);

                Assert.Throws<MultiTenantException>(() => db.SaveChanges());
            }

            // TenantMismatchMode.Ignore
            using (var db = new TestDbContext(tenant1, _options))
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                var blog1 = new Blog { Title = "abc" };
                db.Blogs?.Add(blog1);
                db.SaveChanges();

                db.TenantMismatchMode = TenantMismatchMode.Ignore;
                db.Entry(blog1).Property("TenantId").CurrentValue = "17";
                db.Blogs?.Remove(blog1);

                Assert.Equal(1, db.SaveChanges());
            }

            // TenantMismatchMode.Overwrite
            using (var db = new TestDbContext(tenant1, _options))
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                var blog1 = new Blog { Title = "abc" };
                db.Blogs?.Add(blog1);
                db.SaveChanges();

                db.TenantMismatchMode = TenantMismatchMode.Overwrite;
                db.Entry(blog1).Property("TenantId").CurrentValue = "17";
                db.Blogs?.Remove(blog1);

                Assert.Equal(1, db.SaveChanges());
            }
        }
        finally
        {
            _connection.Close();
        }
    }

    [Fact]
    public void AddOneTrackingHandlerWhenEnforceMultiTenantOnTrackingCalledTwice()
    {
        var tenant = new TenantInfo { Id = "abc", Identifier = "abc" };
        using var db = new TestDbContext(tenant, _options);

        db.EnforceMultiTenantOnTracking<TestDbContext, string>();
        db.EnforceMultiTenantOnTracking<TestDbContext, string>();

        var stateManager = typeof(ChangeTracker)
            .GetField("_stateManager", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.GetValue(db.ChangeTracker)
            ?? typeof(ChangeTracker)
                .GetProperty("StateManager", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(db.ChangeTracker);

        Assert.NotNull(stateManager);

        var trackingDelegateField = stateManager.GetType()
            .GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)
            .FirstOrDefault(f => typeof(MulticastDelegate).IsAssignableFrom(f.FieldType) &&
                                 f.Name.Contains("Tracking", StringComparison.OrdinalIgnoreCase));

        Assert.NotNull(trackingDelegateField);

        var trackingDelegate = trackingDelegateField.GetValue(stateManager) as MulticastDelegate;
        Assert.Equal(1, trackingDelegate?.GetInvocationList().Length ?? 0);
    }

    [Fact]
    public void SetTenantIdOnAttachWhenTenantNotSetModeIsThrow()
    {
        try
        {
            _connection.Open();
            var tenant = new TenantInfo { Id = "abc", Identifier = "abc" };

            using var db = new TestDbContext(tenant, _options);
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
            db.TenantNotSetMode = TenantNotSetMode.Throw;
            db.EnforceMultiTenantOnTracking<TestDbContext, string>();

            var blog = new Blog { Title = "attached" };
            db.Attach(blog);

            Assert.Equal(tenant.Id, db.Entry(blog).Property("TenantId").CurrentValue);
        }
        finally
        {
            _connection.Close();
        }
    }

    [Fact]
    public void SetTenantIdWhenAddingEntity()
    {
        try
        {
            _connection.Open();
            var tenant = new TenantInfo { Id = "abc", Identifier = "abc" };

            using var db = new TestDbContext(tenant, _options);
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
            db.TenantNotSetMode = TenantNotSetMode.Throw;
            db.EnforceMultiTenantOnTracking<TestDbContext, string>();

            var blog = new Blog { Title = "state-change" };
            db.Add(blog);

            Assert.Equal(tenant.Id, db.Entry(blog).Property("TenantId").CurrentValue);
        }
        finally
        {
            _connection.Close();
        }
    }

    [Fact]
    public void ThrowWhenTenantInfoNullAndEntitiesChangedOnSave()
    {
        try
        {
            _connection.Open();
            var tenant = new TenantInfo { Id = "abc", Identifier = "abc" };

            using var db = new TestDbContext(tenant, _options);
            db.Database.EnsureCreated();
            db.TenantInfo = null;

            db.Blogs?.Add(new Blog { Title = "test" });
            Assert.Throws<MultiTenantException>(() => db.SaveChanges());
        }
        finally
        {
            _connection.Close();
        }
    }

    [Fact]
    public void ThrowWhenTenantInfoNullOnTracking()
    {
        try
        {
            _connection.Open();
            var tenant = new TenantInfo { Id = "abc", Identifier = "abc" };

            using var db = new TestDbContext(tenant, _options);
            db.Database.EnsureCreated();
            db.EnforceMultiTenantOnTracking<TestDbContext, string>();
            db.TenantInfo = null;

            Assert.Throws<MultiTenantException>(() => db.Add(new Blog { Title = "test" }));
        }
        finally
        {
            _connection.Close();
        }
    }

    [Fact]
    public async Task HandleTenantNotSetWhenAddingAsync()
    {
        try
        {
            await _connection.OpenAsync();
            var tenant1 = new TenantInfo { Id = "abc", Identifier = "abc" };

            // TenantNotSetMode.Throw, should act as Overwrite when adding
            await using (var db = new TestDbContext(tenant1, _options))
            {
                await db.Database.EnsureDeletedAsync();
                await db.Database.EnsureCreatedAsync();
                db.TenantNotSetMode = TenantNotSetMode.Throw;

                var blog1 = new Blog { Title = "abc" };
                db.Blogs?.Add(blog1);
                await db.SaveChangesAsync();
                Assert.Equal(tenant1.Id, db.Entry(blog1).Property("TenantId").CurrentValue);
            }

            // TenantNotSetMode.Overwrite
            await using (var db = new TestDbContext(tenant1, _options))
            {
                await db.Database.EnsureDeletedAsync();
                await db.Database.EnsureCreatedAsync();
                db.TenantNotSetMode = TenantNotSetMode.Overwrite;

                var blog1 = new Blog { Title = "abc2" };
                db.Blogs?.Add(blog1);
                await db.SaveChangesAsync();
                Assert.Equal(tenant1.Id, db.Entry(blog1).Property("TenantId").CurrentValue);
            }
        }
        finally
        {
            await _connection.CloseAsync();
        }
    }

    [Fact]
    public async Task HandleTenantMismatchWhenAddingAsync()
    {
        try
        {
            await _connection.OpenAsync();
            var tenant1 = new TenantInfo { Id = "abc", Identifier = "abc" };

            // TenantMismatchMode.Throw
            await using (var db = new TestDbContext(tenant1, _options))
            {
                await db.Database.EnsureDeletedAsync();
                await db.Database.EnsureCreatedAsync();
                db.TenantMismatchMode = TenantMismatchMode.Throw;

                var blog1 = new Blog { Title = "abc" };
                db.Blogs?.Add(blog1);
                db.Entry(blog1).Property("TenantId").CurrentValue = "77";
                await Assert.ThrowsAsync<MultiTenantException>(() => db.SaveChangesAsync());
            }

            // TenantMismatchMode.Ignore
            await using (var db = new TestDbContext(tenant1, _options))
            {
                await db.Database.EnsureDeletedAsync();
                await db.Database.EnsureCreatedAsync();
                db.TenantMismatchMode = TenantMismatchMode.Ignore;

                var blog1 = new Blog { Title = "34" };
                db.Blogs?.Add(blog1);
                db.Entry(blog1).Property("TenantId").CurrentValue = "34";
                await db.SaveChangesAsync();
                Assert.Equal("34", db.Entry(blog1).Property("TenantId").CurrentValue);
            }

            // TenantMismatchMode.Overwrite
            await using (var db = new TestDbContext(tenant1, _options))
            {
                await db.Database.EnsureDeletedAsync();
                await db.Database.EnsureCreatedAsync();
                db.TenantMismatchMode = TenantMismatchMode.Overwrite;

                var blog1 = new Blog { Title = "77" };
                db.Blogs?.Add(blog1);
                db.Entry(blog1).Property("TenantId").CurrentValue = "77";
                await db.SaveChangesAsync();
                Assert.Equal(tenant1.Id, db.Entry(blog1).Property("TenantId").CurrentValue);
            }
        }
        finally
        {
            await _connection.CloseAsync();
        }
    }

    [Fact]
    public async Task HandleTenantNotSetWhenUpdatingAsync()
    {
        try
        {
            await _connection.OpenAsync();
            var tenant1 = new TenantInfo { Id = "abc", Identifier = "abc" };

            // TenantNotSetMode.Throw
            await using (var db = new TestDbContext(tenant1, _options))
            {
                await db.Database.EnsureDeletedAsync();
                await db.Database.EnsureCreatedAsync();

                var blog1 = new Blog { Title = "abc" };
                db.Blogs?.Add(blog1);
                await db.SaveChangesAsync();

                db.TenantNotSetMode = TenantNotSetMode.Throw;
                db.Entry(blog1).Property("TenantId").CurrentValue = null;
                await Assert.ThrowsAsync<MultiTenantException>(() => db.SaveChangesAsync());
            }

            // TenantNotSetMode.Overwrite
            await using (var db = new TestDbContext(tenant1, _options))
            {
                await db.Database.EnsureDeletedAsync();
                await db.Database.EnsureCreatedAsync();

                var blog1 = new Blog { Title = "abc12" };
                db.Blogs?.Add(blog1);
                await db.SaveChangesAsync();

                db.TenantNotSetMode = TenantNotSetMode.Overwrite;
                db.Entry(blog1).Property("TenantId").CurrentValue = null;
                await db.SaveChangesAsync();

                Assert.Equal(tenant1.Id, db.Entry(blog1).Property("TenantId").CurrentValue);
            }
        }
        finally
        {
            await _connection.CloseAsync();
        }
    }

    [Fact]
    public async Task HandleTenantMismatchWhenUpdatingAsync()
    {
        try
        {
            await _connection.OpenAsync();
            var tenant1 = new TenantInfo { Id = "abc", Identifier = "abc" };

            // TenantMismatchMode.Throw
            await using (var db = new TestDbContext(tenant1, _options))
            {
                await db.Database.EnsureDeletedAsync();
                await db.Database.EnsureCreatedAsync();

                var blog1 = new Blog { Title = "abc" };
                db.Blogs?.Add(blog1);
                await db.SaveChangesAsync();

                db.TenantMismatchMode = TenantMismatchMode.Throw;
                db.Entry(blog1).Property("TenantId").CurrentValue = "11";
                await Assert.ThrowsAsync<MultiTenantException>(() => db.SaveChangesAsync());
            }

            // TenantMismatchMode.Ignore
            await using (var db = new TestDbContext(tenant1, _options))
            {
                await db.Database.EnsureDeletedAsync();
                await db.Database.EnsureCreatedAsync();

                var blog1 = new Blog { Title = "abc" };
                db.Blogs?.Add(blog1);
                await db.SaveChangesAsync();

                db.TenantMismatchMode = TenantMismatchMode.Ignore;
                db.Entry(blog1).Property("TenantId").CurrentValue = "11";
                await db.SaveChangesAsync();
                Assert.Equal("11", db.Entry(blog1).Property("TenantId").CurrentValue);
            }

            // TenantMismatchMode.Overwrite
            await using (var db = new TestDbContext(tenant1, _options))
            {
                await db.Database.EnsureDeletedAsync();
                await db.Database.EnsureCreatedAsync();

                var blog1 = new Blog { Title = "abc12" };
                db.Blogs?.Add(blog1);
                await db.SaveChangesAsync();

                db.TenantMismatchMode = TenantMismatchMode.Overwrite;
                db.Entry(blog1).Property("TenantId").CurrentValue = "11";
                await db.SaveChangesAsync();
                Assert.Equal(tenant1.Id, db.Entry(blog1).Property("TenantId").CurrentValue);
            }
        }
        finally
        {
            await _connection.CloseAsync();
        }
    }

    [Fact]
    public async Task HandleTenantNotSetWhenDeletingAsync()
    {
        try
        {
            await _connection.OpenAsync();
            var tenant1 = new TenantInfo { Id = "abc", Identifier = "abc" };

            // TenantNotSetMode.Throw
            await using (var db = new TestDbContext(tenant1, _options))
            {
                await db.Database.EnsureDeletedAsync();
                await db.Database.EnsureCreatedAsync();

                var blog1 = new Blog { Title = "abc" };
                db.Blogs?.Add(blog1);
                await db.SaveChangesAsync();

                db.TenantNotSetMode = TenantNotSetMode.Throw;
                db.Entry(blog1).Property("TenantId").CurrentValue = null;
                db.Blogs?.Remove(blog1);
                await Assert.ThrowsAsync<MultiTenantException>(() => db.SaveChangesAsync());
            }

            // TenantNotSetMode.Overwrite
            await using (var db = new TestDbContext(tenant1, _options))
            {
                await db.Database.EnsureDeletedAsync();
                await db.Database.EnsureCreatedAsync();

                var blog1 = new Blog { Title = "abc" };
                db.Blogs?.Add(blog1);
                await db.SaveChangesAsync();

                db.TenantNotSetMode = TenantNotSetMode.Overwrite;
                db.Entry(blog1).Property("TenantId").CurrentValue = null;
                db.Blogs?.Remove(blog1);
                Assert.Equal(1, await db.SaveChangesAsync());
            }
        }
        finally
        {
            await _connection.CloseAsync();
        }
    }

    [Fact]
    public async Task HandleTenantMismatchWhenDeletingAsync()
    {
        try
        {
            await _connection.OpenAsync();
            var tenant1 = new TenantInfo { Id = "abc", Identifier = "abc" };

            // TenantMismatchMode.Throw
            await using (var db = new TestDbContext(tenant1, _options))
            {
                await db.Database.EnsureDeletedAsync();
                await db.Database.EnsureCreatedAsync();

                var blog1 = new Blog { Title = "abc" };
                db.Blogs?.Add(blog1);
                await db.SaveChangesAsync();

                db.TenantMismatchMode = TenantMismatchMode.Throw;
                db.Entry(blog1).Property("TenantId").CurrentValue = "17";
                db.Blogs?.Remove(blog1);
                await Assert.ThrowsAsync<MultiTenantException>(() => db.SaveChangesAsync());
            }

            // TenantMismatchMode.Ignore
            await using (var db = new TestDbContext(tenant1, _options))
            {
                await db.Database.EnsureDeletedAsync();
                await db.Database.EnsureCreatedAsync();

                var blog1 = new Blog { Title = "abc" };
                db.Blogs?.Add(blog1);
                await db.SaveChangesAsync();

                db.TenantMismatchMode = TenantMismatchMode.Ignore;
                db.Entry(blog1).Property("TenantId").CurrentValue = "17";
                db.Blogs?.Remove(blog1);
                Assert.Equal(1, await db.SaveChangesAsync());
            }

            // TenantMismatchMode.Overwrite
            await using (var db = new TestDbContext(tenant1, _options))
            {
                await db.Database.EnsureDeletedAsync();
                await db.Database.EnsureCreatedAsync();

                var blog1 = new Blog { Title = "abc" };
                db.Blogs?.Add(blog1);
                await db.SaveChangesAsync();

                db.TenantMismatchMode = TenantMismatchMode.Overwrite;
                db.Entry(blog1).Property("TenantId").CurrentValue = "17";
                db.Blogs?.Remove(blog1);
                Assert.Equal(1, await db.SaveChangesAsync());
            }
        }
        finally
        {
            await _connection.CloseAsync();
        }
    }
}
