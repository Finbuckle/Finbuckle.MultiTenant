// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Net;
using System.Reflection;
using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Stores;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Finbuckle.MultiTenant.Test.Stores;

public class HttpRemoteStoreShould : MultiTenantStoreTestBase
{
    public class TestHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var result = new HttpResponseMessage();

            var numSegments = request.RequestUri!.Segments.Length;
            if (string.Equals(request.RequestUri.Segments[numSegments - 1], "initech",
                    StringComparison.OrdinalIgnoreCase))
            {
                var tenantInfo = new TenantInfo{Id= "initech-id", Identifier= "initech"};
                var json = JsonConvert.SerializeObject(tenantInfo);
                result.StatusCode = HttpStatusCode.OK;
                result.Content = new StringContent(json);
            }
            else
                result.StatusCode = HttpStatusCode.NotFound;

            return Task.FromResult(result);
        }
    }

    [Fact]
    public void ThrowIfTypedClientParamIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new HttpRemoteStore<TenantInfo>(null!, "http://example.com"));
    }

    [Theory]
    [InlineData("null")]
    [InlineData("")]
    [InlineData("invalidUri")]
    [InlineData("file://nothttp")]
    public void ThrowIfEndpointTemplateIsNotWellFormed(string uri)
    {
        var clientFactory = new Mock<IHttpClientFactory>();
        var client = new HttpRemoteStoreClient<TenantInfo>(clientFactory.Object);
        Assert.Throws<ArgumentException>(() => new HttpRemoteStore<TenantInfo>(client, uri));
    }

    [Fact]
    public void AppendTenantToTemplateIfMissing()
    {
        var clientFactory = new Mock<IHttpClientFactory>();
        var client = new HttpRemoteStoreClient<TenantInfo>(clientFactory.Object);
        var store = new HttpRemoteStore<TenantInfo>(client, "http://example.com/");

        var field = store.GetType().GetField("endpointTemplate", BindingFlags.NonPublic | BindingFlags.Instance);
        var endpointTemplate = field?.GetValue(store);

        Assert.Equal($"http://example.com/{HttpRemoteStore<TenantInfo>.DefaultEndpointTemplateIdentifierToken}",
            endpointTemplate);
    }

    [Fact]
    public void AppendTenantWithSlashToTemplateIfMissing()
    {
        var clientFactory = new Mock<IHttpClientFactory>();
        var client = new HttpRemoteStoreClient<TenantInfo>(clientFactory.Object);
        var store = new HttpRemoteStore<TenantInfo>(client, "http://example.com");

        var field = store.GetType().GetField("endpointTemplate", BindingFlags.NonPublic | BindingFlags.Instance);
        var endpointTemplate = field?.GetValue(store);

        Assert.Equal($"http://example.com/{HttpRemoteStore<TenantInfo>.DefaultEndpointTemplateIdentifierToken}",
            endpointTemplate);
    }

    // Basic store functionality tested in MultiTenantStoresShould.cs

    protected override Task<IMultiTenantStore<TenantInfo>> CreateTestStore()
    {
        var client = new HttpClient(new TestHandler());
        var clientFactory = new Mock<IHttpClientFactory>();
        clientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);
        var typedClient = new HttpRemoteStoreClient<TenantInfo>(clientFactory.Object);
        return Task.FromResult<IMultiTenantStore<TenantInfo>>(new HttpRemoteStore<TenantInfo>(typedClient, "http://example.com"));
    }

    protected override Task<IMultiTenantStore<TenantInfo>> PopulateTestStore(IMultiTenantStore<TenantInfo> store)
    {
        throw new NotImplementedException();
    }

    // [Fact(Skip = "Not valid for this store.")]
    public override Task GetTenantInfoFromStoreById()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public override async Task GetTenantInfoFromStoreByIdentifier()
    {
        await base.GetTenantInfoFromStoreByIdentifier();
    }

    [Fact]
    public override async Task ReturnNullWhenGettingByIdentifierIfTenantInfoNotFound()
    {
        await base.ReturnNullWhenGettingByIdentifierIfTenantInfoNotFound();
    }

    // [Fact(Skip = "Not valid for this store.")]
    public override Task AddTenantInfoToStore()
    {
        return Task.CompletedTask;
    }

    // [Fact(Skip = "Not valid for this store.")]
    public override Task RemoveTenantInfoFromStore()
    {
        return Task.CompletedTask;
    }

    // [Fact(Skip = "Not valid for this store.")]
    public override Task UpdateTenantInfoInStore()
    {
        return Task.CompletedTask;
    }
}