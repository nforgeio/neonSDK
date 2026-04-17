// -----------------------------------------------------------------------------
// FILE:        Components/MotionElementBase.cs
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
    /// Base class for Motion element components (<see cref="MotionDiv"/>, <see cref="MotionSpan"/>, etc.).
    /// Renders a single HTML element and exposes Motion animation methods.
    /// </summary>
    public abstract class MotionElementBase : ComponentBase, IAsyncDisposable
    {
        /// <summary>Unique element ID used as the CSS selector target.</summary>
        public readonly string Id = "motion_" + Guid.NewGuid().ToString("N");

        private string _currentAnimId;
        private bool   _firstRenderDone;

        [Inject]
        protected JsInterop JsInterop { get; set; }

        // ------------------------------------------------------------------ //
        // Parameters
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Values to animate to immediately after the component first renders.
        /// When <c>null</c>, no automatic animation is started.
        /// </summary>
        [Parameter]
        public AnimationValues Animate { get; set; }

        /// <summary>
        /// Options controlling how the <see cref="Animate"/> values are applied.
        /// </summary>
        [Parameter]
        public AnimateOptions Transition { get; set; }

        /// <summary>
        /// Callback invoked when the automatic entry animation completes.
        /// </summary>
        [Parameter]
        public EventCallback OnAnimationComplete { get; set; }

        /// <summary>Child content rendered inside the element.</summary>
        [Parameter]
        public RenderFragment ChildContent { get; set; }

        /// <summary>Allows any additional HTML attributes to be forwarded to the element.</summary>
        [Parameter(CaptureUnmatchedValues = true)]
        public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }

        // ------------------------------------------------------------------ //
        // Lifecycle
        // ------------------------------------------------------------------ //

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                _firstRenderDone = true;

                if (Animate != null)
                    await StartEntryAnimationAsync();
            }
        }

        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();

            // Re-animate when Animate parameter changes after first render
            if (_firstRenderDone && Animate != null)
                await StartEntryAnimationAsync();
        }

        private async Task StartEntryAnimationAsync()
        {
            _currentAnimId = "motion_anim_" + Guid.NewGuid().ToString("N");

            if (OnAnimationComplete.HasDelegate)
            {
                await JsInterop.AnimateAsync(
                    _currentAnimId,
                    "#" + Id,
                    Animate,
                    Transition,
                    () => OnAnimationComplete.InvokeAsync());
            }
            else
            {
                await JsInterop.AnimateAsync(
                    _currentAnimId,
                    "#" + Id,
                    Animate,
                    Transition);
            }
        }

        // ------------------------------------------------------------------ //
        // Public animation control API
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Starts a new animation on this element with the given values and options.
        /// Returns the animation ID that can be used with the other control methods.
        /// </summary>
        public async Task<string> AnimateAsync(AnimationValues values, AnimateOptions options = null)
        {
            var animId = "motion_anim_" + Guid.NewGuid().ToString("N");
            _currentAnimId = animId;
            await JsInterop.AnimateAsync(animId, "#" + Id, values, options);
            return animId;
        }

        /// <summary>
        /// Starts a new animation on this element and invokes <paramref name="onComplete"/>
        /// when it finishes.
        /// </summary>
        public async Task<string> AnimateAsync(AnimationValues values, AnimateOptions options, Func<Task> onComplete)
        {
            var animId = "motion_anim_" + Guid.NewGuid().ToString("N");
            _currentAnimId = animId;
            await JsInterop.AnimateAsync(animId, "#" + Id, values, options, onComplete);
            return animId;
        }

        /// <summary>Pauses the most recent animation started on this element.</summary>
        public Task PauseAsync() => JsInterop.PauseAsync(_currentAnimId);

        /// <summary>Resumes the most recent animation on this element.</summary>
        public Task PlayAsync() => JsInterop.PlayAsync(_currentAnimId);

        /// <summary>Cancels the most recent animation and reverts to its initial state.</summary>
        public Task CancelAsync() => JsInterop.CancelAsync(_currentAnimId);

        /// <summary>Immediately jumps the most recent animation to its end state.</summary>
        public Task CompleteAsync() => JsInterop.CompleteAsync(_currentAnimId);

        /// <summary>Commits the current state and prevents the animation from restarting.</summary>
        public Task StopAsync() => JsInterop.StopAsync(_currentAnimId);

        /// <summary>Gets the current playback time of the most recent animation.</summary>
        public ValueTask<double> GetTimeAsync() => JsInterop.GetTimeAsync(_currentAnimId);

        /// <summary>Seeks the most recent animation to <paramref name="time"/> seconds.</summary>
        public Task SetTimeAsync(double time) => JsInterop.SetTimeAsync(_currentAnimId, time);

        /// <summary>Gets the playback speed of the most recent animation.</summary>
        public ValueTask<double> GetSpeedAsync() => JsInterop.GetSpeedAsync(_currentAnimId);

        /// <summary>Sets the playback speed of the most recent animation.</summary>
        public Task SetSpeedAsync(double speed) => JsInterop.SetSpeedAsync(_currentAnimId, speed);

        // ------------------------------------------------------------------ //
        // Disposal
        // ------------------------------------------------------------------ //

        public virtual async ValueTask DisposeAsync()
        {
            if (!string.IsNullOrEmpty(_currentAnimId))
            {
                try { await JsInterop.CancelAsync(_currentAnimId); }
                catch { /* component may be tearing down after JS runtime */ }
            }
        }
    }
}
