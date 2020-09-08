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

namespace Finbuckle.MultiTenant.Options
{
    public class TenantConfigureOptions<TOptions, TTenantInfo> : ITenantConfigureOptions<TOptions, TTenantInfo>
        where TOptions : class, new()
        where TTenantInfo : class, ITenantInfo, new()
    {
        private readonly Action<TOptions, TTenantInfo> configureOptions;

        public TenantConfigureOptions(Action<TOptions, TTenantInfo> configureOptions)
        {
            this.configureOptions = configureOptions;
        }

        public void Configure(TOptions options, TTenantInfo tenantInfo)
        {
            configureOptions(options, tenantInfo);
        }
    }
}