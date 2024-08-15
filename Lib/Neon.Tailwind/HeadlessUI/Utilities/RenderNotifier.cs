// -----------------------------------------------------------------------------
// FILE:	    RenderNotifier.cs
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

using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

using Neon.Tasks;

namespace Neon.Tailwind.HeadlessUI.Utilities
{
    public class RenderNotifier : ComponentBase
    {
        [Parameter]
        public Transition Transition { get; set; }

        [Parameter]
        public RenderFragment ChildContent { get; set; }

        private bool notify = false;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await SyncContext.Clear;

            await base.OnAfterRenderAsync(firstRender);

            if (notify)
            {
                await Transition?.NotifyRenderAsync();
                return;
            }

            await Task.Delay(10);

            notify = true;

            await InvokeAsync(StateHasChanged);
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.AddContent(0, ChildContent);
        }
    }
}
