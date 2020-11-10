//    Copyright 2018-2020 Finbuckle LLC, Andrew White, and Contributors
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

using System.Threading;

namespace Finbuckle.MultiTenant.Core
{
    public class MultiTenantContextAccessor<T> : IMultiTenantContextAccessor<T>, IMultiTenantContextAccessor
        where T : class, ITenantInfo, new()
    {
        internal static AsyncLocal<IMultiTenantContext<T>> _asyncLocalContext = new AsyncLocal<IMultiTenantContext<T>>();

        public IMultiTenantContext<T> MultiTenantContext
        {
            get
            {
                return _asyncLocalContext.Value;
            }

            set
            {
                _asyncLocalContext.Value = value;
            }
        }

        object IMultiTenantContextAccessor.MultiTenantContext
        {
            get => MultiTenantContext;
            set => MultiTenantContext = value as IMultiTenantContext<T> ?? MultiTenantContext;
        }
    }
}