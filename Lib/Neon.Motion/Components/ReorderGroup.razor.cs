// -----------------------------------------------------------------------------
// FILE:        Components/ReorderGroup.razor.cs
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
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;

using Neon.Motion.Models;

namespace Neon.Motion.Components
{
    /// <summary>
    /// A drag-to-reorder list with FLIP animations — analogous to
    /// <c>Reorder.Group</c> in motion-react.
    /// </summary>
    /// <typeparam name="TItem">The type of item in the list.</typeparam>
    /// <remarks>
    /// <para>
    /// Provide <see cref="Items"/> and a <see cref="RenderItem"/> render fragment.
    /// Bind <see cref="OnReorder"/> to receive the updated list after a drag-and-drop.
    /// Each item gets a stable element ID so Blazor can reuse the same DOM node
    /// when the list reorders, enabling correct FLIP position animation.
    /// </para>
    /// <example>
    /// <code>
    /// &lt;ReorderGroup TItem="string"
    ///               Items="_items"
    ///               RenderItem="@(item =&gt; @&lt;span&gt;@item&lt;/span&gt;)"
    ///               OnReorder="@(updated =&gt; _items = updated)" /&gt;
    /// </code>
    /// </example>
    /// </remarks>
    public partial class ReorderGroup<TItem> : ComponentBase
    {
        private readonly string _idPrefix = "ro_" + Guid.NewGuid().ToString("N")[..8] + "_";

        // Internal list kept in sync with Items and reordered during drag.
        private List<TItem>  _orderedItems = new();

        // Stable element IDs that travel with their item through every reorder.
        // _itemElemIds[i] is always the DOM element ID for _orderedItems[i].
        private List<string> _itemElemIds  = new();

        // Element ID of the item currently being dragged.
        private string _draggingElemId;

        // The last target element id processed by OnDragOverAsync — used to
        // debounce the continuous stream of dragover events so we only reorder
        // when the hovered element actually changes.
        private string _lastTargetElemId;

        // Set to true when _orderedItems changed so OnAfterRenderAsync can FLIP.
        private bool _pendingFlip;

        // Prevents overlapping async drag-over processing.
        private bool _processingDragOver;

        // Rects captured just before the last reorder step, keyed by element ID.
        private readonly Dictionary<string, ElementRect> _prevRects = new();

        [Inject]
        protected JsInterop JsInterop { get; set; }

        // ------------------------------------------------------------------ //
        // Parameters
        // ------------------------------------------------------------------ //

        /// <summary>The current ordered list of items to display.</summary>
        [Parameter, EditorRequired]
        public IList<TItem> Items { get; set; }

        /// <summary>
        /// A render fragment invoked once per item to produce its content.
        /// </summary>
        [Parameter, EditorRequired]
        public RenderFragment<TItem> RenderItem { get; set; }

        /// <summary>
        /// Invoked with the reordered list after a successful drag-and-drop operation.
        /// </summary>
        [Parameter]
        public EventCallback<List<TItem>> OnReorder { get; set; }

        /// <summary>
        /// Timing options for the FLIP position animations.
        /// Defaults to a 0.25 s ease-out tween when <c>null</c>.
        /// </summary>
        [Parameter]
        public AnimateOptions Transition { get; set; }

        /// <summary>Optional CSS class for the outer wrapper element.</summary>
        [Parameter]
        public string CssClass { get; set; } = "motion-reorder-group";

        /// <summary>Any additional HTML attributes forwarded to the wrapper element.</summary>
        [Parameter(CaptureUnmatchedValues = true)]
        public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }

        // ------------------------------------------------------------------ //
        // Lifecycle
        // ------------------------------------------------------------------ //

        protected override void OnParametersSet()
        {
            if (Items == null)
                return;

            var incoming = Items.ToList();

            // Rebuild internal state only when Items changes from outside
            // (ignore updates driven by our own OnReorder callback).
            if (!incoming.SequenceEqual(_orderedItems))
            {
                // Preserve IDs for items that were already tracked;
                // assign new IDs only to items we haven't seen before.
                var oldMap = _orderedItems
                    .Zip(_itemElemIds, (item, id) => (item, id))
                    .GroupBy(p => p.item, EqualityComparer<TItem>.Default)
                    .ToDictionary(g => g.Key, g => g.First().id, EqualityComparer<TItem>.Default);

                _orderedItems = incoming;
                _itemElemIds  = incoming
                    .Select(item => oldMap.TryGetValue(item, out var id)
                        ? id
                        : _idPrefix + Guid.NewGuid().ToString("N")[..8])
                    .ToList();
            }
        }

        // ------------------------------------------------------------------ //
        // Drag events
        // ------------------------------------------------------------------ //

        private async Task OnDragStartAsync(string elemId)
        {
            _draggingElemId   = elemId;
            _lastTargetElemId = elemId;

            // Pre-capture all rects so we have a baseline for the first OnDragOver.
            await CaptureRectsAsync();
        }

        private async Task OnDragOverAsync(string targetElemId)
        {
            if (_draggingElemId == null
                || targetElemId == _draggingElemId
                || targetElemId == _lastTargetElemId
                || _processingDragOver)
            {
                return;
            }

            var dragIdx   = _itemElemIds.IndexOf(_draggingElemId);
            var targetIdx = _itemElemIds.IndexOf(targetElemId);

            if (dragIdx < 0 || targetIdx < 0 || dragIdx == targetIdx)
                return;

            _processingDragOver = true;
            try
            {
                _lastTargetElemId = targetElemId;

                // Capture current positions BEFORE the reorder so we have
                // "from" rects for the FLIP that runs after re-render.
                await CaptureRectsAsync();

                // Move the dragged item and its ID to the target slot.
                var item = _orderedItems[dragIdx];
                _orderedItems.RemoveAt(dragIdx);
                _orderedItems.Insert(targetIdx, item);

                var id = _itemElemIds[dragIdx];
                _itemElemIds.RemoveAt(dragIdx);
                _itemElemIds.Insert(targetIdx, id);

                _pendingFlip = true;
                StateHasChanged();
            }
            finally
            {
                _processingDragOver = false;
            }
        }

        private async Task OnDropAsync()
        {
            _draggingElemId   = null;
            _lastTargetElemId = null;

            if (OnReorder.HasDelegate)
                await OnReorder.InvokeAsync(new List<TItem>(_orderedItems));
        }

        private void OnDragEnd()
        {
            _draggingElemId   = null;
            _lastTargetElemId = null;
            StateHasChanged();
        }

        // ------------------------------------------------------------------ //
        // FLIP
        // ------------------------------------------------------------------ //

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender || !_pendingFlip)
                return;

            _pendingFlip = false;

            var options = Transition ?? new AnimateOptions { Duration = 0.25, Ease = "easeOut" };

            // Build FLIP targets — skip the dragged element (the browser's drag
            // ghost is already showing it at the cursor; animating its real
            // element too creates fighting visual feedback). Use stable
            // elemId-based animIds so a follow-up FLIP on the same element
            // cleanly stops the previous one instead of layering on top.
            var targets = new List<FlipTarget>(_itemElemIds.Count);
            foreach (var elemId in _itemElemIds)
            {
                if (elemId == _draggingElemId)
                    continue;

                if (!_prevRects.TryGetValue(elemId, out var prev))
                    continue;

                targets.Add(new FlipTarget(
                    AnimId:   "ro_flip_" + elemId,
                    Selector: "#" + elemId,
                    FromX:    prev.X,
                    FromY:    prev.Y));
            }

            if (targets.Count > 0)
                await JsInterop.FlipManyAsync(targets, options);

            _prevRects.Clear();
        }

        private async Task CaptureRectsAsync()
        {
            _prevRects.Clear();
            foreach (var elemId in _itemElemIds)
            {
                var rect = await JsInterop.GetRectAsync("#" + elemId);
                if (rect != null)
                    _prevRects[elemId] = rect;
            }
        }
    }
}
