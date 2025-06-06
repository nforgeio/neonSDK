//-----------------------------------------------------------------------------
// FILE:        HeadlessComboboxOption.razor.cs
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
    public partial class HeadlessComboboxOption<TValue> : IDisposable
    {
        [CascadingParameter] public HeadlessCombobox<TValue> CascadedCombobox { get; set; } = default!;
        [CascadingParameter] public HeadlessComboboxOptions<TValue> CascadedOptions { get; set; } = default!;

        [Parameter] public RenderFragment<HeadlessComboboxOption<TValue>> ChildContent { get; set; }

        [Parameter] public bool IsEnabled { get; set; } = true;
        [Parameter] public bool IsVisible { get; set; } = true;

        [Parameter] public TValue Value { get; set; }
        [Parameter] public string SearchValue { get; set; } = "";

        [Parameter] public string Id { get; set; } = HtmlElement.GenerateId();
        [Parameter] public string TagName { get; set; } = "li";

        [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }

        public bool IsSelected => EqualityComparer<TValue>.Default.Equals(Value, Combobox.Value);
        public bool IsActive => Combobox.IsActiveOption(this);
        public HeadlessCombobox<TValue> Combobox { get; set; } = default!;
        public HeadlessComboboxOptions<TValue> Options { get; set; } = default!;

        public override Task SetParametersAsync(ParameterView parameters)
        {
            parameters.SetParameterProperties(this);

            ValidateAndSetCombobox();
            ValidateAndSetOptions();

            return base.SetParametersAsync(ParameterView.Empty);
        }
        [MemberNotNull(nameof(Options), nameof(CascadedOptions))]
        private void ValidateAndSetOptions()
        {
            if (Options == null)
            {
                if (CascadedOptions == null)
                    throw new InvalidOperationException($"You must use {nameof(HeadlessComboboxOption<TValue>)} inside an {nameof(HeadlessComboboxOptions<TValue>)}.");

                Options = CascadedOptions;
            }
            else if (CascadedOptions != Options)
            {
                throw new InvalidOperationException($"{nameof(HeadlessComboboxOption<TValue>)} does not support changing the {nameof(HeadlessComboboxOptions<TValue>)} dynamically.");
            }
        }
        [MemberNotNull(nameof(Combobox), nameof(CascadedCombobox))]
        private void ValidateAndSetCombobox()
        {
            if (Combobox == null)
            {
                if (CascadedCombobox == null)
                    throw new InvalidOperationException($"You must use {nameof(HeadlessComboboxOption<TValue>)} inside an {nameof(HeadlessComboboxOptions<TValue>)}.");

                Combobox = CascadedCombobox;
            }
            else if (CascadedCombobox != Combobox)
            {
                throw new InvalidOperationException($"{nameof(HeadlessComboboxOption<TValue>)} does not support changing the {nameof(HeadlessCombobox<TValue>)} dynamically.");
            }
        }

        /// <inheritdoc/>
        protected override void OnInitialized()
        {
            Combobox.RegisterOption(this);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Combobox.UnregisterOption(this);
        }

        private async Task HandleClick()
        {
            if (!IsEnabled) return;

            Combobox.CurrentValue = Value;

            await Combobox.Close();
        }
        private void HandleFocus(EventArgs e)
        {
            if (IsEnabled)
            {
                Combobox.GoToOption(this);
                return;
            }

            Combobox.GoToOption(ComboboxFocus.Nothing);
        }
        protected async Task HandleMouseEnter(MouseEventArgs e)
        {
            if (!IsEnabled) return;
            if (Combobox.State == ComboboxState.Closed) return;

            await Combobox.OptionsFocusAsync();
            if (IsActive) return;
            Combobox.GoToOption(this);
        }
        protected void HandleMouseLeave(MouseEventArgs e)
        {
            if (!IsEnabled) return;
            if (!IsActive) return;
            Combobox.GoToOption(ComboboxFocus.Nothing);
        }
    }
}
