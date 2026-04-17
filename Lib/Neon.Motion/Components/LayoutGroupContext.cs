// -----------------------------------------------------------------------------
// FILE:        Components/LayoutGroupContext.cs
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

using Neon.Motion.Models;

namespace Neon.Motion.Components
{
    /// <summary>
    /// Cascading context shared between a <see cref="LayoutGroup"/> and all of its
    /// <see cref="LayoutGroupItem"/> descendants. Coordinates rect capture and FLIP
    /// animation across items when the layout changes.
    /// </summary>
    public sealed class LayoutGroupContext
    {
        // item id → (selector, captureFunc)
        private readonly Dictionary<string, (string Selector, Func<Task<ElementRect>> Capture)> _items = new();

        // item id → rect captured before the last layout change
        private readonly Dictionary<string, ElementRect> _capturedRects = new();

        internal AnimateOptions Transition { get; set; }

        internal void Register(string itemId, string selector, Func<Task<ElementRect>> capture)
        {
            _items[itemId] = (selector, capture);
        }

        internal void Unregister(string itemId)
        {
            _items.Remove(itemId);
            _capturedRects.Remove(itemId);
        }

        /// <summary>
        /// Captures the current bounding rects of all registered items.
        /// Call this <em>before</em> making a layout change so the FLIP deltas
        /// can be computed afterward.
        /// </summary>
        public async Task CaptureAsync()
        {
            _capturedRects.Clear();
            foreach (var (id, (_, capture)) in _items)
            {
                var rect = await capture();
                if (rect != null)
                    _capturedRects[id] = rect;
            }
        }

        /// <summary>
        /// Returns the last captured rect for <paramref name="itemId"/>, or <c>null</c>
        /// when no rect was captured.
        /// </summary>
        internal ElementRect GetCaptured(string itemId)
            => _capturedRects.TryGetValue(itemId, out var r) ? r : null;
    }
}
