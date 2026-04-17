// -----------------------------------------------------------------------------
// FILE:        InViewCallbackHelper.cs
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

using Microsoft.JSInterop;

namespace Neon.Motion
{
    /// <summary>
    /// Helper that receives JS-invoked callbacks from a Motion <c>inView()</c> call
    /// and forwards them to .NET delegates.
    /// </summary>
    internal class InViewCallbackHelper
    {
        private readonly Func<string, Task> _onEnter;
        private readonly Func<string, Task> _onLeave;

        public InViewCallbackHelper(Func<string, Task> onEnter, Func<string, Task> onLeave = null)
        {
            _onEnter = onEnter;
            _onLeave = onLeave;
        }

        [JSInvokable("OnEnterView")]
        public Task OnEnterViewAsync(string inViewId) => _onEnter?.Invoke(inViewId) ?? Task.CompletedTask;

        [JSInvokable("OnLeaveView")]
        public Task OnLeaveViewAsync(string inViewId) => _onLeave?.Invoke(inViewId) ?? Task.CompletedTask;
    }
}
