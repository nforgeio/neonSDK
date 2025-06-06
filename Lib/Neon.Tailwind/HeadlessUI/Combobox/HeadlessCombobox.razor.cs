// -----------------------------------------------------------------------------
// FILE:	    HeadlessCombobox.razor.cs
// CONTRIBUTOR: NEONFORGE Team
// COPYRIGHT:   Copyright © 2005-2024 by NEONFORGE LLC.  All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License").
// You may not use this file except in compliance with the License.
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
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;

namespace Neon.Tailwind
{
    public partial class HeadlessCombobox<TValue> : ComponentBase
    {
        [Parameter]
        public RenderFragment<HeadlessCombobox<TValue>> ChildContent { get; set; }

        [Parameter]
        public TValue Value { get; set; }

        [Parameter]
        public EventCallback<TValue> ValueChanged { get; set; }

        [Parameter]
        public EventCallback OnOpen { get; set; }

        [Parameter]
        public EventCallback OnClose { get; set; }

        [Parameter]
        public int DebouceTimeout { get; set; } = 350;

        private readonly List<HeadlessComboboxOption<TValue>> options = new();

        public ComboboxState State { get; protected set; } = ComboboxState.Closed;

        private HeadlessComboboxOption<TValue> activeOption;
        private ClickOffEventHandler clickOffEventHandler;
        private SearchAssistant searchAssistant;

        private HeadlessComboboxOptions<TValue> optionsElement;

        internal HeadlessComboboxInput<TValue> InputElement;
        public string SearchQuery => searchAssistant.SearchQuery;
        public TValue CurrentValue
        {
            get => Value;
            set
            {
                bool valueChanged = !EqualityComparer<TValue>.Default.Equals(Value, value);
                if (valueChanged)
                {
                    Value = value;
                    ValueChanged.InvokeAsync(value);
                    InputElement?.SetValueAsync(value);
                }
            }
        }

        public void RegisterOption(HeadlessComboboxOption<TValue> option)
        {
            options.Add(option);
        }
        public void UnregisterOption(HeadlessComboboxOption<TValue> option)
        {
            if (!options.Contains(option)) return;

            if (activeOption == option)
            {
                GoToOption(ComboboxFocus.Next);
            }
            options.Remove(option);
        }
        public bool IsActiveOption(HeadlessComboboxOption<TValue> option) => activeOption == option;
        public void GoToOption(HeadlessComboboxOption<TValue> option)
        {
            if (option != null && (!option.IsEnabled || !options.Contains(option))) option = null;
            if (activeOption == option) return;

            activeOption = option;
            StateHasChanged();
        }
        public void GoToOption(ComboboxFocus focus)
        {
            switch (focus)
            {
                case ComboboxFocus.First:
                    {
                        GoToOption(options.FirstOrDefault(mi => mi.IsEnabled));
                        break;
                    }
                case ComboboxFocus.Previous:
                    {
                        GoToOption(FindOptionBeforeActiveOption());
                        break;
                    }
                case ComboboxFocus.Next:
                    {
                        GoToOption(FindOptionAfterActiveOption());
                        break;
                    }
                case ComboboxFocus.Last:
                    {
                        activeOption = options.LastOrDefault(mi => mi.IsEnabled);
                        break;
                    }
                default:
                    {
                        GoToOption(null);
                        break;
                    }
            }
        }
        private HeadlessComboboxOption<TValue> FindOptionBeforeActiveOption()
        {
            var reversedMenuOptions = options.ToList();
            reversedMenuOptions.Reverse();
            bool foundTarget = false;
            var itemIndex = reversedMenuOptions.FindIndex(0, mi =>
            {
                if (mi == activeOption)
                {
                    foundTarget = true;
                    return false;
                }
                return foundTarget && mi.IsEnabled;
            });
            if (itemIndex != -1)
                return reversedMenuOptions[itemIndex];
            else
                return options.LastOrDefault(mi => mi.IsEnabled);
        }
        private HeadlessComboboxOption<TValue> FindOptionAfterActiveOption()
        {
            bool foundTarget = false;
            var itemIndex = options.FindIndex(0, mi =>
            {
                if (mi == activeOption)
                {
                    foundTarget = true;
                    return false;
                }
                return foundTarget && mi.IsEnabled;
            });
            if (itemIndex != -1)
                return options[itemIndex];
            else
                return options.FirstOrDefault(mi => mi.IsEnabled);
        }


        public void RegisterOptions(HeadlessComboboxOptions<TValue> options)
            => optionsElement = options;



        private bool shouldFocus;
        public async Task Toggle()
        {
            if (State == ComboboxState.Closed)
                await Open();
            else
                await Close();
        }
        public async Task Close(bool suppressFocus = false)
        {
            if (State == ComboboxState.Closed) return;
            State = ComboboxState.Closed;
            await OnClose.InvokeAsync();
            activeOption = null;
            shouldFocus = !suppressFocus;
            StateHasChanged();
        }
        public async Task Open()
        {
            if (State == ComboboxState.Open) return;
            State = ComboboxState.Open;
            await OnOpen.InvokeAsync();
            shouldFocus = true;
            StateHasChanged();
        }
        public ValueTask OptionsFocusAsync() => optionsElement?.FocusAsync() ?? ValueTask.CompletedTask;
        public void SetActiveAsValue() => CurrentValue = activeOption is null ? default : activeOption.Value;

        public Task HandleClickOff() => Close();
        private void HandleSearchChange(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(SearchQuery))
            {
                var item = options.FirstOrDefault(mi => (mi.SearchValue ?? "").StartsWith(SearchQuery, StringComparison.OrdinalIgnoreCase) && mi.IsEnabled);
                GoToOption(item);
            }
        }
        public async Task SearchAsync(string key)
        {
            await searchAssistant.SearchAsync(key);
        }


        public void Dispose() => searchAssistant.Dispose();

    }
}
