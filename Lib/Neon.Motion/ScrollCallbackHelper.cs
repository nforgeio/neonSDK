// -----------------------------------------------------------------------------
// FILE:        ScrollCallbackHelper.cs
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
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.JSInterop;

using Neon.Motion.Models;

namespace Neon.Motion
{
    /// <summary>
    /// Helper that receives JS-invoked callbacks from a Motion <c>scroll()</c> call
    /// and forwards them to .NET delegates.
    /// </summary>
    internal class ScrollCallbackHelper
    {
        private readonly Func<string, double, ScrollInfo, Task> _onScroll;

        public ScrollCallbackHelper(Func<string, double, ScrollInfo, Task> onScroll)
        {
            _onScroll = onScroll;
        }

        [JSInvokable("OnScroll")]
        public Task OnScrollAsync(string scrollId, double progress, string infoJson)
        {
            if (_onScroll == null) return Task.CompletedTask;

            ScrollInfo info = null;

            if (!string.IsNullOrEmpty(infoJson))
            {
                try { info = JsonSerializer.Deserialize<ScrollInfo>(infoJson); }
                catch { /* ignore deserialization errors */ }
            }

            return _onScroll.Invoke(scrollId, progress, info);
        }
    }
}
