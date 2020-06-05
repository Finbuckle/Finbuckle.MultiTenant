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
using Microsoft.Extensions.Logging;

namespace Finbuckle.MultiTenant
{
    internal class Utilities
    {
        internal static void TryLoginfo(ILogger logger, string message)
        {
            if (logger != null)
            {
                logger.LogInformation(message);
            }
        }

        internal static void TryLogDebug(ILogger logger, string message)
        {
            if (logger != null)
            {
                logger.LogDebug(message);
            }
        }

        internal static void TryLogError(ILogger logger, string message, Exception e)
        {
            if (logger != null)
            {
                logger.LogError(e, message);
            }
        }
    }
}