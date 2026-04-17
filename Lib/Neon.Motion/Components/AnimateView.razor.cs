// -----------------------------------------------------------------------------
// FILE:        Components/AnimateView.razor.cs
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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;

using Neon.Motion.Models;

namespace Neon.Motion.Components
{
    /// <summary>
    /// Animates its child content when it enters the browser viewport — analogous to
    /// the <c>whileInView</c> prop in motion-react.
    /// </summary>
    /// <remarks>
    /// <para>
    /// On first render, <see cref="Initial"/> values are applied instantly to establish
    /// the "from" state. When the element scrolls into view, it animates to
    /// <see cref="Animate"/> values. If <see cref="Exit"/> is set the element animates
    /// back to those values when it scrolls out of view.
    /// </para>
    /// <example>
    /// <code>
    /// &lt;AnimateView
    ///     Initial="@(new AnimationValues().Opacity(0).Y(30))"
    ///     Animate="@(new AnimationValues().Opacity(1).Y(0))"
    ///     Transition="@(new AnimateOptions { Duration = 0.5, Ease = "easeOut" })"&gt;
    ///     &lt;div class="card"&gt;…&lt;/div&gt;
    /// &lt;/AnimateView&gt;
    /// </code>
    /// </example>
    /// </remarks>
    public partial class AnimateView : ComponentBase, IAsyncDisposable
    {
        private readonly string _id       = "motion_av_"   + Guid.NewGuid().ToString("N");
        private readonly string _inViewId = "motion_av_iv_" + Guid.NewGuid().ToString("N");

        [Inject]
        protected JsInterop JsInterop { get; set; }

        // ------------------------------------------------------------------ //
        // Parameters
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Values applied instantly before the element enters the viewport — the "from" state.
        /// </summary>
        [Parameter]
        public AnimationValues Initial { get; set; }

        /// <summary>Target values animated to when the element enters the viewport.</summary>
        [Parameter]
        public AnimationValues Animate { get; set; }

        /// <summary>
        /// Values animated to when the element <em>leaves</em> the viewport.
        /// When <c>null</c> no exit animation is triggered.
        /// </summary>
        [Parameter]
        public AnimationValues Exit { get; set; }

        /// <summary>Timing options for both the enter and exit animations.</summary>
        [Parameter]
        public AnimateOptions Transition { get; set; }

        /// <summary>
        /// Options forwarded to the underlying <c>inView()</c> call (root, margin, amount).
        /// </summary>
        [Parameter]
        public InViewOptions ViewOptions { get; set; }

        /// <summary>Invoked on the Blazor thread when the element enters the viewport.</summary>
        [Parameter]
        public EventCallback OnEnterView { get; set; }

        /// <summary>Invoked on the Blazor thread when the element leaves the viewport.</summary>
        [Parameter]
        public EventCallback OnLeaveView { get; set; }

        /// <summary>Child content rendered inside the wrapper element.</summary>
        [Parameter]
        public RenderFragment ChildContent { get; set; }

        /// <summary>Any additional HTML attributes forwarded to the wrapper element.</summary>
        [Parameter(CaptureUnmatchedValues = true)]
        public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }

        // ------------------------------------------------------------------ //
        // Lifecycle
        // ------------------------------------------------------------------ //

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
                return;

            // Apply Initial values instantly to establish the hidden "from" state.
            if (Initial != null)
            {
                var initId = "motion_av_init_" + Guid.NewGuid().ToString("N");
                await JsInterop.AnimateAsync(
                    initId,
                    "#" + _id,
                    Initial,
                    new AnimateOptions { Duration = 0 });
            }

            // Watch the element with inView().
            await JsInterop.InViewAsync(
                _inViewId,
                "#" + _id,
                onEnter: async _ =>
                {
                    if (Animate != null)
                    {
                        var enterAnim = "motion_av_enter_" + Guid.NewGuid().ToString("N");
                        await JsInterop.AnimateAsync(enterAnim, "#" + _id, Animate, Transition);
                    }
                    if (OnEnterView.HasDelegate)
                        await InvokeAsync(() => OnEnterView.InvokeAsync());
                },
                onLeave: Exit != null || OnLeaveView.HasDelegate
                    ? async _ =>
                      {
                          if (Exit != null)
                          {
                              var exitAnim = "motion_av_exit_" + Guid.NewGuid().ToString("N");
                              await JsInterop.AnimateAsync(exitAnim, "#" + _id, Exit, Transition);
                          }
                          if (OnLeaveView.HasDelegate)
                              await InvokeAsync(() => OnLeaveView.InvokeAsync());
                      }
                    : (Func<string, Task>)null,
                options: ViewOptions);
        }

        // ------------------------------------------------------------------ //
        // Disposal
        // ------------------------------------------------------------------ //

        public async ValueTask DisposeAsync()
        {
            try { await JsInterop.StopInViewAsync(_inViewId); }
            catch { /* JS runtime may be torn down */ }
        }
    }
}
