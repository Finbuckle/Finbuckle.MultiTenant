// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Test.Extensions
{
    public class MultiTenantEntityTypeBuilderExtensionsShould : IDisposable
    {
        public class TestDbContext : MultiTenantDbContext
        {
            private readonly Action<ModelBuilder> _config;

            public TestDbContext(Action<ModelBuilder> config, DbContextOptions options) : base(
                new TenantInfo {Id = "dummy"},
                options)
            {
                this._config = config;
            }

            public DbSet<Blog> Blogs { get; set; }
            public DbSet<Post> Posts { get; set; }

            protected override void OnModelCreating(ModelBuilder builder)
            {
                _config(builder);
            }
        }

        public class Blog
        {
            public int BlogId { get; set; }
            public string Url { get; set; }

            public List<Post> Posts { get; set; }
        }

        public class Post
        {
            public int PostId { get; set; }
            public string Title { get; set; }
            public string Content { get; set; }

            public Blog Blog { get; set; }
        }

        private readonly SqliteConnection _connection = new SqliteConnection("DataSource=:memory:");

        private TestDbContext GetDbContext(Action<ModelBuilder> config)
        {
            var options = new DbContextOptionsBuilder().UseSqlite(_connection).Options;
            var db = new TestDbContext(config, options);
            return db;
        }

        public void Dispose()
        {
            _connection.Dispose();
        }

        [Fact]
        public void AdjustUniqueIndexesOnAdjustUniqueIndexes()
        {
            using (var db = GetDbContext(builder =>
                {
#if NET
                    builder.Entity<Blog>()
                           .HasIndex(e => e.BlogId, nameof(Blog.BlogId))
                           .HasDatabaseName(nameof(Blog.BlogId) + "DbName")
                           .IsUnique();
                    builder.Entity<Blog>()
                           .HasIndex(e => e.Url, nameof(Blog.Url))
                           .HasDatabaseName(nameof(Blog.Url) + "DbName")
                           .IsUnique();
#else
                    builder.Entity<Blog>().HasIndex(e => e.BlogId).HasName(nameof(Blog.BlogId)).IsUnique();
                    builder.Entity<Blog>().HasIndex(e => e.Url).HasName(nameof(Blog.Url)).IsUnique();
#endif
                    builder.Entity<Blog>().IsMultiTenant().AdjustUniqueIndexes();
                }))
            {
                var indexes = db.Model.FindEntityType(typeof(Blog)).GetIndexes().Where(i => i.IsUnique);

                foreach (var index in indexes.Where(i => i.IsUnique))
                {
                    Assert.Contains("TenantId", index.Properties.Select(p => p.Name));
                }
            }
        }

        [Fact]
        public void NotAdjustNonUniqueIndexesOnAdjustUniqueIndexes()
        {
            using (var db = GetDbContext(builder =>
                {
#if NET
                    builder.Entity<Blog>()
                           .HasIndex(e => e.BlogId, nameof(Blog.BlogId))
                           .HasDatabaseName(nameof(Blog.BlogId) + "DbName")
                           .IsUnique();
                    builder.Entity<Blog>()
                           .HasIndex(e => e.Url, nameof(Blog.Url))
                           .HasDatabaseName(nameof(Blog.Url) + "DbName");
#else
                    builder.Entity<Blog>().HasIndex(e => e.BlogId).HasName(nameof(Blog.BlogId)).IsUnique();
                    builder.Entity<Blog>().HasIndex(e => e.Url).HasName(nameof(Blog.Url));
#endif
                    builder.Entity<Blog>().IsMultiTenant().AdjustUniqueIndexes();
                }))
            {
                var indexes = db.Model.FindEntityType(typeof(Blog)).GetIndexes().Where(i => i.IsUnique);

                foreach (var index in indexes.Where(i => !i.IsUnique))
                {
                    Assert.DoesNotContain("TenantId", index.Properties.Select(p => p.Name));
                }
            }
        }

        [Fact]
        public void AdjustAllIndexesOnAdjustIndexes()
        {
            using (var db = GetDbContext(builder =>
                {
#if NET
                    builder.Entity<Blog>()
                           .HasIndex(e => e.BlogId, nameof(Blog.BlogId))
                           .HasDatabaseName(nameof(Blog.BlogId) + "DbName")
                           .IsUnique();
                    builder.Entity<Blog>()
                           .HasIndex(e => e.Url, nameof(Blog.Url))
                           .HasDatabaseName(nameof(Blog.Url) + "DbName");
#else
                    builder.Entity<Blog>().HasIndex(e => e.BlogId).HasName(nameof(Blog.BlogId)).IsUnique();
                    builder.Entity<Blog>().HasIndex(e => e.Url).HasName(nameof(Blog.Url));
#endif
                    builder.Entity<Blog>().IsMultiTenant().AdjustIndexes();
                }))
            {
                var indexes = db.Model.FindEntityType(typeof(Blog)).GetIndexes().Where(i => i.IsUnique);

                foreach (var index in indexes)
                {
                    Assert.Contains("TenantId", index.Properties.Select(p => p.Name));
                }
            }
        }
    }
}