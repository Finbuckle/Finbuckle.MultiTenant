// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Xunit;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Test.Extensions.MultiTenantEntityTypeBuilderExtensions
{
    public class MultiTenantEntityTypeBuilderExtensionsShould
    {
        private TestDbContext GetDbContext(Action<ModelBuilder> config)
        {
            var options = new DbContextOptionsBuilder()
                          .ReplaceService<IModelCacheKeyFactory, DynamicModelCacheKeyFactory>()
                          .Options;
            var db = new TestDbContext(config, options);
            return db;
        }

        [Fact]
        public void AdjustUniqueIndexesOnAdjustUniqueIndexes()
        {
            using var db = GetDbContext(builder =>
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
            });
            var indexes = db.Model.FindEntityType(typeof(Blog)).GetIndexes().Where(i => i.IsUnique);

            foreach (var index in indexes.Where(i => i.IsUnique))
            {
                Assert.Contains("TenantId", index.Properties.Select(p => p.Name));
            }
        }

        [Fact]
        public void NotAdjustNonUniqueIndexesOnAdjustUniqueIndexes()
        {
            using var db = GetDbContext(builder =>
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
            });
            var indexes = db.Model.FindEntityType(typeof(Blog)).GetIndexes().Where(i => i.IsUnique);

            foreach (var index in indexes.Where(i => !i.IsUnique))
            {
                Assert.DoesNotContain("TenantId", index.Properties.Select(p => p.Name));
            }
        }

        [Fact]
        public void AdjustAllIndexesOnAdjustIndexes()
        {
            using var db = GetDbContext(builder =>
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
            });
            var indexes = db.Model.FindEntityType(typeof(Blog)).GetIndexes().Where(i => i.IsUnique);

            foreach (var index in indexes)
            {
                Assert.Contains("TenantId", index.Properties.Select(p => p.Name));
            }
        }
    }
}