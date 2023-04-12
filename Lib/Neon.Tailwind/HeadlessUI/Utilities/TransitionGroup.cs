//-----------------------------------------------------------------------------
// FILE:	    TransitionGroup.cs
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
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

using Neon.Tasks;

namespace Neon.Tailwind
{
    public class TransitionGroup : ComponentBase
    {
        public List<Transition> Transitions { get; private set; } = new List<Transition>();

        [Parameter] 
        public RenderFragment ChildContent { get; set; } = default!;

        [Parameter]
        public bool Show { get; set; } = true;

        [Parameter]
        public bool SuppressInitial { get; set; } = false;

        private TransitionState? state { get; set; }
        public TransitionState State
        {
            get
            {
                if (state.HasValue)
                {
                    return state.Value;
                }

                return TransitionState.Visible;
            }
            set
            {
                state = value;
            }
        }
        public event Action ChildHasChanged;
        public async Task NotifyChildChangedAsync() 
        {
            await SyncContext.Clear;
            
            if (ChildHasChanged != null) 
            { 
                await InvokeAsync(ChildHasChanged);
            }

            await InvokeAsync(StateHasChanged);
        }

        public void RegisterTransition(Transition transition)
        {
            if (Transitions.Contains(transition)) return;
            Transitions.Add(transition);
        }

        public void UnRegisterTransition(Transition transition)
        {
            Transitions.Remove(transition);
        }

        public async Task<Transition> GetChildAsync(string transitionId)
        {
            await SyncContext.Clear;
            
            var transition = Transitions.Where(transition => transition.Id == transitionId).FirstOrDefault();

            return transition;
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (Show || !Transitions.All(t => t.State == TransitionState.Hidden))
            {
                builder.OpenComponent<CascadingValue<TransitionGroup>>(0);
                builder.AddMultipleAttributes(1, new Dictionary<string, object>
                {
                    ["Value"] = this,
                    ["ChildContent"] = ChildContent
                });
                builder.CloseComponent();
            }
        }

        /// <summary>
        /// Opens the transition.
        /// </summary>
        /// <returns></returns>
        public async Task EnterAsync()
        {
            await SyncContext.Clear;
            
            if (State == TransitionState.Visible || State == TransitionState.Entering)
            {
                return;
            }

            State = TransitionState.Entering;

            await Task.WhenAll(Transitions.Select(transition => transition.EnterAsync()).ToList());

            State = TransitionState.Visible;
        }

        /// <summary>
        /// Closes the transition.
        /// </summary>
        /// <returns></returns>
        public async Task LeaveAsync()
        {
            await SyncContext.Clear;
            
            if (State == TransitionState.Leaving || State == TransitionState.Hidden)
            {
                return;
            }

            State = TransitionState.Leaving;

            await Task.WhenAll(Transitions.Select(transition => transition.LeaveAsync()).ToList());

            State = TransitionState.Hidden;
        }

        /// <summary>
        /// Toggles the transition.
        /// </summary>
        /// <returns></returns>
        public async Task ToggleAsync()
        {
            await SyncContext.Clear;
           
            if (State == TransitionState.Visible || State == TransitionState.Entering)
                await LeaveAsync();
            else
                await EnterAsync();
        }
    }
}
