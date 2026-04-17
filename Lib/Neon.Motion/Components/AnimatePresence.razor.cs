// -----------------------------------------------------------------------------
// FILE:        Components/AnimatePresence.razor.cs
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
    /// Animates child content in and out of the DOM — analogous to
    /// <c>AnimatePresence</c> in the React motion package.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When <see cref="IsPresent"/> flips to <c>false</c> the component keeps its
    /// children mounted, runs the <see cref="Exit"/> animation on its wrapper element,
    /// and only removes the children from the DOM once that animation finishes.
    /// </para>
    /// <para>
    /// When <see cref="IsPresent"/> flips back to <c>true</c> the children are
    /// remounted and the entry animation (<see cref="Initial"/> → <see cref="Animate"/>)
    /// is replayed.
    /// </para>
    /// <example>
    /// <code>
    /// &lt;AnimatePresence IsPresent="@_showModal"
    ///                  Initial="@(new AnimationValues().Opacity(0).Y(20))"
    ///                  Animate="@(new AnimationValues().Opacity(1).Y(0))"
    ///                  Exit="@(new AnimationValues().Opacity(0).Y(20))"
    ///                  Transition="@(new AnimateOptions { Duration = 0.3, Ease = "easeOut" })"
    ///                  OnExitComplete="@HandleExitComplete"
    ///                  class="modal-backdrop"&gt;
    ///     &lt;div class="modal"&gt;…&lt;/div&gt;
    /// &lt;/AnimatePresence&gt;
    /// </code>
    /// </example>
    /// </remarks>
    public partial class AnimatePresence : ComponentBase, IAsyncDisposable
    {
        private readonly string _id = "motion_presence_" + Guid.NewGuid().ToString("N");

        // Tracks whether children are actually in the DOM.
        private bool _isRendering;

        // Previous value of IsPresent — lets us detect direction changes.
        private bool _wasPresent;

        // Signals OnAfterRenderAsync that an entry animation should fire.
        private bool _needsEntryAnim;

        // ID of whichever animation is currently active (entry or exit).
        private string _currentAnimId;

        // ------------------------------------------------------------------ //
        // Parameters
        // ------------------------------------------------------------------ //

        /// <summary>Controls whether children are present.</summary>
        [Parameter]
        public bool IsPresent { get; set; } = true;

        /// <summary>
        /// Values applied <em>instantly</em> (duration 0) at the start of the entry
        /// animation — i.e. the "from" state. Mirrors <c>initial</c> in motion-react.
        /// Ignored when <c>null</c>.
        /// </summary>
        [Parameter]
        public AnimationValues Initial { get; set; }

        /// <summary>
        /// Target values for the entry animation — the "to" state.
        /// Mirrors <c>animate</c> in motion-react. No entry animation when <c>null</c>.
        /// </summary>
        [Parameter]
        public AnimationValues Animate { get; set; }

        /// <summary>
        /// Target values for the exit animation.
        /// The wrapper element animates to these values before being removed.
        /// Mirrors <c>exit</c> in motion-react. When <c>null</c> children are removed
        /// from the DOM immediately without animation.
        /// </summary>
        [Parameter]
        public AnimationValues Exit { get; set; }

        /// <summary>
        /// Timing options shared by both the entry and exit animations.
        /// Override per-direction using <see cref="ExitTransition"/>.
        /// </summary>
        [Parameter]
        public AnimateOptions Transition { get; set; }

        /// <summary>
        /// Timing options used exclusively for the exit animation.
        /// Falls back to <see cref="Transition"/> when <c>null</c>.
        /// </summary>
        [Parameter]
        public AnimateOptions ExitTransition { get; set; }

        /// <summary>Invoked once the exit animation has finished and the DOM is cleared.</summary>
        [Parameter]
        public EventCallback OnExitComplete { get; set; }

        /// <summary>Child content managed by this component.</summary>
        [Parameter]
        public RenderFragment ChildContent { get; set; }

        /// <summary>Any additional HTML attributes forwarded to the wrapper element.</summary>
        [Parameter(CaptureUnmatchedValues = true)]
        public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }

        // ------------------------------------------------------------------ //
        // Lifecycle
        // ------------------------------------------------------------------ //

        protected override void OnInitialized()
        {
            // Sync internal state with the initial parameter values so the
            // first OnParametersSetAsync call does not see a spurious transition.
            _isRendering = IsPresent;
            _wasPresent  = IsPresent;
        }

        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();

            var wasPresent = _wasPresent;
            _wasPresent    = IsPresent;

            if (wasPresent && !IsPresent)
            {
                // true → false: start exit animation, keep DOM alive until done.
                await StartExitAsync();
            }
            else if (!wasPresent && IsPresent)
            {
                // false → true: cancel any in-flight exit, remount, queue entry.
                await CancelCurrentAnimAsync();

                _isRendering    = true;
                _needsEntryAnim = Animate != null;
                // Blazor re-renders after OnParametersSetAsync; entry fires in
                // OnAfterRenderAsync once _needsEntryAnim is true.
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            // On first render, run the entry animation if configured and visible.
            bool shouldAnimate = firstRender
                ? IsPresent && Animate != null
                : _needsEntryAnim;

            _needsEntryAnim = false;

            if (!shouldAnimate || !_isRendering)
                return;

            await RunEntryAnimAsync();
        }

        // ------------------------------------------------------------------ //
        // Private helpers
        // ------------------------------------------------------------------ //

        private async Task StartExitAsync()
        {
            if (!_isRendering)
                return;

            if (Exit == null)
            {
                // No exit animation — remove immediately.
                _isRendering = false;
                return;
            }

            var animId     = "motion_exit_" + Guid.NewGuid().ToString("N");
            _currentAnimId = animId;

            var exitOptions = ExitTransition ?? Transition;

            await JsInterop.AnimateAsync(
                animId,
                "#" + _id,
                Exit,
                exitOptions,
                onComplete: async () =>
                {
                    // Guard against the animation being superseded by a re-entry.
                    if (_currentAnimId != animId)
                        return;

                    _currentAnimId = null;
                    _isRendering   = false;

                    await InvokeAsync(async () =>
                    {
                        StateHasChanged();

                        if (OnExitComplete.HasDelegate)
                            await OnExitComplete.InvokeAsync();
                    });
                });
        }

        private async Task RunEntryAnimAsync()
        {
            // Apply Initial values instantly to establish the "from" state.
            if (Initial != null)
            {
                var initId = "motion_init_" + Guid.NewGuid().ToString("N");

                await JsInterop.AnimateAsync(
                    initId,
                    "#" + _id,
                    Initial,
                    new AnimateOptions { Duration = 0 });
            }

            var entryId    = "motion_entry_" + Guid.NewGuid().ToString("N");
            _currentAnimId = entryId;

            await JsInterop.AnimateAsync(entryId, "#" + _id, Animate, Transition);
        }

        private async Task CancelCurrentAnimAsync()
        {
            if (string.IsNullOrEmpty(_currentAnimId))
                return;

            try
            {
                await JsInterop.CancelAsync(_currentAnimId);
            }
            catch
            {
                // JS runtime may already be tearing down.
            }

            _currentAnimId = null;
        }

        // ------------------------------------------------------------------ //
        // Disposal
        // ------------------------------------------------------------------ //

        public async ValueTask DisposeAsync()
        {
            await CancelCurrentAnimAsync();
        }
    }
}
