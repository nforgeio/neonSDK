//-----------------------------------------------------------------------------
// FILE:        SearchAssistant.cs
// CONTRIBUTOR: Marcus Bowyer
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
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Neon.Tailwind
{
    public class SearchAssistant : IDisposable
    {
        public int DebouceTimeout { get; set; } = 750;
        public string SearchQuery { get; private set; } = "";

        public event EventHandler OnChange;

        private CancellationTokenSource cts = new CancellationTokenSource();
        private Task debounceTask;

        public async Task SearchAsync(string key)
        {
            SearchQuery += key;
            OnChange?.Invoke(this, EventArgs.Empty);

            await DebounceAsync();
        }

        public void ClearSearch()
        {
            SearchQuery = "";
            OnChange?.Invoke(this, EventArgs.Empty);
        }

        private async Task DebounceAsync()
        {
            cts.Cancel();

            cts = new CancellationTokenSource();

            debounceTask = Task.Delay(DebouceTimeout, cts.Token);
            
            await debounceTask;

            ClearSearch();
        }

        public void Dispose() => ClearSearch();
    }
}
