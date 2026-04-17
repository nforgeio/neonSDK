// -----------------------------------------------------------------------------
// FILE:        Components/SharedLayout.razor.cs
// CONTRIBUTOR: NEONFORGE Team
// COPYRIGHT:   Copyright © 2005-2024 by NEONFORGE LLC.  All rights reserved.
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

using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;

using Neon.Motion.Models;

namespace Neon.Motion.Components
{
    /// <summary>
    /// Coordinates shared-layout animations across a group of
    /// <see cref="SharedLayoutItem"/> children. When an item with a given
    /// <c>LayoutId</c> unmounts and a new item with the same id mounts (or an
    /// existing item's rect changes), the new element animates position and
    /// size from the previously-recorded rect — analogous to motion-react's
    /// <c>layoutId</c> shared-layout transitions.
    /// </summary>
    /// <remarks>
    /// <example>
    /// <code>
    /// &lt;SharedLayout Transition="@(new AnimateOptions { Duration = 0.35, Ease = "easeInOut" })"&gt;
    ///     @foreach (var tab in _tabs)
    ///     {
    ///         &lt;button @onclick="@(() =&gt; _active = tab)"&gt;
    ///             @tab
    ///             @if (tab == _active)
    ///             {
    ///                 &lt;SharedLayoutItem LayoutId="underline" class="underline" /&gt;
    ///             }
    ///         &lt;/button&gt;
    ///     }
    /// &lt;/SharedLayout&gt;
    /// </code>
    /// </example>
    /// </remarks>
    public partial class SharedLayout : ComponentBase
    {
        private readonly SharedLayoutContext _context = new();

        [Inject]
        protected JsInterop JsInterop { get; set; }

        // ------------------------------------------------------------------ //
        // Parameters
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Timing options applied to every shared-layout transition inside this
        /// group. Defaults to motion.dev's built-in animation when <c>null</c>.
        /// </summary>
        [Parameter]
        public AnimateOptions Transition { get; set; }

        /// <summary>Child content — should contain <see cref="SharedLayoutItem"/> components.</summary>
        [Parameter]
        public RenderFragment ChildContent { get; set; }

        // ------------------------------------------------------------------ //
        // Lifecycle
        // ------------------------------------------------------------------ //

        protected override void OnParametersSet()
        {
            _context.Transition = Transition;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                // Ensure the motion-interop module is loaded and its
                // MutationObserver is running. The call is idempotent — if a
                // previous SharedLayout already initialized it on this page,
                // this just re-scans the DOM for any unhandled [data-sli]
                // items that were pre-rendered into the new subtree.
                await JsInterop.InitSharedLayoutAsync();
            }
        }
    }
}
