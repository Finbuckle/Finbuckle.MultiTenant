//    Copyright 2018 Andrew White
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using System.Data.Common;
using System.Linq;
using Finbuckle.MultiTenant.Core;
using Finbuckle.MultiTenant.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class MultiTenantDbContextShould
{
    private DbContextOptions<TestDbContext> _options;
    private DbConnection _connection;

    public MultiTenantDbContextShould()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _options = new DbContextOptionsBuilder<TestDbContext>()
                .UseSqlite(_connection)
                .Options;

    }

    [Fact]
    public void IdentifyOnlyTenantScopedProperties()
    {
        var tenant1 = new TenantContext("abc", "abc", "abc",
            "DataSource=testdb.db", null, null);
        using (var db = new TestDbContext(tenant1, _options))
        {
            var types = db.MultiTenantEntityTypes.Select(t => t.ClrType);

            Assert.Equal(3, types.Count());
            Assert.Contains(typeof(Blog), types);
            Assert.Contains(typeof(Post), types);
            Assert.DoesNotContain(typeof(Config), types);
        }
    }

    [Fact]
    public void FilterQueryOnTenantId()
    {
        try
        {
            _connection.Open();
            var tenant1 = new TenantContext("abc", "abc", "abc",
                "DataSource=testdb.db", null, null);
            using (var db = new TestDbContext(tenant1, _options))
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                var blog1 = new Blog { Title = "abc" };
                db.Blogs.Add(blog1);
                var post1 = new Post { Title = "post in abc", Blog = blog1 };
                db.Posts.Add(post1);
                db.SaveChanges();
            }

            var tenant2 = new TenantContext("123", "123", "123",
                "DataSource=testdb.db", null, null);
            using (var db = new TestDbContext(tenant2, _options))
            {
                var blog1 = new Blog { Title = "123" };
                db.Blogs.Add(blog1);
                var post1 = new Post { Title = "post in 123", Blog = blog1 };
                db.Posts.Add(post1);
                db.SaveChanges();
            }

            int postCount1 = 0;
            int postCount2 = 0;
            using (var db = new TestDbContext(tenant1, _options))
            {
                postCount1 = db.Posts.Count();
                postCount2 = db.Posts.IgnoreQueryFilters().Count();
            }

            Assert.Equal(1, postCount1);
            Assert.Equal(2, postCount2);
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
            var tenant1 = new TenantContext("abc", "abc", "abc",
                "DataSource=testdb.db", null, null);

            // TenantNotSetMode.Throw, should act as Overwrite when adding
            using (var db = new TestDbContext(tenant1, _options))
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
                db.TenantNotSetMode = TenantNotSetMode.Throw;

                var blog1 = new Blog { Title = "abc" };
                db.Blogs.Add(blog1);
                db.SaveChanges();
                Assert.Equal(tenant1.Identifier, db.Entry<Blog>(blog1).Property("TenantId").CurrentValue);
            }

            // TenantNotSetMode.Overwrite
            using (var db = new TestDbContext(tenant1, _options))
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
                db.TenantNotSetMode = TenantNotSetMode.Overwrite;

                var blog1 = new Blog { Title = "abc2" };
                db.Blogs.Add(blog1);
                db.SaveChanges();
                Assert.Equal(tenant1.Id, db.Entry<Blog>(blog1).Property("TenantId").CurrentValue);
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
            var tenant1 = new TenantContext("abc", "abc", "abc",
                "DataSource=testdb.db", null, null);

            // TenantMismatchMode.Throw
            using (var db = new TestDbContext(tenant1, _options))
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
                db.TenantMismatchMode = TenantMismatchMode.Throw;

                var blog1 = new Blog { Title = "abc" };
                db.Blogs.Add(blog1);
                db.Entry(blog1).Property("TenantId").CurrentValue = "77";

                var e = Assert.Throws<MultiTenantException>(() => db.SaveChanges());
            }

            // TenantMismatchMode.Ignore 
            using (var db = new TestDbContext(tenant1, _options))
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
                db.TenantMismatchMode = TenantMismatchMode.Ignore;

                var blog1 = new Blog { Title = "34" };
                db.Blogs.Add(blog1);
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
                db.Blogs.Add(blog1);
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
            var tenant1 = new TenantContext("abc", "abc", "abc",
                "DataSource=testdb.db", null, null);

            // TenantNotSetMode.Throw
            using (var db = new TestDbContext(tenant1, _options))
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                var blog1 = new Blog { Title = "abc" };
                db.Blogs.Add(blog1);
                db.SaveChanges();

                db.TenantNotSetMode = TenantNotSetMode.Throw;
                db.Entry(blog1).Property("TenantId").CurrentValue = null;

                var e = Assert.Throws<MultiTenantException>(() => db.SaveChanges());
            }

            // TenantNotSetMode.Overwrite
            using (var db = new TestDbContext(tenant1, _options))
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                var blog1 = new Blog { Title = "abc12" };
                db.Blogs.Add(blog1);
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
            var tenant1 = new TenantContext("abc", "abc", "abc",
                "DataSource=testdb.db", null, null);

            // TenantMismatchMode.Throw
            using (var db = new TestDbContext(tenant1, _options))
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                var blog1 = new Blog { Title = "abc" };
                db.Blogs.Add(blog1);
                db.SaveChanges();

                db.TenantMismatchMode = TenantMismatchMode.Throw;
                db.Entry(blog1).Property("TenantId").CurrentValue = "11";

                var e = Assert.Throws<MultiTenantException>(() => db.SaveChanges());
            }

            // TenantMismatchMode.Ignore
            using (var db = new TestDbContext(tenant1, _options))
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                var blog1 = new Blog { Title = "abc" };
                db.Blogs.Add(blog1);
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
                db.Blogs.Add(blog1);
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
            var tenant1 = new TenantContext("abc", "abc", "abc",
                "DataSource=testdb.db", null, null);

            // TenantNotSetMode.Throw
            using (var db = new TestDbContext(tenant1, _options))
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                var blog1 = new Blog { Title = "abc" };
                db.Blogs.Add(blog1);
                db.SaveChanges();

                db.TenantNotSetMode = TenantNotSetMode.Throw;
                db.Entry(blog1).Property("TenantId").CurrentValue = null;
                db.Blogs.Remove(blog1);

                var e = Assert.Throws<MultiTenantException>(() => db.SaveChanges());
            }

            // TenantNotSetMode.Overwrite
            using (var db = new TestDbContext(tenant1, _options))
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                var blog1 = new Blog { Title = "abc" };
                db.Blogs.Add(blog1);
                db.SaveChanges();

                db.TenantNotSetMode = TenantNotSetMode.Overwrite;
                db.Entry(blog1).Property("TenantId").CurrentValue = null;
                db.Blogs.Remove(blog1);

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
            var tenant1 = new TenantContext("abc", "abc", "abc",
                "DataSource=testdb.db", null, null);

            // TenantMismatchMode.Throw
            using (var db = new TestDbContext(tenant1, _options))
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                var blog1 = new Blog { Title = "abc" };
                db.Blogs.Add(blog1);
                db.SaveChanges();

                db.TenantMismatchMode = TenantMismatchMode.Throw;
                db.Entry(blog1).Property("TenantId").CurrentValue = "17";
                db.Blogs.Remove(blog1);

                var e = Assert.Throws<MultiTenantException>(() => db.SaveChanges());
            }

            // TenantMismatchMode.Ignore
            using (var db = new TestDbContext(tenant1, _options))
            {
                db.TenantMismatchMode = TenantMismatchMode.Ignore;
                var blog1 = db.Blogs.First();
                db.Entry(blog1).Property("TenantId").CurrentValue = "17";
                db.Blogs.Remove(blog1);

                Assert.Equal(1, db.SaveChanges());
            }

            // TenantMismatchMode.Overwrite
            using (var db = new TestDbContext(tenant1, _options))
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                var blog1 = new Blog { Title = "abc" };
                db.Blogs.Add(blog1);
                db.SaveChanges();

                db.TenantMismatchMode = TenantMismatchMode.Overwrite;
                db.Entry(blog1).Property("TenantId").CurrentValue = "17";
                db.Blogs.Remove(blog1);

                Assert.Equal(1, db.SaveChanges());
            }
        }
        finally
        {
            _connection.Close();
        }
    }

    [Fact]
    void SetTenantIdMaxLength()
    {
        try
        {
            _connection.Open();
            var tenant1 = new TenantContext("abc", "abc", "abc",
                    "DataSource=testdb.db", null, null);

            var _options = new DbContextOptionsBuilder<TestTenantIdConstraintsTypeDbContext>()
                .UseSqlite(_connection)
                .Options;

            using (var db = new TestTenantIdConstraintsTypeDbContext(tenant1, _options))
            {
                var entityType = db.Model.GetEntityTypes().Where(e=>e.ClrType == typeof(Post)).Single();
                var maxLength = entityType.FindProperty("TenantId").GetMaxLength();
                Assert.Equal(Constants.TenantIdMaxLength, maxLength);

                entityType = db.Model.GetEntityTypes().Where(e=>e.ClrType == typeof(ThingWithTenantId)).Single();
                maxLength = entityType.FindProperty("TenantId").GetMaxLength();
                Assert.Equal(Constants.TenantIdMaxLength, maxLength);

                entityType = db.Model.GetEntityTypes().Where(e=>e.ClrType == typeof(ThingWithHigherTenantIdMaxLength)).Single();
                maxLength = entityType.FindProperty("TenantId").GetMaxLength();
                Assert.Equal(Constants.TenantIdMaxLength, maxLength);

                entityType = db.Model.GetEntityTypes().Where(e=>e.ClrType == typeof(ThingWithLowerTenantIdMaxLength)).Single();
                maxLength = entityType.FindProperty("TenantId").GetMaxLength();
                Assert.Equal(Constants.TenantIdMaxLength, maxLength);        
            }
        }
        finally
        {
            _connection.Close();
        }
    }

    [Fact]
    void ThrowsIfExistingTenantIdWrongType()
    {
        try
        {

            _connection.Open();
            var tenant1 = new TenantContext("abc", "abc", "abc",
                    "DataSource=testdb.db", null, null);

            var _options = new DbContextOptionsBuilder<TestWrongTenantIdTypeDbContext>()
                .UseSqlite(_connection)
                .Options;

            using (var db = new TestWrongTenantIdTypeDbContext(tenant1, _options))
            {
                Assert.Throws<MultiTenantException>(() => db.Database.EnsureCreated());
            }
        }
        finally
        {
            _connection.Close();
        }
    }
}