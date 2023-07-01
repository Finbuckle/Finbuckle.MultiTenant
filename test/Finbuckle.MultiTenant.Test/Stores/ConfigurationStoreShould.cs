// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System;
using System.Threading.Tasks;
using Finbuckle.MultiTenant.Stores;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Finbuckle.MultiTenant.Test.Stores
{
    public class ConfigurationStoreShould : MultiTenantStoreTestBase
    {
        [Fact]
        public void NotThrowIfNoDefaultSection()
        {
            // See https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/426
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddJsonFile("ConfigurationStoreTestSettings_NoDefaults.json");
            IConfiguration configuration = configBuilder.Build();

            // ReSharper disable once ObjectCreationAsStatement
            // Will throw if fail
            new ConfigurationStore<TenantInfo>(configuration);
        }

        [Fact]
        public void ThrowIfNullConfiguration()
        {
            Assert.Throws<ArgumentNullException>(() => new ConfigurationStore<TenantInfo>(null!));
        }

        [Fact]
        public void ThrowIfEmptyOrNullSection()
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddJsonFile("ConfigurationStoreTestSettings.json");
            IConfiguration configuration = configBuilder.Build();

            Assert.Throws<ArgumentException>(() => new ConfigurationStore<TenantInfo>(configuration, ""));
            Assert.Throws<ArgumentException>(() => new ConfigurationStore<TenantInfo>(configuration, null!));
        }

        [Fact]
        public void ThrowIfInvalidSection()
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddJsonFile("ConfigurationStoreTestSettings.json");
            IConfiguration configuration = configBuilder.Build();

            Assert.Throws<MultiTenantException>(() => new ConfigurationStore<TenantInfo>(configuration, "invalid"));
        }

        [Fact]
        public void IgnoreCaseWhenGettingTenantInfoFromStoreByIdentifier()
        {
            var store = CreateTestStore();

            Assert.Equal("initech", store.TryGetByIdentifierAsync("INITECH").Result!.Identifier);
        }

        [Fact]
        public void ThrowWhenTryingToGetIdentifierGivenNullIdentifier()
        {
            var store = CreateTestStore();

            Assert.ThrowsAsync<ArgumentNullException>(async () => await store.TryGetByIdentifierAsync(null!));
        }

        // Basic store functionality tested in MultiTenantStoresShould.cs

        protected override IMultiTenantStore<TenantInfo> CreateTestStore()
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddJsonFile("ConfigurationStoreTestSettings.json");
            var configuration = configBuilder.Build();

            return new ConfigurationStore<TenantInfo>(configuration);
        }

        protected override IMultiTenantStore<TenantInfo> PopulateTestStore(IMultiTenantStore<TenantInfo> store)
        {
            throw new NotImplementedException();
        }

        [Fact]
        public override void GetTenantInfoFromStoreById()
        {
            base.GetTenantInfoFromStoreById();
        }

        [Fact]
        public override void GetTenantInfoFromStoreByIdentifier()
        {
            base.GetTenantInfoFromStoreByIdentifier();
        }

        [Fact]
        public override void ReturnNullWhenGettingByIdentifierIfTenantInfoNotFound()
        {
            base.ReturnNullWhenGettingByIdentifierIfTenantInfoNotFound();
        }

        [Fact]
        public override void ReturnNullWhenGettingByIdIfTenantInfoNotFound()
        {
            base.ReturnNullWhenGettingByIdIfTenantInfoNotFound();
        }

        // [Fact(Skip = "Not valid for this store.")]
        public override void AddTenantInfoToStore()
        {
        }

        // [Fact(Skip = "Not valid for this store.")]
        public override void RemoveTenantInfoFromStore()
        {
        }

        // [Fact(Skip = "Not valid for this store.")]
        public override void UpdateTenantInfoInStore()
        {
        }

        [Fact]
        public override void GetAllTenantsFromStoreAsync()
        {
            base.GetAllTenantsFromStoreAsync();
        }
    }
}