//    Copyright 2018-2020 Andrew White
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
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Stores;
using Moq;
using Newtonsoft.Json;
using Xunit;

public class HttpRemoteStoreShould : IMultiTenantStoreTestBase<HttpRemoteStore<TenantInfo>>
{
    public class TestHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var result = new HttpResponseMessage();

            var numSegments = request.RequestUri.Segments.Length;
            if (string.Equals(request.RequestUri.Segments[numSegments - 1], "initech", StringComparison.OrdinalIgnoreCase))
            {

                var tenantInfo = new TenantInfo { Id = "initech-id", Identifier = "initech" };
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
        Assert.Throws<ArgumentNullException>(() => new HttpRemoteStore<TenantInfo>(null, "http://example.com"));
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
        var endpointTemplate = field.GetValue(store);

        Assert.Equal($"http://example.com/{HttpRemoteStore<TenantInfo>.defaultEndpointTemplateIdentifierToken}", endpointTemplate);
    }

    [Fact]
    public void AppendTenantWithSlashToTemplateIfMissing()
    {
        var clientFactory = new Mock<IHttpClientFactory>();
        var client = new HttpRemoteStoreClient<TenantInfo>(clientFactory.Object);
        var store = new HttpRemoteStore<TenantInfo>(client, "http://example.com");

        var field = store.GetType().GetField("endpointTemplate", BindingFlags.NonPublic | BindingFlags.Instance);
        var endpointTemplate = field.GetValue(store);

        Assert.Equal($"http://example.com/{HttpRemoteStore<TenantInfo>.defaultEndpointTemplateIdentifierToken}", endpointTemplate);
    }

    // Basic store functionality tested in MultiTenantStoresShould.cs

    protected override IMultiTenantStore<TenantInfo> CreateTestStore()
    {
        var client = new HttpClient(new TestHandler());
        var clientFactory = new Mock<IHttpClientFactory>();
        clientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);
        var typedClient = new HttpRemoteStoreClient<TenantInfo>(clientFactory.Object);
        return new HttpRemoteStore<TenantInfo>(typedClient, "http://example.com");
    }

    protected override IMultiTenantStore<TenantInfo> PopulateTestStore(IMultiTenantStore<TenantInfo> store)
    {
        throw new NotImplementedException();
    }

    // [Fact(Skip = "Not valid for this store.")]
    public override void GetTenantInfoFromStoreById()
    {
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

    // [Fact(Skip = "Not valid for this store.")]
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