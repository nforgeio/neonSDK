// -----------------------------------------------------------------------------
// FILE:        JsInterop.cs
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.JSInterop;

using Neon.Motion.Models;

namespace Neon.Motion
{
    /// <summary>
    /// Scoped service providing access to the motion.dev JavaScript API from Blazor.
    /// Inject this service and call <c>AnimateAsync</c>, <c>ScrollAsync</c>,
    /// or <c>InViewAsync</c> to drive animations imperatively.
    /// </summary>
    public class JsInterop : IAsyncDisposable
    {
        private readonly Lazy<Task<IJSObjectReference>> _moduleTask;
        private IJSObjectReference _module;

        // Keep DotNetObjectReferences alive for the lifetime of this service
        private readonly ConcurrentDictionary<string, IDisposable> _callbacks = new();

        public JsInterop(IJSRuntime jsRuntime)
        {
            _moduleTask = new Lazy<Task<IJSObjectReference>>(() =>
                jsRuntime.InvokeAsync<IJSObjectReference>(
                    "import", "./_content/Neon.Motion/motion-interop.js").AsTask());
        }

        private async ValueTask<IJSObjectReference> GetModuleAsync()
        {
            if (_module == null)
                _module = await _moduleTask.Value.ConfigureAwait(false);
            return _module;
        }

        // ------------------------------------------------------------------ //
        // animate()
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Animates one or more elements matching <paramref name="selector"/> to the
        /// target <paramref name="values"/>.
        /// </summary>
        /// <param name="animId">
        /// A unique identifier for this animation. Re-using an existing ID cancels
        /// the previous animation before starting the new one.
        /// </param>
        /// <param name="selector">CSS selector string targeting the element(s) to animate.</param>
        /// <param name="values">Target animation values.</param>
        /// <param name="options">Optional animation options.</param>
        public async Task AnimateAsync(
            string           animId,
            string           selector,
            AnimationValues  values,
            AnimateOptions   options = null)
        {
            var module     = await GetModuleAsync();
            var valuesJson = values?.ToJson() ?? "{}";
            var optionsJson = options?.ToJson();

            await module.InvokeVoidAsync(
                "motionAnimate", animId, selector, valuesJson, optionsJson);
        }

        /// <summary>
        /// Animates elements and invokes <paramref name="onComplete"/> when the animation finishes.
        /// </summary>
        public async Task AnimateAsync(
            string           animId,
            string           selector,
            AnimationValues  values,
            AnimateOptions   options,
            Func<Task>       onComplete)
        {
            var module = await GetModuleAsync();

            var helper = new AnimationCallbackHelper(_ => onComplete?.Invoke() ?? Task.CompletedTask);
            var dotnetRef = DotNetObjectReference.Create(helper);

            // Replace any existing callback for this animId
            if (_callbacks.TryRemove(animId, out var old))
                old.Dispose();
            _callbacks[animId] = dotnetRef;

            await module.InvokeVoidAsync(
                "motionAnimateWithCallbacks",
                animId,
                selector,
                values?.ToJson() ?? "{}",
                options?.ToJson(),
                dotnetRef);
        }

        /// <summary>
        /// Animates multiple elements matching <paramref name="selector"/> with a staggered
        /// delay between each, equivalent to Motion's <c>stagger()</c> helper.
        /// </summary>
        /// <param name="animId">Unique animation identifier.</param>
        /// <param name="selector">CSS selector targeting the elements to animate.</param>
        /// <param name="values">Target animation values.</param>
        /// <param name="staggerSeconds">Delay in seconds between each successive element.</param>
        /// <param name="options">Optional animation options (delay/repeat/ease, etc.).</param>
        public async Task AnimateStaggerAsync(
            string          animId,
            string          selector,
            AnimationValues values,
            double          staggerSeconds,
            AnimateOptions  options = null)
        {
            var module = await GetModuleAsync();

            await module.InvokeVoidAsync(
                "motionAnimateStagger",
                animId,
                selector,
                values?.ToJson() ?? "{}",
                options?.ToJson(),
                staggerSeconds);
        }

        /// <summary>Pauses the animation identified by <paramref name="animId"/>.</summary>
        public async Task PauseAsync(string animId)
        {
            if (string.IsNullOrEmpty(animId)) return;
            var module = await GetModuleAsync();
            await module.InvokeVoidAsync("motionPause", animId);
        }

        /// <summary>Resumes or restarts the animation identified by <paramref name="animId"/>.</summary>
        public async Task PlayAsync(string animId)
        {
            if (string.IsNullOrEmpty(animId)) return;
            var module = await GetModuleAsync();
            await module.InvokeVoidAsync("motionPlay", animId);
        }

        /// <summary>Cancels the animation and reverts to the initial state.</summary>
        public async Task CancelAsync(string animId)
        {
            if (string.IsNullOrEmpty(animId)) return;
            var module = await GetModuleAsync();
            await module.InvokeVoidAsync("motionCancel", animId);

            if (_callbacks.TryRemove(animId, out var cb))
                cb.Dispose();
        }

        /// <summary>Immediately jumps the animation to its end state.</summary>
        public async Task CompleteAsync(string animId)
        {
            if (string.IsNullOrEmpty(animId)) return;
            var module = await GetModuleAsync();
            await module.InvokeVoidAsync("motionComplete", animId);
        }

        /// <summary>Commits the current state and prevents the animation from restarting.</summary>
        public async Task StopAsync(string animId)
        {
            if (string.IsNullOrEmpty(animId)) return;
            var module = await GetModuleAsync();
            await module.InvokeVoidAsync("motionStop", animId);

            if (_callbacks.TryRemove(animId, out var cb))
                cb.Dispose();
        }

        /// <summary>Gets the current playback time of an animation in seconds.</summary>
        public async ValueTask<double> GetTimeAsync(string animId)
        {
            if (string.IsNullOrEmpty(animId)) return 0;
            var module = await GetModuleAsync();
            return await module.InvokeAsync<double>("motionGetTime", animId);
        }

        /// <summary>Seeks the animation to <paramref name="time"/> seconds.</summary>
        public async Task SetTimeAsync(string animId, double time)
        {
            if (string.IsNullOrEmpty(animId)) return;
            var module = await GetModuleAsync();
            await module.InvokeVoidAsync("motionSetTime", animId, time);
        }

        /// <summary>Gets the current playback speed multiplier.</summary>
        public async ValueTask<double> GetSpeedAsync(string animId)
        {
            if (string.IsNullOrEmpty(animId)) return 1;
            var module = await GetModuleAsync();
            return await module.InvokeAsync<double>("motionGetSpeed", animId);
        }

        /// <summary>Sets the playback speed multiplier (e.g. 2 = double speed, -1 = reverse).</summary>
        public async Task SetSpeedAsync(string animId, double speed)
        {
            if (string.IsNullOrEmpty(animId)) return;
            var module = await GetModuleAsync();
            await module.InvokeVoidAsync("motionSetSpeed", animId, speed);
        }

        // ------------------------------------------------------------------ //
        // scroll()
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Tracks scroll progress and invokes <paramref name="onScroll"/> on each frame.
        /// </summary>
        /// <param name="scrollId">
        /// Unique identifier. Re-using an existing ID cancels the previous subscription.
        /// </param>
        /// <param name="onScroll">
        /// Callback invoked with the scroll ID, progress (0–1), and optional detailed info.
        /// </param>
        /// <param name="options">Optional scroll options (axis, offset, target selector, container selector).</param>
        public async Task ScrollAsync(
            string                                  scrollId,
            Func<string, double, ScrollInfo, Task>  onScroll,
            ScrollOptions                           options = null)
        {
            var module = await GetModuleAsync();

            var helper    = new ScrollCallbackHelper(onScroll);
            var dotnetRef = DotNetObjectReference.Create(helper);

            if (_callbacks.TryRemove(scrollId, out var old))
                old.Dispose();
            _callbacks[scrollId] = dotnetRef;

            await module.InvokeVoidAsync(
                "motionScroll",
                scrollId,
                options?.Target,
                options?.Container,
                options?.ToJson(),
                dotnetRef);
        }

        /// <summary>Cancels the scroll subscription identified by <paramref name="scrollId"/>.</summary>
        public async Task CancelScrollAsync(string scrollId)
        {
            if (string.IsNullOrEmpty(scrollId)) return;
            var module = await GetModuleAsync();
            await module.InvokeVoidAsync("motionCancelScroll", scrollId);

            if (_callbacks.TryRemove(scrollId, out var cb))
                cb.Dispose();
        }

        // ------------------------------------------------------------------ //
        // inView()
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Watches elements matching <paramref name="selector"/> and invokes callbacks
        /// when they enter or leave the viewport.
        /// </summary>
        /// <param name="inViewId">
        /// Unique identifier. Re-using an existing ID stops the previous observer.
        /// </param>
        /// <param name="selector">CSS selector for the element(s) to observe.</param>
        /// <param name="onEnter">Invoked with the inViewId when the element enters the viewport.</param>
        /// <param name="onLeave">Optional callback invoked when the element leaves the viewport.</param>
        /// <param name="options">Optional inView options.</param>
        public async Task InViewAsync(
            string          inViewId,
            string          selector,
            Func<string, Task> onEnter,
            Func<string, Task> onLeave  = null,
            InViewOptions   options     = null)
        {
            var module = await GetModuleAsync();

            var helper    = new InViewCallbackHelper(onEnter, onLeave);
            var dotnetRef = DotNetObjectReference.Create(helper);

            if (_callbacks.TryRemove(inViewId, out var old))
                old.Dispose();
            _callbacks[inViewId] = dotnetRef;

            await module.InvokeVoidAsync(
                "motionInView",
                inViewId,
                selector,
                options?.ToJson(),
                dotnetRef);
        }

        /// <summary>Stops the inView observer identified by <paramref name="inViewId"/>.</summary>
        public async Task StopInViewAsync(string inViewId)
        {
            if (string.IsNullOrEmpty(inViewId)) return;
            var module = await GetModuleAsync();
            await module.InvokeVoidAsync("motionStopInView", inViewId);

            if (_callbacks.TryRemove(inViewId, out var cb))
                cb.Dispose();
        }

        // ------------------------------------------------------------------ //
        // Layout / FLIP
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Returns the bounding rect of the first element matching <paramref name="selector"/>,
        /// or <c>null</c> when no element is found.
        /// </summary>
        /// <param name="selector">CSS selector targeting the element.</param>
        /// <param name="includeTransform">
        /// When <c>true</c> (default) returns the element's visual rect including any
        /// in-flight CSS transform. When <c>false</c> the transform is temporarily
        /// cleared so the returned rect reflects the element's <em>layout</em>
        /// position — useful for shared-layout animations where the canonical
        /// resting rect is needed, not the in-flight visual rect.
        /// </param>
        public async ValueTask<ElementRect> GetRectAsync(string selector, bool includeTransform = true)
        {
            var module = await GetModuleAsync();
            return await module.InvokeAsync<ElementRect>("motionGetRect", selector, includeTransform);
        }

        /// <summary>
        /// FLIP-animates an element from a previously captured position
        /// (<paramref name="fromX"/>, <paramref name="fromY"/>) to its current DOM position.
        /// No animation is started when the displacement is below half a pixel.
        /// </summary>
        /// <param name="animId">
        /// Animation identifier. Re-using the same ID for the same element ensures
        /// the previous in-flight FLIP is stopped before the new one begins.
        /// </param>
        /// <param name="selector">CSS selector targeting the element to animate.</param>
        /// <param name="fromX">The X coordinate captured <em>before</em> the layout change.</param>
        /// <param name="fromY">The Y coordinate captured <em>before</em> the layout change.</param>
        /// <param name="options">Optional animation options.</param>
        public async Task FlipFromRectAsync(
            string         animId,
            string         selector,
            double         fromX,
            double         fromY,
            AnimateOptions options = null)
        {
            var module = await GetModuleAsync();
            await module.InvokeVoidAsync(
                "motionFlipFromRect",
                animId,
                selector,
                fromX,
                fromY,
                options?.ToJson());
        }

        /// <summary>
        /// Batched FLIP: animates several elements at once with a single JS interop
        /// call. This avoids N round-trips from .NET to JS, keeps all layout
        /// measurements on the same animation frame, and prevents the layout
        /// thrashing caused by alternating per-element writes and reads.
        /// </summary>
        /// <param name="flips">The set of elements to FLIP.</param>
        /// <param name="options">Timing options applied to every animation.</param>
        public async Task FlipManyAsync(
            IEnumerable<FlipTarget> flips,
            AnimateOptions          options = null)
        {
            if (flips == null) return;

            var module = await GetModuleAsync();
            var json   = System.Text.Json.JsonSerializer.Serialize(flips);

            await module.InvokeVoidAsync(
                "motionFlipMany",
                json,
                options?.ToJson());
        }

        /// <summary>
        /// One-time initialization of the shared-layout MutationObserver on the
        /// client. Called by <see cref="Components.SharedLayout"/> on first
        /// render; idempotent (safe to call any number of times).
        /// </summary>
        /// <remarks>
        /// All per-item shared-layout work (measure + animate + reveal) is
        /// driven entirely client-side by the observer, so that the
        /// measurement and animation can start in the same microtask as the
        /// DOM mutation — before the browser paints — eliminating the Blazor
        /// Server round-trip gap.
        /// </remarks>
        public async ValueTask InitSharedLayoutAsync()
        {
            var module = await GetModuleAsync();
            await module.InvokeVoidAsync("motionSharedLayoutInit");
        }

        // ------------------------------------------------------------------ //
        // Disposal
        // ------------------------------------------------------------------ //

        public async ValueTask DisposeAsync()
        {
            foreach (var cb in _callbacks.Values)
                cb.Dispose();
            _callbacks.Clear();

            if (_moduleTask.IsValueCreated)
            {
                var module = await _moduleTask.Value.ConfigureAwait(false);
                await module.DisposeAsync();
            }
        }
    }
}
