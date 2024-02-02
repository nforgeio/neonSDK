// -----------------------------------------------------------------------------
// FILE:	    RenderingContext.cs
// CONTRIBUTOR: NEONFORGE Team
// COPYRIGHT:   Copyright © 2005-2023 by NEONFORGE LLC.  All rights reserved.
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

using Microsoft.AspNetCore.Http;

using System;
using System.Runtime.InteropServices;

namespace Neon.Blazor
{
    /// <summary>
    /// Provides information about the rendering context. This depends on an <see cref="IHttpContextAccessor"/>
    /// being registered in the service collection.
    /// </summary>
    public class RenderingContext
    {
        private static OSPlatform browserPlatform = OSPlatform.Create("Browser");

        private IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        public RenderingContext(IHttpContextAccessor httpContextAccessor = null)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Whether the code is running on the server.
        /// </summary>
        public bool IsServer => !RuntimeInformation.IsOSPlatform(browserPlatform);

        /// <summary>
        /// Whether the code is running on the client.
        /// </summary>
        public bool IsClient => RuntimeInformation.IsOSPlatform(browserPlatform);

        /// <summary>
        /// Whether the current request is prerendering.
        /// </summary>
        public bool IsPrerendering => !httpContextAccessor?.HttpContext?.Response.HasStarted ?? false;
    }
}
