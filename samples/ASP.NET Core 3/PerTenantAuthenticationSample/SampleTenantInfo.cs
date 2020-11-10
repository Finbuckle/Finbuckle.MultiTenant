// Copyright 2018-2020 Finbuckle LLC, Andrew White, and Contributors
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Finbuckle.MultiTenant;

namespace PerTenantAuthenticationSample
{
    public class SampleTenantInfo : ITenantInfo
    {
        public string Id { get; set; }
        public string Identifier { get; set; }
        public string Name { get; set; }
        public string ConnectionString { get; set; }
        
        public string ChallengeScheme { get; set; }

        public string CookiePath { get; set; }
        public string CookieLoginPath { get; set; }
        public string CookieLogoutPath { get; set; }

        public string OpenIdConnectAuthority { get; set; }
        public string OpenIdConnectClientId { get; set; }
        public string OpenIdConnectClientSecret { get; set; }
    }
}