//-----------------------------------------------------------------------------
// FILE:	    AnimatePresenseDemo.razor.cs
// CONTRIBUTOR: Marcus Bowyer
// COPYRIGHT:  	Copyright © 2005-2023 by NEONFORGE LLC.  All rights reserved.
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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;

using Neon.Tailwind;
using Neon.Tasks;

namespace TestTailwind.Components
{
    public partial class AnimatePresenseDemo : ComponentBase
    {
        private TransitionGroup               transitionGroup;
        private bool                          isShowing       = true;
        private SortedDictionary<int, string> messages        = new SortedDictionary<int, string>();
        private int                           messageId       = 0;
        protected override void OnInitialized()
        {
            if (transitionGroup != null)
            {
                transitionGroup.ChildHasChanged += StateHasChanged;
            }
            base.OnInitialized();
        }

        private async Task AddEntryAsync()
        {
            await SyncContext.Clear;

            var id = messageId;
            messageId++;
            messages.Add(id, $"Hello, Tailwind! {id}");
            await InvokeAsync(StateHasChanged);
            await Task.Delay(3000);
            var transition = await transitionGroup.GetChildAsync(id.ToString());
            await transition.LeaveAsync();
            messages.Remove(id);
            await InvokeAsync(StateHasChanged);
        }

        public class Message
        {
            public string Id { get; set; }
            public string MessageString { get; set; }
        }
    }
}
