// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Test.Extensions
{
    public class EntityTypeBuilderExtensionsShould : IDisposable
    {
        public class TestIdentityDbContext : MultiTenantIdentityDbContext
        {
            public TestIdentityDbContext(TenantInfo tenantInfo, DbContextOptions options)
                : base(tenantInfo, options)
            {
            }
        }
        
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        public class TestDbContext : DbContext
        {
            private readonly Action<ModelBuilder> _config;

            public TestDbContext(Action<ModelBuilder> config, DbContextOptions options) : base(options)
            {
                this._config = config;
            }

            DbSet<MyMultiTenantThing> MyMultiTenantThings { get; set; }
            DbSet<MyThingWithTenantId> MyThingsWithTenantIds { get; set; }
            DbSet<MyThingWithIntTenantId> MyThingsWithIntTenantId { get; set; }
            DbSet<MyMultiTenantThingWithAttribute> MyMultiTenantThingsWithAttribute { get; set; }

            protected override void OnModelCreating(ModelBuilder builder)
            {
                // If the test passed in a custom builder use it
                if (_config != null)
                    _config(builder);
                // Of use the standard builder configuration
                else
                {
                    builder.Entity<MyMultiTenantThing>().IsMultiTenant();
                    builder.Entity<MyThingWithTenantId>().IsMultiTenant();
                    
                    // for MyMultiTenantThingWithAttribute
                    builder.ConfigureMultiTenant();
                }
            }
        }

        public class MyMultiTenantThing
        {
            public int Id { get; set; }
        }

        [MultiTenant]
        public class MyMultiTenantThingWithAttribute
        {
            public int Id { get; set; }
        }

        public class MyThingWithTenantId
        {
            public int Id { get; set; }
            public string TenantId { get; set; }
        }

        public class MyThingWithIntTenantId
        {
            public int Id { get; set; }
            public int TenantId { get; set; }
        }

        private readonly SqliteConnection _connection = new SqliteConnection("DataSource=:memory:");

        public void Dispose()
        {
            _connection.Dispose();
        }

        private DbContext GetDbContext(Action<ModelBuilder> config = null)
        {
            _connection.Open(); 
            var options = new DbContextOptionsBuilder()
                .UseSqlite(_connection)
                #if NETCOREAPP2_1
                .ReplaceService<IModelCacheKeyFactory, DynamicModelCacheKeyFactory>() // needed for testing only
                #endif
                .Options;
            return new TestDbContext(config, options);
        }

        private TestIdentityDbContext GetTestIdentityDbContext(TenantInfo tenant)
        {
            _connection.Open();
            var options = new DbContextOptionsBuilder()
                    .UseSqlite(_connection)
                    .Options;
            return new TestIdentityDbContext(tenant, options);
        }

        [Fact]
        public void SetMultiTenantAnnotation()
        {
            using (var db = GetDbContext())
            {
                var annotation = db.Model.FindEntityType(typeof(MyMultiTenantThing)).FindAnnotation(Constants.MultiTenantAnnotationName);

                Assert.True((bool)annotation.Value);
            }
        }

        [Fact]
        public void AddTenantIdStringShadowProperty()
        {
            using (var db = GetDbContext())
            {
                var prop = db.Model.FindEntityType(typeof(MyMultiTenantThing)).FindProperty("TenantId");

                Assert.Equal(typeof(string), prop.ClrType);
                // IsShadowProperty doesn't work?
                // Assert.True(prop.IsShadowProperty());
                Assert.Null(prop.FieldInfo);
            }
        }

        [Fact]
        public void RespectExistingTenantIdStringProperty()
        {
            using (var db = GetDbContext())
            {
                var prop = db.Model.FindEntityType(typeof(MyThingWithTenantId)).FindProperty("TenantId");

                Assert.Equal(typeof(string), prop.ClrType);
                // TODO: IsShadowProperty doesn't work?
                // Assert.False(prop.IsShadowProperty());
                Assert.NotNull(prop.FieldInfo);
            }
        }

        [Fact]
        public void ThrowOnNonStringExistingTenantIdProperty()
        {
            using (var db = GetDbContext(b => b.Entity<MyThingWithIntTenantId>().IsMultiTenant()))
            {
                Assert.Throws<MultiTenantException>(() => db.Model);
            }
        }

        [Fact]
        public void SetsTenantIdStringMaxLength()
        {
            using (var db = GetDbContext())
            {
                var prop = db.Model.FindEntityType(typeof(MyMultiTenantThing)).FindProperty("TenantId");

                Assert.Equal(Internal.Constants.TenantIdMaxLength, prop.GetMaxLength());
            }
        }

        [Fact]
        public void SetGlobalFilterQuery()
        {
            // Doesn't appear to be a way to test this except to try it out...
            try
            {
                _connection.Open();
                var options = new DbContextOptionsBuilder()
                    .UseSqlite(_connection)
                    .Options;
                var tenant1 = new TenantInfo
                {
                    Id = "abc",
                    Identifier = "abc",
                    Name = "abc",
                    ConnectionString = "DataSource=testDb.db"
                };

                using (var db = new TestBlogDbContext(tenant1, options))
                {
                    db.Database.EnsureDeleted();
                    db.Database.EnsureCreated();

                    var blog1 = new Blog { Title = "abc" };
                    db.Blogs.Add(blog1);
                    var post1 = new Post { Title = "post in abc", Blog = blog1 };
                    db.Posts.Add(post1);
                    db.SaveChanges();
                }

                var tenant2 = new TenantInfo
                {
                    Id = "123",
                    Identifier = "123",
                    Name = "123",
                    ConnectionString = "DataSource=testDb.db"
                };
                using (var db = new TestBlogDbContext(tenant2, options))
                {
                    var blog1 = new Blog { Title = "123" };
                    db.Blogs.Add(blog1);
                    var post1 = new Post { Title = "post in 123", Blog = blog1 };
                    db.Posts.Add(post1);
                    db.SaveChanges();
                }

                int postCount1 = 0;
                int postCount2 = 0;
                using (var db = new TestBlogDbContext(tenant1, options))
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
        public void RespectExistingQueryFilter()
        {
            // Doesn't appear to be a way to test this except to try it out...
            var options = new DbContextOptionsBuilder()
                    .UseSqlite(_connection)
                    .Options;
            try
            {
                _connection.Open();
                var tenant1 = new TenantInfo
                {
                    Id = "abc",
                    Identifier = "abc",
                    Name = "abc",
                    ConnectionString = "DataSource=testDb.db"
                };

                using (var db = new TestDbContextWithExistingGlobalFilter(tenant1, options))
                {
                    db.Database.EnsureDeleted();
                    db.Database.EnsureCreated();

                    var blog1 = new Blog { Title = "abc" };
                    db.Blogs.Add(blog1);
                    var post1 = new Post { Title = "post in abc", Blog = blog1 };
                    db.Posts.Add(post1);
                    var post2 = new Post { Title = "Filtered Title", Blog = blog1 };
                    db.Posts.Add(post2);
                    db.SaveChanges();
                }

                int postCount1 = 0;
                int postCount2 = 0;
                using (var db = new TestDbContextWithExistingGlobalFilter(tenant1, options))
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

        // The tests below are Identity specific
        [Fact]
        public void AdjustRoleIndex()
        {
            var tenant1 = new TenantInfo
            {
                Id = "abc",
                Identifier = "abc",
                Name = "abc",
                ConnectionString = "DataSource=testDb.db"
            };

            using (var c = GetTestIdentityDbContext(tenant1))
            {
                var props = new List<IProperty>();
                props.Add(c.Model.FindEntityType(typeof(IdentityRole)).FindProperty("NormalizedName"));
                props.Add(c.Model.FindEntityType(typeof(IdentityRole)).FindProperty("TenantId"));

                var index = c.Model.FindEntityType(typeof(IdentityRole)).FindIndex(props);
                Assert.NotNull(index);
                Assert.True(index.IsUnique);
            }
        }

        [Fact]
        public void AdjustUserLoginKey()
        {
            var tenant1 = new TenantInfo
            {
                Id = "abc",
                Identifier = "abc",
                Name = "abc",
                ConnectionString = "DataSource=testDb.db"
            };

            using (var c = GetTestIdentityDbContext(tenant1))
            {
                Assert.True(c.Model.FindEntityType(typeof(IdentityUserLogin<string>)).FindProperty("Id").IsPrimaryKey());
            }
        }

        [Fact]
        public void AddUserLoginIndex()
        {
            var tenant1 = new TenantInfo
            {
                Id = "abc",
                Identifier = "abc",
                Name = "abc",
                ConnectionString = "DataSource=testDb.db"
            };

            using (var c = GetTestIdentityDbContext(tenant1))
            {
                var props = new List<IProperty>();
                props.Add(c.Model.FindEntityType(typeof(IdentityUserLogin<string>)).FindProperty("LoginProvider"));
                props.Add(c.Model.FindEntityType(typeof(IdentityUserLogin<string>)).FindProperty("ProviderKey"));
                props.Add(c.Model.FindEntityType(typeof(IdentityUserLogin<string>)).FindProperty("TenantId"));

                var index = c.Model.FindEntityType(typeof(IdentityUserLogin<string>)).FindIndex(props);
                Assert.NotNull(index);
                Assert.True(index.IsUnique);
            }
        }

        [Fact]
        public void AdjustUserIndex()
        {
            var tenant1 = new TenantInfo
            {
                Id = "abc",
                Identifier = "abc",
                Name = "abc",
                ConnectionString = "DataSource=testDb.db"
            };

            using (var c = GetTestIdentityDbContext(tenant1))
            {
                var props = new List<IProperty>();
                props.Add(c.Model.FindEntityType(typeof(IdentityUser)).FindProperty("NormalizedUserName"));
                props.Add(c.Model.FindEntityType(typeof(IdentityUser)).FindProperty("TenantId"));

                var index = c.Model.FindEntityType(typeof(IdentityUser)).FindIndex(props);
                Assert.NotNull(index);
                Assert.True(index.IsUnique);
            }
        }
    }
}
