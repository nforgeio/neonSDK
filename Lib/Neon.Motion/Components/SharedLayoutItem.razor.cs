// -----------------------------------------------------------------------------
// FILE:        Components/SharedLayoutItem.razor.cs
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
using System.Linq;
using System.Text;
using System.Text.Json;

using Microsoft.AspNetCore.Components;

using Neon.Motion.Models;

namespace Neon.Motion.Components
{
    /// <summary>
    /// An element participating in a <see cref="SharedLayout"/> group. Any
    /// <see cref="SharedLayoutItem"/> that shares a <c>LayoutId</c> with a
    /// previously-mounted (or previously-sized) item animates continuously from
    /// the old rect to its current rect — position and, optionally, size.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The wrapper <c>&lt;div&gt;</c> renders with the <c>sli-hidden</c> CSS class
    /// (opacity 0) on every meaningful re-render so there is no visible flash of
    /// the element at its natural position before the JS animation starts. A
    /// client-side <c>MutationObserver</c> (registered once by the enclosing
    /// <see cref="SharedLayout"/>) sees the element appear, consults its rect
    /// cache for a previous rect matching <see cref="LayoutId"/>, and either
    /// animates from the cached rect or reveals the element immediately.
    /// </para>
    /// <para>
    /// Unlike the previous implementation, this component does NOT make any
    /// JS interop calls from <c>OnAfterRenderAsync</c> — all measurement and
    /// animation happens synchronously in the observer callback, before the
    /// browser paints. This eliminates the Blazor Server round-trip gap that
    /// previously produced a visible blank frame between the old element
    /// disappearing and the animation starting.
    /// </para>
    /// </remarks>
    public partial class SharedLayoutItem : ComponentBase
    {
        private readonly string _id = "motion_sli_" + Guid.NewGuid().ToString("N");

        // Signature of the parameters at the last render — used by ShouldRender
        // to suppress re-renders triggered by ambient parent re-renders (e.g.
        // an unrelated click on a sibling demo). Without this, every page
        // re-render would re-apply .sli-hidden and trigger a pointless
        // reveal flash.
        private string _lastSignature;
        private bool   _hasRenderedOnce;

        // ------------------------------------------------------------------ //
        // Parameters
        // ------------------------------------------------------------------ //

        /// <summary>
        /// The shared-layout identifier. All <see cref="SharedLayoutItem"/>
        /// instances with the same <c>LayoutId</c> animate as a single logical
        /// element when they mount, unmount, or change rect.
        /// </summary>
        [Parameter, EditorRequired]
        public string LayoutId { get; set; }

        /// <summary>
        /// When <c>true</c> (default) the transition also morphs width and height
        /// via <c>scaleX</c> / <c>scaleY</c>. Set to <c>false</c> for
        /// position-only FLIP — useful when the from/to sizes differ greatly and
        /// content distortion during scale would be distracting.
        /// </summary>
        [Parameter]
        public bool AnimateScale { get; set; } = true;

        /// <summary>
        /// Optional per-item timing options. When <c>null</c> the transition from
        /// the enclosing <see cref="SharedLayout"/> is used; if that is also
        /// <c>null</c>, motion.dev's default animation is applied.
        /// </summary>
        [Parameter]
        public AnimateOptions Transition { get; set; }

        /// <summary>Child content rendered inside the wrapper element.</summary>
        [Parameter]
        public RenderFragment ChildContent { get; set; }

        /// <summary>Any additional HTML attributes forwarded to the wrapper element.</summary>
        [Parameter(CaptureUnmatchedValues = true)]
        public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }

        /// <summary>
        /// The cascading <see cref="SharedLayoutContext"/> provided by the
        /// enclosing <see cref="SharedLayout"/>. Supplies the default transition.
        /// </summary>
        [CascadingParameter]
        public SharedLayoutContext Context { get; set; }

        // ------------------------------------------------------------------ //
        // Lifecycle
        // ------------------------------------------------------------------ //

        protected override bool ShouldRender()
        {
            if (!_hasRenderedOnce)
                return true;

            var signature = BuildSignature();
            if (signature == _lastSignature)
                return false;

            _lastSignature = signature;
            return true;
        }

        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                _hasRenderedOnce = true;
                _lastSignature   = BuildSignature();
            }
        }

        // ------------------------------------------------------------------ //
        // Helpers used by the .razor template
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Returns the CSS class string for the wrapper div, prepending
        /// <c>sli-hidden</c> (opacity 0) so the element is invisible on first
        /// paint. The JS-side observer removes this class after the animation
        /// (or immediately when no animation is needed).
        /// </summary>
        internal string GetClass()
        {
            var userClass = AdditionalAttributes != null &&
                            AdditionalAttributes.TryGetValue("class", out var c)
                            ? c?.ToString()
                            : null;

            return string.IsNullOrWhiteSpace(userClass)
                ? "sli-hidden"
                : $"sli-hidden {userClass}";
        }

        /// <summary>
        /// Returns <see cref="AdditionalAttributes"/> with the <c>class</c> key
        /// stripped, because <c>class</c> is rendered explicitly via
        /// <see cref="GetClass"/> to allow merging with <c>sli-hidden</c>.
        /// </summary>
        internal IReadOnlyDictionary<string, object> GetAttrsWithoutClass()
        {
            if (AdditionalAttributes == null || !AdditionalAttributes.ContainsKey("class"))
                return AdditionalAttributes;

            var d = AdditionalAttributes
                .Where(kv => kv.Key != "class")
                .ToDictionary(kv => kv.Key, kv => kv.Value);

            return d.Count > 0 ? d : null;
        }

        /// <summary>
        /// Serializes the effective options (scale flag + resolved transition)
        /// as a compact JSON string for the <c>data-sli-opts</c> attribute so
        /// the JS observer can read them without a Blazor round-trip.
        /// </summary>
        internal string GetOptsJson()
        {
            var transition = Transition ?? Context?.Transition;

            var payload = new Dictionary<string, object>
            {
                ["scale"] = AnimateScale,
            };

            if (transition != null)
            {
                // ToJson() emits a JSON object; re-parse so it nests as a real
                // object in the outer payload instead of being string-encoded.
                using var doc = JsonDocument.Parse(transition.ToJson());
                payload["transition"] = doc.RootElement.Clone();
            }

            return JsonSerializer.Serialize(payload);
        }

        // ------------------------------------------------------------------ //
        // Private helpers
        // ------------------------------------------------------------------ //

        // Combines the parameters that can change this item's layout into a
        // single string for cheap equality comparison. Catches LayoutId
        // changes and attribute changes (class, style) which are what actually
        // affect rect; ignores ChildContent identity since its reference changes
        // every render even when the output is identical.
        private string BuildSignature()
        {
            var sb = new StringBuilder();
            sb.Append(LayoutId).Append('|');

            if (AdditionalAttributes != null)
            {
                foreach (var kv in AdditionalAttributes.OrderBy(k => k.Key, StringComparer.Ordinal))
                {
                    if (kv.Key == "class" || kv.Key == "style" || kv.Key == "id")
                        sb.Append(kv.Key).Append('=').Append(kv.Value?.ToString()).Append(';');
                }
            }

            return sb.ToString();
        }
    }
}
