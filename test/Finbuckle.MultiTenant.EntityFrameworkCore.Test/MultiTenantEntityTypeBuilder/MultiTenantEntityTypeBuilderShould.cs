// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System;
using System.Linq;
using Finbuckle.MultiTenant.EntityFrameworkCore;
using Finbuckle.MultiTenant.EntityFrameworkCore.Test.MultiTenantEntityTypeBuilder;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MultiTenantEntityTypeBuilderShould
{


    public class MultiTenantEntityTypeBuilderShould
    {
        private TestDbContext GetDbContext(Action<ModelBuilder> config)
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            var options = new DbContextOptionsBuilder().UseSqlite(connection)
                                                       .ReplaceService<IModelCacheKeyFactory,
                                                           DynamicModelCacheKeyFactory>() // needed for testing only
                                                       .Options;

            return new TestDbContext(config, options);
        }

        [Fact]
        public void AdjustIndexOnAdjustIndex()
        {
            IMutableIndex? origIndex = null;

            using (var db = GetDbContext(builder =>
                {
                    builder.Entity<Blog>().HasIndex(e => e.BlogId);

                    origIndex = builder.Entity<Blog>().Metadata.GetIndexes().First();
                    builder.Entity<Blog>().IsMultiTenant().AdjustIndex(origIndex);
                }))
            {
                var index = db.Model.FindEntityType(typeof(Blog))?.GetIndexes().First();
                Assert.Contains("BlogId", index!.Properties.Select(p => p.Name));
                Assert.Contains("TenantId", index.Properties.Select(p => p.Name));
            }
        }

        [Fact]
        public void PreserveIndexNameOnAdjustIndex()
        {
            IMutableIndex? origIndex = null;

            using (var db = GetDbContext(builder =>
                {
#if NET
                    builder.Entity<Blog>()
                           .HasIndex(e => e.BlogId, "CustomIndexName")
                           .HasDatabaseName("CustomIndexDbName");
#else
                    builder.Entity<Blog>().HasIndex(e => e.BlogId).HasName("CustomIndexName");
#endif
                    origIndex = builder.Entity<Blog>().Metadata.GetIndexes().First();
                    builder.Entity<Blog>().IsMultiTenant().AdjustIndex(origIndex);
                }))
            {
                var index = db.Model.FindEntityType(typeof(Blog))?.GetIndexes().First();
#if NET
                Assert.Equal("CustomIndexName", index!.Name);
                Assert.Equal("CustomIndexDbName", index.GetDatabaseName());
#elif NETCOREAPP3_1
                Assert.Equal("CustomIndexName", index.GetName());
#elif NETCOREAPP2_1
                Assert.Equal("CustomIndexName", index.Relational().Name);
#endif
            }
        }

        [Fact]
        public void PreserveIndexUniquenessOnAdjustIndex()
        {
            using (var db = GetDbContext(builder =>
                {
                    builder.Entity<Blog>().HasIndex(e => e.BlogId).IsUnique();
                    builder.Entity<Blog>().HasIndex(e => e.Url);

                    foreach (var index in builder.Entity<Blog>().Metadata.GetIndexes().ToList())
                        builder.Entity<Blog>().IsMultiTenant().AdjustIndex(index);
                }))
            {
                var index = db.Model.FindEntityType(typeof(Blog))?
                              .GetIndexes()
                              .Single(i => i.Properties.Select(p => p.Name).Contains("BlogId"));
                Assert.True(index!.IsUnique);
                index = db.Model.FindEntityType(typeof(Blog))?
                          .GetIndexes()
                          .Single(i => i.Properties.Select(p => p.Name).Contains("Url"));
                Assert.False(index!.IsUnique);
            }
        }

        [Fact]
        public void AdjustPrimaryKeyOnAdjustKey()
        {
            using (var db = GetDbContext(builder =>
                {
                    var key = builder.Entity<Post>().Metadata.GetKeys().First();
                    builder.Entity<Post>().IsMultiTenant().AdjustKey(key, builder);
                }))
            {
                var key = db.Model.FindEntityType(typeof(Post))?.GetKeys().ToList();

                Assert.Single(key!);
                Assert.Equal(2, key![0].Properties.Count);
                Assert.Contains("PostId", key[0].Properties.Select(p => p.Name));
                Assert.Contains("TenantId", key[0].Properties.Select(p => p.Name));
            }
        }

        [Fact]
        public void AdjustDependentForeignKeyOnAdjustPrimaryKey()
        {
            using (var db = GetDbContext(builder =>
                {
                    var key = builder.Entity<Blog>().Metadata.GetKeys().First();

                    builder.Entity<Blog>().IsMultiTenant().AdjustKey(key, builder);
                }))
            {
                var key = db.Model.FindEntityType(typeof(Post))?.GetForeignKeys().ToList();

                Assert.Single(key!);
                Assert.Equal(2, key![0].Properties.Count);
                Assert.Contains("BlogId", key[0].Properties.Select(p => p.Name));
                Assert.Contains("TenantId", key[0].Properties.Select(p => p.Name));
            }
        }

        [Fact]
        public void AdjustAlternateKeyOnAdjustKey()
        {
            using (var db = GetDbContext(builder =>
                {
                    var key = builder.Entity<Blog>().HasAlternateKey(b => b.Url).Metadata;
                    builder.Entity<Blog>().IsMultiTenant().AdjustKey(key, builder);
                }))
            {
                var key = db.Model.FindEntityType(typeof(Blog))?.GetKeys().Where(k => !k.IsPrimaryKey()).ToList();

                Assert.Single(key!);
                Assert.Equal(2, key![0].Properties.Count);
                Assert.Contains("Url", key[0].Properties.Select(p => p.Name));
                Assert.Contains("TenantId", key[0].Properties.Select(p => p.Name));
            }
        }

        [Fact]
        public void AdjustDependentForeignKeyOnAdjustAlternateKey()
        {
            using (var db = GetDbContext(builder =>
                {
                    var key = builder.Entity<Blog>().HasAlternateKey(b => b.Url).Metadata;
                    builder.Entity<Post>()
                           .HasOne(p => p.Blog!)
                           .WithMany(b => b.Posts!)
                           .HasForeignKey(p => p.Title) // Since Title is a string lets use it as key to Blog.Url
                           .HasPrincipalKey(b => b.Url);
                    builder.Entity<Blog>().IsMultiTenant().AdjustKey(key, builder);
                }))
            {
                var key = db.Model.FindEntityType(typeof(Post))?.GetForeignKeys().ToList();

                Assert.Single(key!);
                Assert.Equal(2, key![0].Properties.Count);
                Assert.Contains("Title", key[0].Properties.Select(p => p.Name));
                Assert.Contains("TenantId", key[0].Properties.Select(p => p.Name));
            }
        }
    }
}