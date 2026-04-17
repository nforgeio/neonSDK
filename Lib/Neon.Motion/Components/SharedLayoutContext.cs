// -----------------------------------------------------------------------------
// FILE:        Components/SharedLayoutContext.cs
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

using Neon.Motion.Models;

namespace Neon.Motion.Components
{
    /// <summary>
    /// Cascading context shared between a <see cref="SharedLayout"/> and its
    /// <see cref="SharedLayoutItem"/> descendants. Propagates the ambient
    /// <see cref="Transition"/> so each item can fall back to the group's
    /// default timing when it doesn't specify its own.
    /// </summary>
    /// <remarks>
    /// Rect tracking — which used to live here — now lives entirely in the
    /// JavaScript side (<c>motion-interop.js</c>) so the measurement and
    /// animation start can happen in the same frame as the DOM mutation,
    /// without a Blazor Server round-trip. See <see cref="SharedLayoutItem"/>
    /// for the full flow.
    /// </remarks>
    public sealed class SharedLayoutContext
    {
        internal AnimateOptions Transition { get; set; }
    }
}
