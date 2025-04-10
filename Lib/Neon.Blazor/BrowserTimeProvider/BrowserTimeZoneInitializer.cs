// -----------------------------------------------------------------------------
// FILE:	    BrowserTimeZoneInitializer.cs
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
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Neon.Blazor
{
    internal class BrowserTimeZoneInitializer : ComponentBase
    {
        [Inject]
        public TimeProvider TimeProvider { get; set; } = default!;

        [Inject]
        public IJSRuntime JS { get; set; } = default!;

        private IJSObjectReference jsModule;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
            {
                return;
            }

            jsModule ??= await JS.InvokeAsync<IJSObjectReference>("import", "./_content/Neon.Blazor/interop.js");

            if (TimeProvider is BrowserTimeProvider btp && !btp.IsLocalTimeZoneSet)
            {
                try
                {
                    var timeZone = await jsModule.InvokeAsync<string>("getBrowserTimeZone");
                    btp.SetBrowserTimeZone(timeZone);
                }
                catch (JSDisconnectedException)
                {
                }
            }
        }
    }
}
