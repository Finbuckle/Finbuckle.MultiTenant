//    Copyright 2019 Andrew White
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
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Stores;
using Microsoft.Extensions.Configuration;
using Xunit;

public class ConfigurationStoreShould : IMultiTenantStoreTestBase<ConfigurationStore>
{
    [Fact]
    public void ThrowIfNullConfiguration()
    {
        Assert.Throws<ArgumentNullException>(() => new ConfigurationStore(null));
    }

    [Fact]
    public void ThrowIfEmptyOrNullSection()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("ConfigurationStoreTestSettings.json");
        IConfiguration configuration = configBuilder.Build();

        Assert.Throws<ArgumentException>(() => new ConfigurationStore(configuration, ""));
        Assert.Throws<ArgumentException>(() => new ConfigurationStore(configuration, null));
    }

    [Fact]
    public void ThrowIfInvalidSection()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("ConfigurationStoreTestSettings.json");
        IConfiguration configuration = configBuilder.Build();

        Assert.Throws<MultiTenantException>(() => new ConfigurationStore(configuration, "invalid"));
    }

    [Fact]
    public void IgnoreCaseWhenGettingTenantInfoFromStoreByIdentifier()
    {
        var store = CreateTestStore();

        Assert.Equal("initech", store.TryGetByIdentifierAsync("INITECH").Result.Identifier);
    }

    // Basic store functionality tested in MultiTenantStoresShould.cs

    protected override IMultiTenantStore CreateTestStore()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("ConfigurationStoreTestSettings.json");
        var configuration = configBuilder.Build();

        return new ConfigurationStore(configuration);
    }

    protected override IMultiTenantStore PopulateTestStore(IMultiTenantStore store)
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
}