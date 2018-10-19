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

namespace Finbuckle.MultiTenant.Core
{
    /// <summary>
    /// Contains constant values for Finbuckle.MultiTenant.Core.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// The maximum character length for Id property on a TenantContet.
        /// The property setter will throw a MultiTenantException if the assigned value exceeds this limit.
        /// </summary>
        public const int TenantIdMaxLength = 64;
    }
}