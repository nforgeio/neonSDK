//-----------------------------------------------------------------------------
// FILE:        HeadlessComboboxOptions.razor.cs
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
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

using Neon.Blazor;

namespace Neon.Tailwind
{
    public partial class HeadlessComboboxOptions<TValue> : ComponentBase, IAsyncDisposable
    {
        [CascadingParameter] public HeadlessCombobox<TValue> CascadedCombobox { get; set; } = default!;

        [Parameter] public RenderFragment ChildContent { get; set; }

        [Parameter] public bool Static { get; set; }
        [Parameter] public string TagName { get; set; } = "ul";
        [Parameter] public string Id { get; set; } = HtmlElement.GenerateId();

        [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }

        private HtmlElement rootElement;
        private KeyDownEventHandler keyDownEventHandler;

        protected HeadlessCombobox<TValue> Combobox { get; set; } = default!;

        [MemberNotNull(nameof(Combobox), nameof(CascadedCombobox))]
        public override Task SetParametersAsync(ParameterView parameters)
        {
            parameters.SetParameterProperties(this);

            if (Combobox == null)
            {
                if (CascadedCombobox == null)
                    throw new InvalidOperationException($"You must use {nameof(HeadlessComboboxOptions<TValue>)} inside an {nameof(HeadlessCombobox<TValue>)}.");

                Combobox = CascadedCombobox;
            }
            else if (CascadedCombobox != Combobox)
            {
                throw new InvalidOperationException($"{nameof(HeadlessComboboxOptions<TValue>)} does not support changing the {nameof(HeadlessCombobox<TValue>)} dynamically.");
            }

            return base.SetParametersAsync(ParameterView.Empty);
        }
        protected override void OnInitialized() => Combobox.RegisterOptions(this);
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (keyDownEventHandler != null)
                await keyDownEventHandler.RegisterElement(rootElement!);
        }

        public async Task HandleKeyDown(KeyboardEventArgs eventArgs)
        {
            string key = eventArgs.Key;
            if (string.IsNullOrEmpty(key)) return;

            switch (key)
            {
                case KeyboardKey.Enter:
                    Combobox.SetActiveAsValue();
                    await Combobox.Close();
                    break;
                case KeyboardKey.ArrowDown:
                    Combobox.GoToOption(ComboboxFocus.Next);
                    break;
                case KeyboardKey.ArrowUp:
                    Combobox.GoToOption(ComboboxFocus.Previous);
                    break;
                case KeyboardKey.Home:
                case KeyboardKey.PageUp:
                    Combobox.GoToOption(ComboboxFocus.First);
                    break;
                case KeyboardKey.End:
                case KeyboardKey.PageDown:
                    Combobox.GoToOption(ComboboxFocus.Last);
                    break;
                case KeyboardKey.Escape:
                    await Combobox.Close();
                    break;
                case KeyboardKey.Tab:
                    await Combobox.Close(true);
                    break;
                default:
                    await Combobox.SearchAsync(key);
                    break;
            }
        }

        public ValueTask FocusAsync() => rootElement?.FocusAsync() ?? ValueTask.CompletedTask;

        public async ValueTask DisposeAsync()
        {
            if (keyDownEventHandler != null)
                await keyDownEventHandler.UnregisterElement(rootElement!);
        }

        public static implicit operator ElementReference(HeadlessComboboxOptions<TValue> element) => element?.rootElement ?? default!;
    }
}
