//    Copyright 2018 Andrew White
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
using System.Threading.Tasks;
using Finbuckle.MultiTenant;

internal class TestStore : IMultiTenantStore
{
    public TestStore()
    {
    }

    public Task<TenantInfo> GetByIdentifierAsync(string identifier)
    {
        throw new NotImplementedException();
    }

    public Task<bool> TryAddAsync(TenantInfo context)
    {
        throw new NotImplementedException();
    }

    public Task<bool> TryRemoveAsync(string identifier)
    {
        throw new NotImplementedException();
    }

    public Task<bool> TryUpdateAsync(TenantInfo tenantInfo)
    {
        throw new NotImplementedException();
    }
}