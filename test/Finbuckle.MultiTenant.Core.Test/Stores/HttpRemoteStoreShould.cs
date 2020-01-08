﻿//    Copyright 2019 Andrew White
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
using System.Threading;
using System.Threading.Tasks;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Stores;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Xunit;

public class HttpRemoteStoreShould : IMultiTenantStoreTestBase<HttpRemoteStore>
{
    public class TestHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var result = new HttpResponseMessage();

            var numSegments = request.RequestUri.Segments.Length;
            if (string.Equals(request.RequestUri.Segments[numSegments - 1], "initech", StringComparison.OrdinalIgnoreCase))
            {

                var tenantInfo = new TenantInfo("initech-id", "initech", "Initech", "connstring", null);
                var json = JsonConvert.SerializeObject(tenantInfo);
                result.StatusCode = HttpStatusCode.OK;
                result.Content = new StringContent(json);
            }
            else
                result.StatusCode = HttpStatusCode.NotFound;

            return Task.FromResult(result);
        }
    }

    // Basic store functionality tested in MultiTenantStoresShould.cs

    protected override IMultiTenantStore CreateTestStore()
    {
        var client = new HttpClient(new TestHandler());
        var typedClient = new HttpRemoteStoreClient(client, "http://example.com");
        return new HttpRemoteStore(typedClient);
    }

    protected override IMultiTenantStore PopulateTestStore(IMultiTenantStore store)
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