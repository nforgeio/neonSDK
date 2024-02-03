// -----------------------------------------------------------------------------
// FILE:	    ServerRenderContext.cs
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

using System.Runtime.Versioning;

namespace Neon.Blazor
{
    [UnsupportedOSPlatform("browser")]
    public class ServerRenderContext : IRenderContext
    {
        private Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        public ServerRenderContext(Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor = null)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        /// <inheritdoc/>
        public bool IsServer => true;

        /// <inheritdoc/>
        public bool IsClient => false;

        /// <inheritdoc/>
        public bool IsPrerendering => !httpContextAccessor?.HttpContext?.Response.HasStarted ?? false;
    }
}
