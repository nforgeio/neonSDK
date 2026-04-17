// -----------------------------------------------------------------------------
// FILE:        Components/MotionDiv.razor.cs
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

namespace Neon.Motion.Components
{
    /// <summary>
    /// A <c>&lt;div&gt;</c> element with built-in Motion animation support.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Set the <c>Animate</c> parameter to define animation target values and the
    /// <c>Transition</c> parameter to control timing. The animation starts automatically
    /// after the element's first render.
    /// </para>
    /// <para>
    /// Use the <c>AnimateAsync()</c>, <c>PauseAsync()</c>, <c>PlayAsync()</c>,
    /// and <c>CancelAsync()</c> methods for imperative control.
    /// </para>
    /// <example>
    /// <code>
    /// &lt;MotionDiv @ref="box"
    ///            Animate="@(new AnimationValues().Opacity(0, 1).X(-20, 0))"
    ///            Transition="@(new AnimateOptions { Duration = 0.6, Ease = "easeOut" })"
    ///            class="my-box"&gt;
    ///     Hello!
    /// &lt;/MotionDiv&gt;
    /// </code>
    /// </example>
    /// </remarks>
    public partial class MotionDiv : MotionElementBase
    {
    }
}
