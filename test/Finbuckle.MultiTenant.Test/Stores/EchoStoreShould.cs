// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Globalization;
using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Stores;
using Xunit;

namespace Finbuckle.MultiTenant.Test.Stores;

public class EchoStoreShould
{
    [Fact]
    public async Task EchoIdentifierForStringTenantId()
    {
        var store = new EchoStore<TenantInfo<string>, string>(identifier => identifier);

        var byIdentifier = await store.GetByIdentifierAsync("initech");
        Assert.Equal("initech", byIdentifier!.Id);
        Assert.Equal("initech", byIdentifier.Identifier);

        var byId = await store.GetAsync("initech");
        Assert.Equal("initech", byId!.Id);
        Assert.Equal("initech", byId.Identifier);
    }

    [Fact]
    public async Task EchoIdentifierForIntTenantId()
    {
        var store = new EchoStore<TenantInfo<int>, int>(
            identifier => int.Parse(identifier, CultureInfo.InvariantCulture));

        var byIdentifier = await store.GetByIdentifierAsync("17");
        Assert.Equal(17, byIdentifier!.Id);
        Assert.Equal("17", byIdentifier.Identifier);

        // GetAsync fills the identifier from the id via ToString.
        var byId = await store.GetAsync(42);
        Assert.Equal(42, byId!.Id);
        Assert.Equal("42", byId.Identifier);
    }

    [Fact]
    public async Task EchoIdentifierForGuidTenantId()
    {
        var store = new EchoStore<TenantInfo<Guid>, Guid>(Guid.Parse);
        var guid = Guid.NewGuid();

        var byIdentifier = await store.GetByIdentifierAsync(guid.ToString());
        Assert.Equal(guid, byIdentifier!.Id);
        Assert.Equal(guid.ToString(), byIdentifier.Identifier);

        var byId = await store.GetAsync(guid);
        Assert.Equal(guid, byId!.Id);
        Assert.Equal(guid.ToString(), byId.Identifier);
    }

    [Fact]
    public void ThrowIfConversionDelegateIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new EchoStore<TenantInfo<int>, int>(null!));
    }
}
