// -----------------------------------------------------------------------------
// FILE:	    JSHelper.cs
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Microsoft.JSInterop;

using Neon.Cryptography;

namespace Neon.Blazor
{
    public static class JsExtensions
    {
        private static IJSObjectReference jsModule;


        public static async Task ImportModuleAsync(
            this IJSRuntime JS,
            string src,
            HashMethod hashMethod,
            string hash,
            string fallback = null)
        {
            if (jsModule == null)
            {
                jsModule = await JS.InvokeAsync<IJSObjectReference>("import", "./_content/Neon.Blazor/interop.js");
            }

           var checkedIntegrity = await jsModule.InvokeAsync<bool>("checkIntegrityAsync",src,$"{hashMethod}-{hash}");

            if (checkedIntegrity)
            {
                await JS.InvokeAsync<IJSObjectReference>("import", src);
            }
            else if (fallback != null)
            {
                await JS.InvokeAsync<IJSObjectReference>("import", fallback);
            }
            else
            {
                throw new Exception("Failed to load module");
            }
        }
    }
}
