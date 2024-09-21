//-----------------------------------------------------------------------------
// FILE:        Transition.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

using Neon.Tailwind.HeadlessUI.Utilities;
using Neon.Tasks;

namespace Neon.Tailwind
{
    public class Transition : ComponentBase
    {
        /// <summary>
        /// The parent <see cref="TransitionGroup"/>.
        /// </summary>
        [CascadingParameter] 
        public TransitionGroup TransitionGroup { get; set; }

        /// <summary>
        /// The tag ID.
        /// </summary>
        [Parameter]
        public string Id { get; set; } = GenerateId();

        /// <summary>
        /// The child content of the transition.
        /// </summary>
        [Parameter] 
        public RenderFragment<string> ChildContent { get; set; }

        /// <summary>
        /// Classes to add to the transitioning element during the entire enter phase.
        /// </summary>
        [Parameter]
        public string Enter { get; set; }

        /// <summary>
        /// The starting point to enter from.
        /// </summary>
        [Parameter]
        public string EnterFrom { get; set; }

        /// <summary>
        /// The ending point to enter to.
        /// </summary>
        [Parameter]
        public string EnterTo { get; set; }

        /// <summary>
        /// Classes to add to the transitioning element during the entire leave phase.
        /// </summary>
        [Parameter]
        public string Leave { get; set; }

        /// <summary>
        /// Classes to add to the transitioning element before the leave phase starts.
        /// </summary>
        [Parameter]
        public string LeaveFrom { get; set; }

        /// <summary>
        /// Classes to add to the transitioning element immediately after the leave phase starts.
        /// </summary>
        [Parameter]
        public string LeaveTo { get; set; }

        /// <summary>
        /// Whether the transition should run on initial mount.
        /// </summary>
        [Parameter]
        public bool? Show { get; set; }

        [Parameter]
        public bool? SuppressInitial { get; set; }

        /// <summary>
        /// Callback that is fired when the transition is starting.
        /// </summary>
        [Parameter] 
        public EventCallback<bool> BeginTransition { get; set; }

        /// <summary>
        /// Callback that is fired when the transition has ended.
        /// </summary>
        [Parameter] 
        public EventCallback<bool> EndTransition { get; set; }

        /// <summary>
        /// Additional attributes.
        /// </summary>
        [Parameter(CaptureUnmatchedValues = true)]
        public IReadOnlyDictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();

        public event Action OnTransitionChange;
        public bool ChildIsVisible => (State != Tailwind.TransitionState.Hidden);

        private bool disposed = false;

        private TaskCompletionSource<bool> hasRendered = new TaskCompletionSource<bool>();
        private TaskCompletionSource<bool> childRendered = new TaskCompletionSource<bool>();

        /// <summary>
        /// Generates an ID.
        /// </summary>
        /// <returns></returns>
        public static string GenerateId() => Guid.NewGuid().ToString("N");
        private async Task NotifyTransitionChangedAsync()
        {
            childRendered = new TaskCompletionSource<bool>();

            await InvokeAsync(StateHasChanged);

            if (OnTransitionChange != null)
            {
                await InvokeAsync(OnTransitionChange);
            }

            if (TransitionGroup != null)
            {
                await TransitionGroup?.NotifyChildChangedAsync();
            }
        }

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

        private TransitionState? state { get; set; }
        public string CurrentCssClass { get; private set; }
        public string LastCssClass { get; private set; }
        public string ClassAttributes { get; private set; }

        private bool transitionStarted;
        private bool stateChangeRequested;
        private int enterDuration;
        private int leaveDuration;
        private bool show
        {
            get
            {
                if (TransitionGroup != null)
                {
                    return TransitionGroup.Show;
                }

                if (Show.HasValue)
                {
                    return Show.Value;
                }

                return true;
            }
        }

        private bool suppressInitial
        {
            get
            {
                if (SuppressInitial.HasValue)
                {
                    return SuppressInitial.Value;
                }

                if (TransitionGroup != null)
                {
                    return TransitionGroup.SuppressInitial;
                }

                return false;
            }
        }

        private bool isInitialized = false;

        /// <inheritdoc/>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            TransitionGroup?.RegisterTransition(this);
            OnTransitionChange += StateHasChanged;
        }

        protected override async Task OnParametersSetAsync()
        {
            await SyncContext.Clear;

            await base.OnParametersSetAsync();

            if (!stateChangeRequested)
            {
                return;
            }

            stateChangeRequested = false;

            GetClassAttributes();

            string durationPattern = @"duration[-[]+([0-9]+)[a-z\]]*";

            if (!string.IsNullOrEmpty(Enter))
            {
                var match = Regex.Match(Enter, durationPattern);
                if (match.Success)
                {
                    enterDuration = int.Parse(match.Groups[1].Value);
                }
                else
                {
                    enterDuration = 0;
                }
            }

            if (!string.IsNullOrEmpty(Leave))
            {
                var match = Regex.Match(Leave, durationPattern);
                if (match.Success)
                {
                    leaveDuration = int.Parse(match.Groups[1].Value);
                }
                else
                {
                    leaveDuration = 0;
                }
            }

            if (!isInitialized)
            {
                if (show && suppressInitial)
                {
                    State = TransitionState.Visible;
                }
                else
                {
                    State = TransitionState.Hidden;
                }

                await ClearCurrentTransitionAsync();
            }
            else
            {
                if (show)
                {
                    await EnterAsync();
                }
                else
                {
                    await LeaveAsync();
                }
            }

            isInitialized = true;
        }

        /// <inheritdoc/>
        public override async Task SetParametersAsync(ParameterView parameters)
        {
            await SyncContext.Clear;

            var currentShowValue  = Show;

            parameters.SetParameterProperties(this);

            Show                 = show;
            stateChangeRequested = currentShowValue != show;

            await base.SetParametersAsync(ParameterView.Empty);
        }

        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);
        }

        /// <inheritdoc/>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await SyncContext.Clear;

            await base.OnAfterRenderAsync(firstRender);

            if (!hasRendered.Task.IsCompleted)
            {
                hasRendered.TrySetResult(true);
            }

            if (firstRender)
            {
                if (!suppressInitial && show)
                {
                    await EnterAsync();
                }
            }

            if (TransitionHasStartedOrCompleted())
            {
                return;
            }
        }
        
        /// <inheritdoc/>
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenComponent<CascadingValue<Transition>>(0);
            builder.AddAttribute(1, "Value", this);
            builder.AddMultipleAttributes(2, Attributes.Select(a => new KeyValuePair<string, object>(a.Key, a.Value)));

            if (State != TransitionState.Hidden)
            {
                if (CurrentCssClass != LastCssClass)
                {
                    LastCssClass = CurrentCssClass;
                }

                RenderFragment content = b =>
                {
                    b.OpenComponent<RenderNotifier>(0);
                    b.AddComponentParameter(1, nameof(RenderNotifier.Transition), this);
                    b.AddComponentParameter(2, nameof(RenderNotifier.ChildContent), ChildContent?.Invoke(CurrentCssClass));
                    b.CloseComponent();
                };

                builder.AddAttribute(3, nameof(ChildContent), content);
            }
            builder.SetKey(Id);

            builder.CloseComponent();
        }

        /// <summary>
        /// Opens the transition.
        /// </summary>
        /// <returns></returns>
        public async Task EnterAsync()
        {
            await SyncContext.Clear;

            if (suppressInitial && !isInitialized)
            {
                State = TransitionState.Visible;
                await ClearCurrentTransitionAsync();
                isInitialized = true;
                return;
            }

            if (isInitialized)
            {
                if (State == TransitionState.Visible || State == TransitionState.Entering)
                {
                    return;
                }
            }
            
            await BeginTransition.InvokeAsync();
            await InitializeEnteringAsync();
            await StartTransition();
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

            await InitializeLeavingAsync();
            await StartTransition();
            await EndTransition.InvokeAsync();
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

        private void GetClassAttributes()
        {
            if (Attributes != null)
            {
                try
                {
                    if (Attributes.TryGetValue("class", out var classAttributes))
                    {
                        ClassAttributes = (string)classAttributes;
                    }
                }
                catch
                {

                }
            }
        }

        private async Task StartTransition()
        {
            await SyncContext.Clear;

            transitionStarted = true;

            var cssClass = new StringBuilder();

            if (!string.IsNullOrEmpty(ClassAttributes))
            {
                cssClass.Append(ClassAttributes);
            }

            var tasks = new List<Task>()
            {
                hasRendered.Task
            };

            if (State == TransitionState.Visible || State == TransitionState.Entering)
            {
                tasks.Add(childRendered.Task);
            }

            await Task.WhenAll(tasks);

            switch (State)
            {
                case TransitionState.Entering:

                    cssClass.Append($" {Enter}");
                    cssClass.Append($" {EnterTo}");
                    break;

                case TransitionState.Leaving:
                default:

                    cssClass.Append($" {Leave}");
                    cssClass.Append($" {LeaveTo}");
                    break;
            }

            CurrentCssClass = cssClass.ToString();

            await NotifyTransitionChangedAsync();

            switch (State)
            {
                case TransitionState.Entering:

                    if (enterDuration <= 0)
                    {
                        return;
                    }
                    await Task.Delay(enterDuration);

                    break;

                case TransitionState.Leaving:
                default:

                    if (leaveDuration <= 0)
                    {
                        return;
                    }

                    await Task.Delay(leaveDuration);
                    
                    break;
            }

            switch (State)
            {
                case TransitionState.Entering:
                case TransitionState.Visible:

                    State = TransitionState.Visible;

                    break;

                case TransitionState.Hidden:
                case TransitionState.Leaving:

                    State = TransitionState.Hidden;

                    break;

                default:

                    break;

            }

            await ClearCurrentTransitionAsync();
        }

        private bool TransitionHasStartedOrCompleted()
        {
            return State == TransitionState.Visible || State == TransitionState.Hidden || transitionStarted;
        }

        private async Task ClearCurrentTransitionAsync()
        {
            await SyncContext.Clear;
            
            CurrentCssClass = ClassAttributes;
            transitionStarted = false;

            await NotifyTransitionChangedAsync();
        }

        private async Task InitializeEnteringAsync()
        {
            await SyncContext.Clear;

            await ClearCurrentTransitionAsync();

            if (enterDuration == 0)
            {
                State = TransitionState.Visible;
                return;
            }

            string attributeClass = string.Empty;
            if (Attributes != null)
            {
                if (Attributes.TryGetValue("class", out var classAttributes))
                {
                    attributeClass = (string)classAttributes;
                }
            }

            State = TransitionState.Entering;

            var cssClass = new StringBuilder();

            if (!string.IsNullOrEmpty(ClassAttributes))
            {
                cssClass.Append(ClassAttributes);
            }
            cssClass.Append($" {Enter}");
            cssClass.Append($" {EnterFrom}");

            CurrentCssClass = cssClass.ToString();

            await NotifyTransitionChangedAsync();
        }

        private async Task InitializeLeavingAsync()
        {
            await SyncContext.Clear;

            await ClearCurrentTransitionAsync();

            if (leaveDuration == 0)
            {
                State = TransitionState.Leaving;
                return;
            }

            if (suppressInitial && !isInitialized)
            {
                State = TransitionState.Hidden;
                isInitialized = true;
                return;
            }

            State = TransitionState.Leaving;

            var cssClass = new StringBuilder();

            if (!string.IsNullOrEmpty(ClassAttributes))
            {
                cssClass.Append(ClassAttributes);
            }
            cssClass.Append($" {Leave}");
            cssClass.Append($" {LeaveFrom}");

            CurrentCssClass = cssClass.ToString();

            await NotifyTransitionChangedAsync();
        }

        public async Task NotifyRenderAsync()
        {
            await SyncContext.Clear;

            if (!childRendered.Task.IsCompleted)
            {
                childRendered.TrySetResult(true);

                await InvokeAsync(StateHasChanged);
            }
        }
    }
}
