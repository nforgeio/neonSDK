// -----------------------------------------------------------------------------
// FILE:        Components/LayoutGroup.razor.cs
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
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;

using Neon.Motion.Models;

namespace Neon.Motion.Components
{
    /// <summary>
    /// Coordinates FLIP layout animations across a group of <see cref="LayoutGroupItem"/>
    /// children — analogous to <c>LayoutGroup</c> in motion-react.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Wrap a dynamic list of <see cref="LayoutGroupItem"/> children inside a
    /// <c>LayoutGroup</c>. Before changing the list (e.g. reordering, adding, or removing
    /// items), call <see cref="CaptureAsync"/> to snapshot each item's position.
    /// After the list change causes a Blazor re-render each <see cref="LayoutGroupItem"/>
    /// automatically FLIP-animates from its captured position to its new position.
    /// </para>
    /// <example>
    /// <code>
    /// &lt;LayoutGroup @ref="_group" Transition="@(new AnimateOptions { Duration = 0.4 })"&gt;
    ///     @foreach (var item in _items)
    ///     {
    ///         &lt;LayoutGroupItem ItemId="@item.Id"&gt;
    ///             &lt;div&gt;@item.Name&lt;/div&gt;
    ///         &lt;/LayoutGroupItem&gt;
    ///     }
    /// &lt;/LayoutGroup&gt;
    ///
    /// async Task Shuffle()
    /// {
    ///     await _group.CaptureAsync();      // snapshot positions BEFORE change
    ///     _items = _items.OrderBy(_ => Random.Shared.Next()).ToList();
    ///     // re-render triggers each LayoutGroupItem to FLIP-animate automatically
    /// }
    /// </code>
    /// </example>
    /// </remarks>
    public partial class LayoutGroup : ComponentBase
    {
        private readonly LayoutGroupContext _context = new();

        // ------------------------------------------------------------------ //
        // Parameters
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Timing options applied to every item's FLIP animation within this group.
        /// </summary>
        [Parameter]
        public AnimateOptions Transition { get; set; }

        /// <summary>Child content — should consist of <see cref="LayoutGroupItem"/> components.</summary>
        [Parameter]
        public RenderFragment ChildContent { get; set; }

        // ------------------------------------------------------------------ //
        // Lifecycle
        // ------------------------------------------------------------------ //

        protected override void OnParametersSet()
        {
            _context.Transition = Transition;
        }

        // ------------------------------------------------------------------ //
        // Public API
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Snapshots the current bounding rect of every registered
        /// <see cref="LayoutGroupItem"/>. Call this <em>before</em> modifying the list.
        /// </summary>
        public Task CaptureAsync() => _context.CaptureAsync();
    }
}
