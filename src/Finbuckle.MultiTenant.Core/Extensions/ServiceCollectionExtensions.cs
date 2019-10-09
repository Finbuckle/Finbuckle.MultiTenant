// Copyright 2019 Andrew White
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

using System;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class FinbuckleServiceCollectionExtensions
    {
        public static bool DecorateService<TService, TImpl>(this IServiceCollection services, params object[] parameters)
        {
            var existingService = services.SingleOrDefault(s => s.ServiceType == typeof(TService));
            if (existingService == null)
                return false;

            var newService = new ServiceDescriptor(existingService.ServiceType,
                                           sp =>
                                           {
                                               TService inner = (TService)ActivatorUtilities.CreateInstance(sp, existingService.ImplementationType);

                                               var parameters2 = new object[parameters.Length + 1];
                                               Array.Copy(parameters, 0, parameters2, 1, parameters.Length);
                                               parameters2[0] = inner;

                                               return ActivatorUtilities.CreateInstance<TImpl>(sp, parameters2);
                                           },
                                           existingService.Lifetime);

            if (existingService.ImplementationInstance != null)
            {
                newService = new ServiceDescriptor(existingService.ServiceType,
                                           sp =>
                                           {
                                               TService inner = (TService)existingService.ImplementationInstance;
                                               return ActivatorUtilities.CreateInstance<TImpl>(sp, inner, parameters);
                                           },
                                           existingService.Lifetime);
            }
            else if (existingService.ImplementationFactory != null)
            {
                newService = new ServiceDescriptor(existingService.ServiceType,
                                           sp =>
                                           {
                                               TService inner = (TService)existingService.ImplementationFactory(sp);
                                               return ActivatorUtilities.CreateInstance<TImpl>(sp, inner, parameters);
                                           },
                                           existingService.Lifetime);
            }

            services.Remove(existingService);
            services.Add(newService);

            return true;
        }
    }
}