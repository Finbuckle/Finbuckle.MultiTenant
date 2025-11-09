// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.EntityFrameworkCore.Stores.EFCoreStore;
using Finbuckle.MultiTenant.Test.Stores;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Test.Stores;

public class EfCoreStoreShould
    : MultiTenantStoreTestBase, IDisposable
{
    public class TestEfCoreStoreDbContext : EFCoreStoreDbContext<TenantInfo>
    {
        public TestEfCoreStoreDbContext(DbContextOptions options) : base(options)
        {
        }
    }

    private readonly SqliteConnection _connection = new SqliteConnection("DataSource=:memory:");

    public void Dispose()
    {
        _connection.Dispose();
    }

    private IProperty? GetModelProperty(string propName)
    {
        _connection.Open();
        var options = new DbContextOptionsBuilder().UseSqlite(_connection).Options;
        var dbContext = new TestEfCoreStoreDbContext(options);

        var model = dbContext.Model.FindEntityType(typeof(TenantInfo));
        var prop = model?.GetProperties().SingleOrDefault(p => p.Name == propName);
        return prop;
    }

    protected override IMultiTenantStore<TenantInfo> CreateTestStore()
    {
        _connection.Open();
        var options = new DbContextOptionsBuilder().UseSqlite(_connection).Options;
        var dbContext = new TestEfCoreStoreDbContext(options);
        dbContext.Database.EnsureCreated();

        var store = new EFCoreStore<TestEfCoreStoreDbContext, TenantInfo>(dbContext);
        return PopulateTestStore(store);
    }
    
    protected override IMultiTenantStore<TenantInfo> PopulateTestStore(IMultiTenantStore<TenantInfo> store)
    {
        return base.PopulateTestStore(store);
    }

    [Fact]
    public void AddTenantIdAsKey()
    {
        var prop = GetModelProperty("Id");
        Assert.True(prop!.IsPrimaryKey());
    }

    [Fact]
    public void AddIdentifierUniqueConstraint()
    {
        var prop = GetModelProperty("Identifier");
        Assert.True(prop!.IsIndex());
    }

    [Fact]
    public async Task NotTrackContextOnGet()
    {
        var store = (EFCoreStore<TestEfCoreStoreDbContext, TenantInfo>)CreateTestStore();
        var tenant = await store.GetAsync("initech-id");

        var entity = store.dbContext.Entry(tenant!);
        Assert.Equal(EntityState.Detached, entity.State);
    }

    [Fact]
    public async Task NotTrackContextOnGetByIdentifier()
    {
        var store = (EFCoreStore<TestEfCoreStoreDbContext, TenantInfo>)CreateTestStore();
        var tenant = await store.GetByIdentifierAsync("initech");

        var entity = store.dbContext.Entry(tenant!);
        Assert.Equal(EntityState.Detached, entity.State);
    }

    [Fact]
    public async Task NotTrackContextOnGetAll()
    {
        var store = (EFCoreStore<TestEfCoreStoreDbContext, TenantInfo>)CreateTestStore();
        var tenant = (await store.GetAllAsync()).First();

        var entity = store.dbContext.Entry(tenant);
        Assert.Equal(EntityState.Detached, entity.State);
    }

    [Fact]
    public async Task NotTrackContextOnAdd()
    {
        var store = (EFCoreStore<TestEfCoreStoreDbContext, TenantInfo>)CreateTestStore();
        var tenant = new TenantInfo(Id: "test-id", Identifier: "test-identifier", Name: "test");
        await store.AddAsync(tenant);

        var entity = store.dbContext.Entry(tenant);
        Assert.Equal(EntityState.Detached, entity.State);
    }

    [Fact]
    public async Task NotTrackContextOnUpdate()
    {
        var store = (EFCoreStore<TestEfCoreStoreDbContext, TenantInfo>)CreateTestStore();
        var tenant = await store.GetByIdentifierAsync("initech");
        tenant = tenant! with { Name = "new name" };
        await store.UpdateAsync(tenant);

        var entity = store.dbContext.Entry(tenant);
        Assert.Equal(EntityState.Detached, entity.State);
    }

    [Fact]
    public async Task NotTrackContextOnRemove()
    {
        var store = (EFCoreStore<TestEfCoreStoreDbContext, TenantInfo>)CreateTestStore();
        var tenant = await store.GetByIdentifierAsync("initech");
        tenant = tenant! with { Name = "new name" };
        await store.RemoveAsync(tenant.Id);

        var entity = store.dbContext.Entry(tenant);
        Assert.Equal(EntityState.Detached, entity.State);
    }

    // Basic store functionality tested in MultiTenantStoresShould.cs

    [Fact]
    public override void GetTenantInfoFromStoreById()
    {
        base.GetTenantInfoFromStoreById();
    }

    [Fact]
    public override void ReturnNullWhenGettingByIdIfTenantInfoNotFound()
    {
        base.ReturnNullWhenGettingByIdIfTenantInfoNotFound();
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
    public override void AddTenantInfoToStore()
    {
        base.AddTenantInfoToStore();
    }

    [Fact]
    public override void RemoveTenantInfoFromStore()
    {
        base.RemoveTenantInfoFromStore();
    }

    [Fact]
    public override void UpdateTenantInfoInStore()
    {
        base.UpdateTenantInfoInStore();
    }

    [Fact]
    public override void GetAllTenantsFromStoreAsync()
    {
        base.GetAllTenantsFromStoreAsync();
    }
}