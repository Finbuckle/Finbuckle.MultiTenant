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
using System.Collections.Concurrent;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Core;
using Finbuckle.MultiTenant.Strategies;
using Xunit;

public class StaticTenantResolverShould
{
    [Theory]
    [InlineData("initech")]
    [InlineData("Initech")] // maintain case
    [InlineData("")] // empty string
    [InlineData("    ")] // whitespace
    [InlineData(null)] // null
    public async void ReturnExpectedIdentifier(string staticIdentifier)
    {
        var strategy = new StaticMultiTenantStrategy(staticIdentifier);

        var identifier = await strategy.GetIdentifierAsync(new Object());

        Assert.Equal(staticIdentifier, identifier);
    }
}