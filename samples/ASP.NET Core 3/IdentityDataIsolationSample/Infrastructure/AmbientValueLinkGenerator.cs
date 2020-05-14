// Copyright 2018-2020 Andrew White
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

using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Finbuckle.Utilities.AspNetCore
{
    public class AmbientValueLinkGenerator : LinkGenerator
    {
        private readonly LinkGenerator _inner;
        private readonly IList<string> _ambientValueKeys;

        public AmbientValueLinkGenerator(LinkGenerator inner, IList<string> ambientValueKeys)
        {
            _inner = inner;
            _ambientValueKeys = ambientValueKeys;
        }

        private (RouteValueDictionary newValues, RouteValueDictionary newAmbientValues) PromoteAmbientValues(RouteValueDictionary values, RouteValueDictionary ambientValues)
        {
            if (ambientValues == null)
                return (values, ambientValues);

            // Copy them so we don't affect anything outside our call chain.
            var newValues = new RouteValueDictionary(values);
            var newAmbientValues = new RouteValueDictionary(ambientValues);

            foreach (var key in _ambientValueKeys)
            {
                // Do we even have this ambient value?
                if (newAmbientValues.TryGetValue(key, out var value))
                {
                    // Try to add it to the regular values.
                    if (newValues.TryAdd(key, value))
                    {
                        // Remove from ambient value if successful.
                        newAmbientValues.Remove(key);
                    }
                }
            }

            return (newValues, newAmbientValues);
        }

        public override string GetPathByAddress<TAddress>(HttpContext httpContext, TAddress address, RouteValueDictionary values, RouteValueDictionary ambientValues = null, PathString? pathBase = null, FragmentString fragment = default, LinkOptions options = null)
        {
            var promotedValues = PromoteAmbientValues(values, ambientValues);
            return _inner.GetPathByAddress(httpContext,
                                           address,
                                           promotedValues.newValues,
                                           promotedValues.newAmbientValues,
                                           pathBase,
                                           fragment,
                                           options);
        }


        public override string GetPathByAddress<TAddress>(TAddress address, RouteValueDictionary values, PathString pathBase = default, FragmentString fragment = default, LinkOptions options = null)
        {
            return _inner.GetPathByAddress(address,
                                           values,
                                           pathBase,
                                           fragment,
                                           options);
        }

        public override string GetUriByAddress<TAddress>(HttpContext httpContext, TAddress address, RouteValueDictionary values, RouteValueDictionary ambientValues = null, string scheme = null, HostString? host = null, PathString? pathBase = null, FragmentString fragment = default, LinkOptions options = null)
        {
            var promotedValues = PromoteAmbientValues(values, ambientValues);
            return _inner.GetUriByAddress(httpContext,
                                          address,
                                          promotedValues.newValues,
                                          promotedValues.newAmbientValues,
                                          scheme,
                                          host,
                                          pathBase,
                                          fragment,
                                          options);
        }

        public override string GetUriByAddress<TAddress>(TAddress address, RouteValueDictionary values, string scheme, HostString host, PathString pathBase = default, FragmentString fragment = default, LinkOptions options = null)
        {
            return _inner.GetUriByAddress(address,
                                          values,
                                          scheme,
                                          host,
                                          pathBase,
                                          fragment,
                                          options);
        }
    }

}