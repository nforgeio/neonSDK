// -----------------------------------------------------------------------------
// FILE:        Components/LayoutGroupItem.razor.cs
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
    /// An item inside a <see cref="LayoutGroup"/> that automatically FLIP-animates
    /// to its new position after a layout change.
    /// </summary>
    public partial class LayoutGroupItem : ComponentBase, IDisposable
    {
        private readonly string _id = "motion_lgi_" + Guid.NewGuid().ToString("N");
        private bool _hasRenderedOnce;

        [Inject]
        protected JsInterop JsInterop { get; set; }

        // ------------------------------------------------------------------ //
        // Parameters
        // ------------------------------------------------------------------ //

        /// <summary>
        /// A stable identifier for this item within the <see cref="LayoutGroup"/>.
        /// Must be unique among all items in the same group.
        /// </summary>
        [Parameter, EditorRequired]
        public string ItemId { get; set; }

        /// <summary>Child content rendered inside the wrapper element.</summary>
        [Parameter]
        public RenderFragment ChildContent { get; set; }

        /// <summary>Any additional HTML attributes forwarded to the wrapper element.</summary>
        [Parameter(CaptureUnmatchedValues = true)]
        public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }

        /// <summary>
        /// The cascading <see cref="LayoutGroupContext"/> provided by the parent
        /// <see cref="LayoutGroup"/>.
        /// </summary>
        [CascadingParameter]
        public LayoutGroupContext Context { get; set; }

        // ------------------------------------------------------------------ //
        // Lifecycle
        // ------------------------------------------------------------------ //

        protected override void OnInitialized()
        {
            Context?.Register(
                ItemId,
                "#" + _id,
                () => JsInterop.GetRectAsync("#" + _id).AsTask());
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                _hasRenderedOnce = true;
                return;
            }

            if (!_hasRenderedOnce)
                return;

            // After every subsequent render, check whether this item has a captured rect
            // and FLIP-animate from it to the new position.
            if (Context == null)
                return;

            var captured = Context.GetCaptured(ItemId);
            if (captured == null)
                return;

            var animId = "motion_flip_" + Guid.NewGuid().ToString("N");
            var options = Context.Transition;

            await JsInterop.FlipFromRectAsync(
                animId,
                "#" + _id,
                captured.X,
                captured.Y,
                options);
        }

        // ------------------------------------------------------------------ //
        // Disposal
        // ------------------------------------------------------------------ //

        public void Dispose()
        {
            Context?.Unregister(ItemId);
        }
    }
}
