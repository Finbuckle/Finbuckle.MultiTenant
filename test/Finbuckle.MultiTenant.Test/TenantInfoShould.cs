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

using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Internal;
using Xunit;

public class TenantInfoShould
{
    [Fact]
    public void ThrowIfIdSetWithLengthAboveTenantIdMaxLength()
    {
        // OK
        new TenantInfo { Id = "".PadRight(1, 'a') };

        // OK
        new TenantInfo { Id = "".PadRight(Constants.TenantIdMaxLength, 'a') };
        
        Assert.Throws<MultiTenantException>(() => new TenantInfo{ Id = "".PadRight(Constants.TenantIdMaxLength + 1, 'a') });
        Assert.Throws<MultiTenantException>(() => new TenantInfo{ Id = "".PadRight(Constants.TenantIdMaxLength
            + 999, 'a') });
    }
}