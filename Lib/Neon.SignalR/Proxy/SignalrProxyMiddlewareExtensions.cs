// -----------------------------------------------------------------------------
// FILE:	    SignalrProxyMiddlewareExtensions.cs
// CONTRIBUTOR: NEONFORGE Team
// COPYRIGHT:   Copyright © 2005-2024 by NEONFORGE LLC.  All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License").
// You may not use this file except in compliance with the License.
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;

namespace Neon.SignalR
{
    /// <summary>
    /// Extension methods for the <see cref="SignalrProxyMiddleware"/>.
    /// </summary>
    public static class SignalrProxyMiddlewareExtensions
    {
        /// <summary>
        /// Extension method to add the <see cref="SignalrProxyMiddleware"/> to the pipeline.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseSignalrProxy(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SignalrProxyMiddleware>();
        }
    }
}
